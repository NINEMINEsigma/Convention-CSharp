using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Convention
{
    public class FileOperationException : Exception
    {
        public FileOperationException(string message) : base(message) { }
        public FileOperationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class CompressionException : FileOperationException
    {
        public CompressionException(string message) : base(message) { }
        public CompressionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class EncryptionException : FileOperationException
    {
        public EncryptionException(string message) : base(message) { }
        public EncryptionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class HashException : FileOperationException
    {
        public HashException(string message) : base(message) { }
        public HashException(string message, Exception innerException) : base(message, innerException) { }
    }

    [Serializable]
    public sealed class ToolFile
    {
        private string FullPath;
        private FileSystemInfo OriginInfo;
        public ToolFile(string path) 
        {
            FullPath = Path.GetFullPath(path);
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

        public string GetFilename(bool is_without_extension = false)
        {
            if (is_without_extension && Path.HasExtension(FullPath))
            {
                return Path.GetFileNameWithoutExtension(FullPath);
            }
            else if (FullPath.EndsWith(Path.DirectorySeparatorChar.ToString()) || FullPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                return Path.GetFileName(FullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            }
            else
            {
                return Path.GetFileName(FullPath);
            }
        }

        public string GetDir()
        {
            return Path.GetDirectoryName(FullPath);
        }

        public ToolFile GetDirToolFile()
        {
            return new ToolFile(GetDir());
        }

        public string GetCurrentDirName()
        {
            return Path.GetDirectoryName(FullPath);
        }

        public ToolFile GetParentDir()
        {
            return new ToolFile(GetDir());
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

        public List<string[]> LoadAsCsv()
        {
            if (IsFile() == false)
                throw new InvalidOperationException("Target is not a file");
            
            var lines = File.ReadAllLines(FullPath);
            var result = new List<string[]>();
            
            foreach (var line in lines)
            {
                var fields = line.Split(',');
                result.Add(fields);
            }
            
            return result;
        }

        public string LoadAsXml()
        {
            return LoadAsText();
        }

        public string LoadAsExcel()
        {
            // 注意：真正的 Excel 读取需要第三方库如 EPPlus 或 NPOI
            // 这里返回文本内容作为简化实现
            return LoadAsText();
        }

        public string LoadAsUnknown(string suffix)
        {
            return LoadAsText();
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
        public void SaveAsText(string data)
        {
            using var fs = new FileStream(FullPath, FileMode.CreateNew, FileAccess.Write);
            using var sw = new StreamWriter(fs);
            sw.Write(data);
            sw.Flush();
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
            SaveDataAsBinary(FullPath, data, (OriginInfo as FileInfo).OpenWrite());
        }

        public void SaveAsCsv(List<string[]> csvData)
        {
            if (IsFile() == false)
                throw new InvalidOperationException("Target is not a file");
            
            var lines = csvData.Select(row => string.Join(",", row));
            File.WriteAllLines(FullPath, lines);
        }

        public void SaveAsXml(string xmlData)
        {
            SaveAsText(xmlData);
        }

        public void SaveAsExcel(string excelData)
        {
            SaveAsText(excelData);
        }

        public void SaveAsDataframe(List<string[]> dataframeData)
        {
            SaveAsCsv(dataframeData);
        }

        public void SaveAsUnknown(object unknownData)
        {
            if (unknownData is byte[] bytes)
                SaveAsBinary(bytes);
            else
                SaveAsText(unknownData.ToString());
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

        #region Size and Properties

        public long GetSize()
        {
            if (IsDir())
            {
                return GetDirectorySize(FullPath);
            }
            else
            {
                return (OriginInfo as FileInfo)?.Length ?? 0;
            }
        }

        private long GetDirectorySize(string path)
        {
            long size = 0;
            try
            {
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    size += fileInfo.Length;
                }

                var dirs = Directory.GetDirectories(path);
                foreach (var dir in dirs)
                {
                    size += GetDirectorySize(dir);
                }
            }
            catch (Exception)
            {
                // 忽略访问权限错误
            }
            return size;
        }

        #endregion

        #region Operator

        public static ToolFile operator |(ToolFile left, string rightPath)
        {
            string lp = left.GetFullPath();
            return new ToolFile(Path.Combine(lp, rightPath));
        }

        public override bool Equals(object obj)
        {
            if (obj is ToolFile other)
            {
                return Path.GetFullPath(FullPath).Equals(Path.GetFullPath(other.FullPath), StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Path.GetFullPath(FullPath).GetHashCode();
        }

        public ToolFile Open(string path)
        {
            this.FullPath = path;
            Refresh();
            return this;
        }
        public ToolFile Close()
        {
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

        public ToolFile Copy(string targetPath = null)
        {
            if (targetPath == null)
                return new ToolFile(FullPath);
            
            if (!Exists())
                throw new FileNotFoundException("File not found");
            
            var targetFile = new ToolFile(targetPath);
            if (targetFile.IsDir())
                targetFile = targetFile | GetFilename();
            
            if (IsDir())
                CopyDirectory(FullPath, targetFile.GetFullPath());
            else
                File.Copy(FullPath, targetFile.GetFullPath());
            
            return targetFile;
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }

        public ToolFile Delete()
        {
            if (IsDir())
                Directory.Delete(FullPath);
            else
                File.Delete(FullPath);
            return this;
        }

        public ToolFile Remove()
        {
            return Delete();
        }

        #endregion

        #region Directory Operations

        public ToolFile MustExistsPath()
        {
            TryCreateParentPath();
            Create();
            return this;
        }
        public ToolFile TryCreateParentPath()
        {
            string dirPath = Path.GetDirectoryName(FullPath);
            if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            return this;
        }
        public List<string> DirIter()
        {
            if (!IsDir())
                throw new InvalidOperationException("Target is not a directory");
            return Directory.GetFileSystemEntries(FullPath).ToList();
        }
        public List<ToolFile> DirToolFileIter()
        {
            if (!IsDir())
                throw new InvalidOperationException("Target is not a directory");
            var result = new List<ToolFile>();
            foreach (var entry in Directory.GetFileSystemEntries(FullPath))
            {
                result.Add(new ToolFile(entry));
            }
            return result;
        }
        public ToolFile BackToParentDir()
        {
            FullPath = GetDir();
            Refresh();
            return this;
        }
        public int DirCount(bool ignore_folder = true)
        {
            if (!IsDir())
                throw new InvalidOperationException("Target is not a directory");
            
            var entries = Directory.GetFileSystemEntries(FullPath);
            if (ignore_folder)
            {
                return entries.Count(entry => File.Exists(entry));
            }
            return entries.Length;
        }
        public ToolFile DirClear()
        {
            if (!IsDir())
                throw new InvalidOperationException("Target is not a directory");
            
            foreach (var file in DirToolFileIter())
            {
                file.Remove();
            }
            return this;
        }

        public ToolFile MakeFileInside(ToolFile data, bool is_delete_source = false)
        {
            if (!IsDir())
                throw new InvalidOperationException("Cannot make file inside a file, because this object target is not a directory");
            
            var result = this | data.GetFilename();
            if (is_delete_source)
                data.Move(result.GetFullPath());
            else
                data.Copy(result.GetFullPath());
            
            return this;
        }

        public ToolFile FirstFileWithExtension(string extension)
        {
            var targetDir = IsDir() ? this : GetDirToolFile();
            foreach (var file in targetDir.DirToolFileIter())
            {
                if (!file.IsDir() && file.GetExtension() == extension)
                {
                    return file;
                }
            }
            return null;
        }

        public ToolFile FirstFile(Func<string, bool> predicate)
        {
            var targetDir = IsDir() ? this : GetDirToolFile();
            foreach (var file in targetDir.DirToolFileIter())
            {
                if (predicate(file.GetFilename()))
                {
                    return file;
                }
            }
            return null;
        }

        public List<ToolFile> FindFileWithExtension(string extension)
        {
            var targetDir = IsDir() ? this : GetDirToolFile();
            var result = new List<ToolFile>();
            foreach (var file in targetDir.DirToolFileIter())
            {
                if (!file.IsDir() && file.GetExtension() == extension)
                {
                    result.Add(file);
                }
            }
            return result;
        }

        public List<ToolFile> FindFile(Func<string, bool> predicate)
        {
            var targetDir = IsDir() ? this : GetDirToolFile();
            var result = new List<ToolFile>();
            foreach (var file in targetDir.DirToolFileIter())
            {
                if (predicate(file.GetFilename()))
                {
                    result.Add(file);
                }
            }
            return result;
        }

        #endregion

        #region Compression

        public ToolFile Compress(string outputPath = null, string format = "zip")
        {
            if (!Exists())
                throw new FileNotFoundException($"File not found: {GetFullPath()}");

            if (outputPath == null)
            {
                outputPath = GetFullPath() + (format == "zip" ? ".zip" : ".tar");
            }

            try
            {
                if (format.ToLower() == "zip")
                {
                    if (IsDir())
                    {
                        ZipFile.CreateFromDirectory(FullPath, outputPath);
                    }
                    else
                    {
                        using (var archive = ZipFile.Open(outputPath, ZipArchiveMode.Create))
                        {
                            archive.CreateEntryFromFile(FullPath, GetFilename());
                        }
                    }
                }
                else
                {
                    throw new CompressionException($"Unsupported compression format: {format}");
                }

                return new ToolFile(outputPath);
            }
            catch (Exception ex)
            {
                throw new CompressionException($"Compression failed: {ex.Message}", ex);
            }
        }

        public ToolFile Decompress(string outputPath = null)
        {
            if (!Exists() || !IsFile())
                throw new FileNotFoundException($"File not found: {GetFullPath()}");

            if (GetExtension().ToLower() != "zip")
                throw new CompressionException("Only ZIP files are supported for decompression");

            if (outputPath == null)
            {
                outputPath = Path.Combine(GetDir(), Path.GetFileNameWithoutExtension(GetFilename()));
            }

            try
            {
                ZipFile.ExtractToDirectory(FullPath, outputPath);
                return new ToolFile(outputPath);
            }
            catch (Exception ex)
            {
                throw new CompressionException($"Decompression failed: {ex.Message}", ex);
            }
        }

        #endregion

        #region Encryption

        public ToolFile Encrypt(string key, string algorithm = "AES")
        {
            if (!Exists() || !IsFile())
                throw new FileNotFoundException($"File not found: {GetFullPath()}");

            try
            {
                byte[] data = LoadAsBinary();
                byte[] encryptedData;

                if (algorithm.ToUpper() == "AES")
                {
                    using (var aes = Aes.Create())
                    {
                        var keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                        aes.Key = keyBytes;
                        aes.GenerateIV();

                        using (var encryptor = aes.CreateEncryptor())
                        using (var ms = new MemoryStream())
                        {
                            ms.Write(aes.IV, 0, aes.IV.Length);
                            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                            {
                                cs.Write(data, 0, data.Length);
                                cs.FlushFinalBlock();
                            }
                            encryptedData = ms.ToArray();
                        }
                    }
                }
                else
                {
                    throw new EncryptionException($"Unsupported encryption algorithm: {algorithm}");
                }

                SaveAsBinary(encryptedData);
                return this;
            }
            catch (Exception ex)
            {
                throw new EncryptionException($"Encryption failed: {ex.Message}", ex);
            }
        }

        public ToolFile Decrypt(string key, string algorithm = "AES")
        {
            if (!Exists() || !IsFile())
                throw new FileNotFoundException($"File not found: {GetFullPath()}");

            try
            {
                byte[] encryptedData = LoadAsBinary();
                byte[] decryptedData;

                if (algorithm.ToUpper() == "AES")
                {
                    using (var aes = Aes.Create())
                    {
                        var keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                        aes.Key = keyBytes;

                        using (var ms = new MemoryStream(encryptedData))
                        {
                            byte[] iv = new byte[16];
                            ms.Read(iv, 0, 16);
                            aes.IV = iv;

                            using (var decryptor = aes.CreateDecryptor())
                            using (var resultMs = new MemoryStream())
                            {
                                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                                {
                                    cs.CopyTo(resultMs);
                                }
                                decryptedData = resultMs.ToArray();
                            }
                        }
                    }
                }
                else
                {
                    throw new EncryptionException($"Unsupported encryption algorithm: {algorithm}");
                }

                SaveAsBinary(decryptedData);
                return this;
            }
            catch (Exception ex)
            {
                throw new EncryptionException($"Decryption failed: {ex.Message}", ex);
            }
        }

        #endregion

        #region Hash

        public string CalculateHash(string algorithm = "MD5", int chunkSize = 8192)
        {
            if (!Exists() || !IsFile())
                throw new FileNotFoundException($"File not found: {GetFullPath()}");

            try
            {
                using (var hashAlgorithm = GetHashAlgorithm(algorithm))
                using (var stream = File.OpenRead(FullPath))
                {
                    byte[] hash = hashAlgorithm.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
            catch (Exception ex)
            {
                throw new HashException($"Hash calculation failed: {ex.Message}", ex);
            }
        }

        private HashAlgorithm GetHashAlgorithm(string algorithm)
        {
            return algorithm.ToUpper() switch
            {
                "MD5" => MD5.Create(),
                "SHA1" => SHA1.Create(),
                "SHA256" => SHA256.Create(),
                "SHA512" => SHA512.Create(),
                _ => throw new HashException($"Unsupported hash algorithm: {algorithm}")
            };
        }

        public bool VerifyHash(string expectedHash, string algorithm = "MD5")
        {
            string actualHash = CalculateHash(algorithm);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        public ToolFile SaveHash(string algorithm = "MD5", string outputPath = null)
        {
            string hash = CalculateHash(algorithm);
            
            if (outputPath == null)
            {
                outputPath = GetFullPath() + "." + algorithm.ToLower();
            }

            var hashFile = new ToolFile(outputPath);
            hashFile.SaveAsText(hash);
            
            return hashFile;
        }

        #endregion

        #region File Monitoring

        public void StartMonitoring(Action<string, string> callback, bool recursive = false, 
            List<string> ignorePatterns = null, bool ignoreDirectories = false, 
            bool caseSensitive = true, bool isLog = true)
        {
            if (!IsDir())
                throw new InvalidOperationException("Monitoring is only supported for directories");

            // 注意：这是一个简化实现，实际的文件监控需要更复杂的实现
            // 可以使用 FileSystemWatcher 来实现完整功能
            var watcher = new FileSystemWatcher(FullPath)
            {
                IncludeSubdirectories = recursive,
                EnableRaisingEvents = true
            };

            watcher.Created += (sender, e) => callback("created", e.FullPath);
            watcher.Changed += (sender, e) => callback("modified", e.FullPath);
            watcher.Deleted += (sender, e) => callback("deleted", e.FullPath);
            watcher.Renamed += (sender, e) => callback("moved", e.FullPath);
        }

        #endregion

        #region Backup

        public ToolFile CreateBackup(string backupDir = null, int maxBackups = 5, 
            string backupFormat = "zip", bool includeMetadata = true)
        {
            if (!Exists())
                throw new FileNotFoundException($"File not found: {GetFullPath()}");

            if (backupDir == null)
            {
                backupDir = Path.Combine(GetDir(), "backups");
            }

            var backupDirectory = new ToolFile(backupDir);
            if (!backupDirectory.Exists())
            {
                backupDirectory.Create();
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"{GetFilename()}_{timestamp}.{backupFormat}";
            string backupPath = Path.Combine(backupDir, backupFileName);

            try
            {
                if (backupFormat.ToLower() == "zip")
                {
                    Compress(backupPath, "zip");
                }
                else
                {
                    Copy(backupPath);
                }

                // 清理旧备份
                CleanOldBackups(backupDir, maxBackups, backupFormat);

                return new ToolFile(backupPath);
            }
            catch (Exception ex)
            {
                throw new FileOperationException($"Backup creation failed: {ex.Message}", ex);
            }
        }

        private void CleanOldBackups(string backupDir, int maxBackups, string format)
        {
            var backupFiles = Directory.GetFiles(backupDir, $"*.{format}")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .Skip(maxBackups);

            foreach (var file in backupFiles)
            {
                file.Delete();
            }
        }

        public ToolFile RestoreBackup(string backupFile, string restorePath = null, bool verifyHash = true)
        {
            var backupToolFile = new ToolFile(backupFile);
            if (!backupToolFile.Exists())
                throw new FileNotFoundException($"Backup file not found: {backupFile}");

            if (restorePath == null)
            {
                restorePath = GetFullPath();
            }

            try
            {
                if (backupToolFile.GetExtension().ToLower() == "zip")
                {
                    backupToolFile.Decompress(restorePath);
                }
                else
                {
                    backupToolFile.Copy(restorePath);
                }

                return new ToolFile(restorePath);
            }
            catch (Exception ex)
            {
                throw new FileOperationException($"Backup restoration failed: {ex.Message}", ex);
            }
        }

        public List<ToolFile> ListBackups(string backupDir = null)
        {
            if (backupDir == null)
            {
                backupDir = Path.Combine(GetDir(), "backups");
            }

            if (!Directory.Exists(backupDir))
                return new List<ToolFile>();

            return Directory.GetFiles(backupDir)
                .Where(f => Path.GetFileName(f).StartsWith(GetFilename()))
                .Select(f => new ToolFile(f))
                .OrderByDescending(f => f.GetTimestamp())
                .ToList();
        }

        #endregion

        #region Permissions

        public Dictionary<string, bool> GetPermissions()
        {
            if (!Exists())
                throw new FileNotFoundException($"File not found: {GetFullPath()}");

            var permissions = new Dictionary<string, bool>();
            
            try
            {
                var fileInfo = new FileInfo(FullPath);
                var attributes = fileInfo.Attributes;

                permissions["read"] = true; // 如果能获取到文件信息，说明可读
                permissions["write"] = !attributes.HasFlag(FileAttributes.ReadOnly);
                permissions["execute"] = false; // Windows 文件没有执行权限概念
                permissions["hidden"] = attributes.HasFlag(FileAttributes.Hidden);
            }
            catch (Exception)
            {
                permissions["read"] = false;
                permissions["write"] = false;
                permissions["execute"] = false;
                permissions["hidden"] = false;
            }

            return permissions;
        }

        public ToolFile SetPermissions(bool? read = null, bool? write = null, 
            bool? execute = null, bool? hidden = null, bool recursive = false)
        {
            if (!Exists())
                throw new FileNotFoundException($"File not found: {GetFullPath()}");

            try
            {
                var fileInfo = new FileInfo(FullPath);
                var attributes = fileInfo.Attributes;

                if (write.HasValue)
                {
                    if (write.Value)
                        attributes &= ~FileAttributes.ReadOnly;
                    else
                        attributes |= FileAttributes.ReadOnly;
                }

                if (hidden.HasValue)
                {
                    if (hidden.Value)
                        attributes |= FileAttributes.Hidden;
                    else
                        attributes &= ~FileAttributes.Hidden;
                }

                fileInfo.Attributes = attributes;

                if (recursive && IsDir())
                {
                    foreach (var child in DirToolFileIter())
                    {
                        child.SetPermissions(read, write, execute, hidden, true);
                    }
                }

                return this;
            }
            catch (Exception ex)
            {
                throw new FileOperationException($"Permission setting failed: {ex.Message}", ex);
            }
        }

        public bool IsReadable()
        {
            try
            {
                var permissions = GetPermissions();
                return permissions["read"];
            }
            catch
            {
                return false;
            }
        }

        public bool IsWritable()
        {
            try
            {
                var permissions = GetPermissions();
                return permissions["write"];
            }
            catch
            {
                return false;
            }
        }

        public bool IsExecutable()
        {
            try
            {
                var permissions = GetPermissions();
                return permissions["execute"];
            }
            catch
            {
                return false;
            }
        }

        public bool IsHidden()
        {
            try
            {
                var permissions = GetPermissions();
                return permissions["hidden"];
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Utility Methods

        public ToolFile MakeFileInside(string source, bool isDeleteSource = false)
        {
            if (IsDir() == false)
                throw new InvalidOperationException();
            var sourceFile = new ToolFile(source);
            return MakeFileInside(sourceFile, isDeleteSource);
        }
        public DateTime GetTimestamp()
        {
            return (OriginInfo as FileInfo)?.LastWriteTime ?? DateTime.MinValue;
        }

        #endregion
    }
}
