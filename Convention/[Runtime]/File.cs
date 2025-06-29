using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Convention
{
    [Serializable]
    public sealed class ToolFile
    {
        private string FullPath;
        private FileSystemInfo OriginInfo;
        public FileStream OriginControlStream { get; private set; }
        public ToolFile(string path) 
        {
            FullPath = path;
            Refresh();
        }
        public override string ToString()
        {
            return this.FullPath;
        }

        #region Path

        public static implicit operator string(ToolFile data) => data.FullPath;
        public string GetFullPath()
        {
            return this.FullPath;
        }
        public string GetName(bool is_ignore_extension = false)
        {
            return this.FullPath[..(
                (this.FullPath.Contains('.') && is_ignore_extension)
                    ? this.FullPath.LastIndexOf('.')
                    : ^0
                )]
                    [..(
                (this.FullPath[^1] == '/' || this.FullPath[^1] == '\\')
                    ? ^1
                    : ^0
                )];
        }
        public string GetExtension()
        {
            if (IsDir())
                return "";
            return this.FullPath[(
                (this.FullPath.Contains('.'))
                    ? this.FullPath.LastIndexOf('.')
                    : ^0
                )..];
        }

        #endregion

        #region Exists

        public bool Exists() => File.Exists(FullPath) || Directory.Exists(FullPath);

        public static implicit operator bool(ToolFile file) => file.Exists();

        #endregion

        public ToolFile Refresh()
        {
            if (Exists() == false)
                OriginInfo = null;
            else if (IsDir())
                OriginInfo = new DirectoryInfo(FullPath);
            else
                OriginInfo = new FileInfo(FullPath);
            return this;
        }

        #region Load

        public T LoadAsRawJson<T>()
        {
            return JsonSerializer.Deserialize<T>(LoadAsText());
        }
        public T LoadAsJson<T>(string key = "data")
        {
            return EasySave.EasySave.Load<T>(key, FullPath);
        }
        public string LoadAsText()
        {
            if (IsFile() == false)
                throw new InvalidOperationException("Target is not a file");
            string result = "";
            using (var fs = (this.OriginInfo as FileInfo).OpenText())
            {
                result = fs.ReadToEnd();
            }
            return result;
        }
        public byte[] LoadAsBinary()
        {
            if (IsFile() == false)
                throw new InvalidOperationException("Target is not a file");
            var file = this.OriginInfo as FileInfo;
            const int BlockSize = 1024;
            long FileSize = file.Length;
            byte[] result = new byte[FileSize];
            long offset = 0;
            using (var fs = file.OpenRead())
            {
                fs.ReadAsync(result[(int)(offset)..(int)(offset + BlockSize)], 0, (int)(offset + BlockSize) - (int)(offset));
                offset += BlockSize;
                offset = System.Math.Min(offset, FileSize);
            }
            return result;
        }

        #endregion

        #region Save

        public void SaveAsRawJson<T>(T data)
        {
            SaveAsText(JsonSerializer.Serialize<T>(data));
        }
        public void SaveAsJson<T>(T data, string key)
        {
            EasySave.EasySave.Save(key, data,FullPath);
        }
        private void SaveAsText(string data)
        {
            if (OriginControlStream != null && OriginControlStream.CanWrite)
            {
                using var sw = new StreamWriter(OriginControlStream);
                sw.Write(data);
                sw.Flush();
            }
            else
            {
                using var fs = new FileStream(FullPath, FileMode.CreateNew, FileAccess.Write);
                using var sw = new StreamWriter(fs);
                sw.Write(data);
                sw.Flush();
            }
        }
        public static void SaveDataAsBinary(string path,  byte[] outdata, FileStream Stream = null)
        {
            if (Stream != null && Stream.CanWrite)
            {
                Stream.Write(outdata, 0, outdata.Length);
                Stream.Flush();
            }
            else
            {
                using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
                fs.Write(outdata, 0, outdata.Length);
                fs.Flush();
            }
        }

        public void SaveAsBinary(byte[] data)
        {
            SaveDataAsBinary(FullPath, data, OriginControlStream);
        }

        #endregion

        #region IsFileType

        public bool IsDir()
        {
            if (Exists())
            {
                return Directory.Exists(this.FullPath);
            }
            return this.FullPath[^1] == '\\' || this.FullPath[^1] == '/';
        }

        public bool IsFile()
        {
            return !IsDir();
        }

        public bool IsFileEmpty()
        {
            if (IsFile())
                return (this.OriginInfo as FileInfo).Length == 0;
            throw new InvalidOperationException();
        }

        #endregion

        #region Operator

        public static ToolFile operator |(ToolFile left, string rightPath)
        {
            string lp = left.GetFullPath();
            return new ToolFile(Path.Combine(lp, rightPath));
        }
        public ToolFile Open(string path)
        {
            this.FullPath = path;
            Refresh();
            return this;
        }
        public ToolFile Open(FileMode mode)
        {
            this.Close();
            OriginControlStream = new FileStream(this.FullPath, mode);
            return this;
        }
        public ToolFile Close()
        {
            OriginControlStream?.Close();
            return this;
        }
        public ToolFile Create()
        {
            if (Exists() == false)
            {
                if (IsDir())
                    Directory.CreateDirectory(this.FullPath);
                else
                    File.Create(this.FullPath);
            }
            return this;
        }
        public ToolFile Rename(string newPath)
        {
            if(IsDir())
            {
                var dir = OriginInfo as DirectoryInfo;
                dir.MoveTo(newPath);
            }
            else
            {
                var file = OriginInfo as FileInfo;
                file.MoveTo(newPath);
            }
            FullPath = newPath;
            return this;
        }
        public ToolFile Move(string path)
        {
            Rename(path);
            return this;
        }
        public ToolFile Copy(string path,out ToolFile copyTo)
        {
            if (IsDir())
            {
                throw new InvalidOperationException();
            }
            else
            {
                var file = OriginInfo as FileInfo;
                file.CopyTo(path);
                copyTo = new(path);
            }
            return this;
        }
        public ToolFile Delete()
        {
            if (IsDir())
                Directory.Delete(FullPath);
            else
                File.Delete(FullPath);
            return this;
        }
        public ToolFile Remove() => Delete();
        public ToolFile MustExistsPath()
        {
            this.Close();
            this.TryCreateParentPath();
            this.Create();
            return this;
        }
        #endregion

        #region Dir

        public ToolFile TryCreateParentPath()
        {
            if (this.GetName().Contains('/') || this.GetName().Contains('\\'))
            {
                var parent = new ToolFile(this.GetParentDir());
                if (parent.Exists())
                {
                    parent.Create();
                }
            }
            return this;
        }

        public List<string> DirIter()
        {
            if (this.IsDir())
            {
                var dir = new DirectoryInfo(FullPath);
                var result = dir.GetDirectories().ToList().ConvertAll(x => x.FullName);
                result.AddRange(dir.GetFiles().ToList().ConvertAll(x => x.FullName));
                return result;
            }
            return null;
        }

        public List<ToolFile> DirToolFileIter()
        {
            if (this.IsDir())
            {
                return DirIter().ConvertAll(x => new ToolFile(x));
            }
            throw new DirectoryNotFoundException(FullPath);
        }

        public ToolFile BackToParentDir()
        {
            var file = new ToolFile(this.GetParentDir());
            this.Close();
            this.FullPath = file.FullPath;
            this.OriginInfo = file.OriginInfo;
            return this;
        }

        public ToolFile GetParentDir()
        {
            if (IsDir())
                return new ToolFile((this.OriginInfo as DirectoryInfo).Parent.FullName);
            else
                return new ToolFile((this.OriginInfo as FileInfo).DirectoryName);
        }

        public int DirCount()
        {
            if (IsDir())
                return Directory.EnumerateFiles(FullPath).Count();
            return -1;
        }

        public ToolFile DirClear()
        {
            if (IsDir())
            {
                foreach (var file in DirIter())
                {
                    File.Delete(file);
                }
            }
            throw new DirectoryNotFoundException();
        }

        public ToolFile MakeFileInside(string source, bool isDeleteSource = false)
        {
            if (this.IsDir() == false)
                throw new DirectoryNotFoundException(FullPath);
            string target = this | source;
            if (isDeleteSource)
                File.Move(target, source);
            else
                File.Copy(target, source);
            return this;
        }

        #endregion

        public static string[] SelectMultipleFiles(string filter = "所有文件|*.*", string title = "选择文件")
        {
            if (PlatformIndicator.IsPlatformWindows)
                return WindowsKit.SelectMultipleFiles(filter, title);
            throw new NotImplementedException();
        }

        public static string SelectFile(string filter = "所有文件|*.*", string title = "选择文件")
        {
            if (PlatformIndicator.IsPlatformWindows)
            {
                var results = WindowsKit.SelectMultipleFiles(filter, title);
                if (results != null && results.Length > 0)
                    return results[0];
            }
            throw new NotImplementedException();
        }

        public static string SaveFile(string filter = "所有文件|*.*", string title = "保存文件")
        {
            if (PlatformIndicator.IsPlatformWindows)
                return WindowsKit.SaveFile(filter, title);
            throw new NotImplementedException();
        }

        public static string SelectFolder(string description = "请选择文件夹")
        {
            if (PlatformIndicator.IsPlatformWindows)
                return WindowsKit.SelectFolder(description);
            throw new NotImplementedException();
        }

        public DateTime GetTimestamp()
        {
            return File.GetLastWriteTime(FullPath).ToUniversalTime();
        }

        public static string BrowseFile(params string[] extensions)
        {
            string filter = "";
            foreach (var ext in extensions)
            {
                filter += "*." + ext + ";";
            }
            string result = SelectFile("所有文件|" + filter);
            return result;
        }
        public static ToolFile BrowseToolFile(params string[] extensions)
        {
            return new ToolFile(BrowseFile(extensions));
        }
    }
}
