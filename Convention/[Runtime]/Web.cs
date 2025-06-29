using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Convention
{
    [Serializable]
    public sealed class ToolURL
    {
        private string url;
        private static readonly HttpClient httpClient = new();
        private object data;

        public ToolURL(string url)
        {
            this.url = url;
        }

        public override string ToString()
        {
            return this.url;
        }

        #region HTTP Methods

        public async Task<bool> GetAsync(Action<HttpResponseMessage> callback)
        {
            if (!IsValid)
                return false;

            try
            {
                var response = await httpClient.GetAsync(this.url);
                callback(response);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                callback(null);
                return false;
            }
        }

        public bool Get(Action<HttpResponseMessage> callback)
        {
            return GetAsync(callback).GetAwaiter().GetResult();
        }

        public async Task<bool> PostAsync(Action<HttpResponseMessage> callback, Dictionary<string, string> formData = null)
        {
            if (!IsValid)
                return false;

            try
            {
                HttpContent content = null;
                if (formData != null)
                {
                    content = new FormUrlEncodedContent(formData);
                }

                var response = await httpClient.PostAsync(this.url, content);
                callback(response);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                callback(null);
                return false;
            }
        }

        public bool Post(Action<HttpResponseMessage> callback, Dictionary<string, string> formData = null)
        {
            return PostAsync(callback, formData).GetAwaiter().GetResult();
        }

        #endregion

        #region URL Properties

        public string FullURL => this.url;
        public static implicit operator string(ToolURL data) => data.FullURL;

        public string GetFullURL()
        {
            return this.url;
        }

        public string GetFilename()
        {
            if (string.IsNullOrEmpty(this.url))
                return "";

            Uri uri = new Uri(this.url);
            string path = uri.AbsolutePath;
            return Path.GetFileName(path);
        }

        public string GetExtension()
        {
            string filename = GetFilename();
            if (string.IsNullOrEmpty(filename))
                return "";

            return Path.GetExtension(filename);
        }

        public bool ExtensionIs(params string[] extensions)
        {
            string el = GetExtension().ToLower();
            string eln = el.Length > 1 ? el[1..] : null;
            foreach (string extension in extensions)
                if (el == extension || eln == extension)
                    return true;
            return false;
        }

        #endregion

        #region Validation

        public bool IsValid => ValidateURL();

        public bool ValidateURL()
        {
            if (string.IsNullOrEmpty(this.url))
                return false;

            return Uri.TryCreate(this.url, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static implicit operator bool(ToolURL url) => url.IsValid;

        #endregion

        #region Load Methods

        public async Task<string> LoadAsTextAsync()
        {
            if (!IsValid)
                return null;

            try
            {
                var response = await httpClient.GetAsync(this.url);
                if (response.IsSuccessStatusCode)
                {
                    this.data = await response.Content.ReadAsStringAsync();
                    return (string)this.data;
                }
            }
            catch
            {
                // 请求失败
            }

            return null;
        }

        public string LoadAsText()
        {
            return LoadAsTextAsync().GetAwaiter().GetResult();
        }

        public async Task<byte[]> LoadAsBinaryAsync()
        {
            if (!IsValid)
                return null;

            try
            {
                var response = await httpClient.GetAsync(this.url);
                if (response.IsSuccessStatusCode)
                {
                    this.data = await response.Content.ReadAsByteArrayAsync();
                    return (byte[])this.data;
                }
            }
            catch
            {
                // 请求失败
            }

            return null;
        }

        public byte[] LoadAsBinary()
        {
            return LoadAsBinaryAsync().GetAwaiter().GetResult();
        }

        public T LoadAsJson<T>()
        {
            string jsonText = LoadAsText();
            if (string.IsNullOrEmpty(jsonText))
                return default(T);

            try
            {
                T result = JsonSerializer.Deserialize<T>(jsonText);
                this.data = result;
                return result;
            }
            catch
            {
                return default(T);
            }
        }

        public async Task<T> LoadAsJsonAsync<T>()
        {
            string jsonText = await LoadAsTextAsync();
            if (string.IsNullOrEmpty(jsonText))
                return default(T);

            try
            {
                T result = JsonSerializer.Deserialize<T>(jsonText);
                this.data = result;
                return result;
            }
            catch
            {
                return default(T);
            }
        }

        #endregion

        #region Save Methods

        public void Save(string localPath = null)
        {
            if (IsText)
                SaveAsText(localPath);
            else if (IsJson)
                SaveAsJson(localPath);
            else
                SaveAsBinary(localPath);
        }

        public void SaveAsText(string localPath = null)
        {
            if (localPath == null)
            {
                localPath = Path.Combine(Path.GetTempPath(), GetFilename());
            }

            if (this.data is string text)
            {
                File.WriteAllText(localPath, text);
            }
        }

        public void SaveAsJson(string localPath = null)
        {
            if (localPath == null)
            {
                localPath = Path.Combine(Path.GetTempPath(), GetFilename());
            }

            if (this.data != null)
            {
                string jsonText = JsonSerializer.Serialize(this.data);
                File.WriteAllText(localPath, jsonText);
            }
        }

        public void SaveAsBinary(string localPath = null)
        {
            if (localPath == null)
            {
                localPath = Path.Combine(Path.GetTempPath(), GetFilename());
            }

            if (this.data is byte[] bytes)
            {
                File.WriteAllBytes(localPath, bytes);
            }
        }

        #endregion

        #region URL Types

        public bool IsText => ExtensionIs("txt", "html", "htm", "css", "js", "xml", "csv");
        public bool IsJson => ExtensionIs("json");
        public bool IsImage => ExtensionIs("jpg", "jpeg", "png", "gif", "bmp", "svg");
        public bool IsDocument => ExtensionIs("pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx");

        #endregion

        #region Operators

        public static ToolURL operator |(ToolURL left, string rightPath)
        {
            string baseUrl = left.GetFullURL();
            if (baseUrl.EndsWith('/'))
            {
                return new ToolURL(baseUrl + rightPath);
            }
            else
            {
                return new ToolURL(baseUrl + "/" + rightPath);
            }
        }

        public ToolURL Open(string url)
        {
            this.url = url;
            return this;
        }

        public async Task<ToolURL> DownloadAsync(string localPath = null)
        {
            if (!IsValid)
                return this;

            if (localPath == null)
            {
                localPath = Path.Combine(Path.GetTempPath(), GetFilename());
            }

            try
            {
                var response = await httpClient.GetAsync(this.url);
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(localPath, bytes);
                }
            }
            catch
            {
                // 下载失败
            }

            return this;
        }

        public ToolURL Download(string localPath = null)
        {
            return DownloadAsync(localPath).GetAwaiter().GetResult();
        }

        #endregion
    }
}

