using System;
using System.Collections.Generic;
using System.Linq;
using Convention;

namespace Convention.Test
{
    public class FileTest
    {
        public static void RunTests()
        {
            Console.WriteLine("=== ToolFile 功能测试 ===");
            
            // 测试基本文件操作
            TestBasicOperations();
            
            // 测试文件加载和保存
            TestLoadAndSave();
            
            // 测试压缩和解压
            TestCompression();
            
            // 测试加密和解密
            TestEncryption();
            
            // 测试哈希计算
            TestHash();
            
            // 测试备份功能
            TestBackup();
            
            // 测试权限管理
            TestPermissions();
            
            Console.WriteLine("=== 所有测试完成 ===");
        }
        
        private static void TestBasicOperations()
        {
            Console.WriteLine("\n--- 基本文件操作测试 ---");
            
            // 创建测试文件
            var testFile = new ToolFile("test_file.txt");
            testFile.SaveAsText("这是一个测试文件的内容");
            
            Console.WriteLine($"文件存在: {testFile.Exists()}");
            Console.WriteLine($"文件大小: {testFile.GetSize()} 字节");
            Console.WriteLine($"文件名: {testFile.GetFilename()}");
            Console.WriteLine($"文件扩展名: {testFile.GetExtension()}");
            Console.WriteLine($"文件目录: {testFile.GetDir()}");
            Console.WriteLine($"文件时间戳: {testFile.GetTimestamp()}");
            
            // 清理
            testFile.Remove();
        }
        
        private static void TestLoadAndSave()
        {
            Console.WriteLine("\n--- 文件加载和保存测试 ---");
            
            // 测试 JSON 保存和加载
            var jsonFile = new ToolFile("test_data.json");
            var testData = new { name = "测试", value = 123, items = new[] { "item1", "item2" } };
            jsonFile.SaveAsRawJson(testData);
            
            var loadedData = jsonFile.LoadAsRawJson<dynamic>();
            Console.WriteLine($"JSON 数据加载成功: {loadedData != null}");
            
            // 测试 CSV 保存和加载
            var csvFile = new ToolFile("test_data.csv");
            var csvData = new List<string[]>
            {
                new[] { "姓名", "年龄", "城市" },
                new[] { "张三", "25", "北京" },
                new[] { "李四", "30", "上海" }
            };
            csvFile.SaveAsCsv(csvData);
            
            var loadedCsv = csvFile.LoadAsCsv();
            Console.WriteLine($"CSV 数据加载成功: {loadedCsv.Count} 行");
            
            // 清理
            jsonFile.Remove();
            csvFile.Remove();
        }
        
        private static void TestCompression()
        {
            Console.WriteLine("\n--- 压缩和解压测试 ---");
            
            // 创建测试文件
            var testFile = new ToolFile("test_compress.txt");
            testFile.SaveAsText("这是一个用于压缩测试的文件内容。".PadRight(1000, 'x'));
            
            // 压缩文件
            var compressedFile = testFile.Compress();
            Console.WriteLine($"压缩成功: {compressedFile.Exists()}");
            Console.WriteLine($"压缩后大小: {compressedFile.GetSize()} 字节");
            
            // 解压文件
            var decompressedDir = compressedFile.Decompress();
            Console.WriteLine($"解压成功: {decompressedDir.Exists()}");
            
            // 清理
            testFile.Remove();
            compressedFile.Remove();
            if (decompressedDir.Exists())
                decompressedDir.Remove();
        }
        
        private static void TestEncryption()
        {
            Console.WriteLine("\n--- 加密和解密测试 ---");
            
            // 创建测试文件
            var testFile = new ToolFile("test_encrypt.txt");
            testFile.SaveAsText("这是一个用于加密测试的敏感数据");
            
            // 加密文件
            testFile.Encrypt("mysecretkey123");
            Console.WriteLine($"加密成功: {testFile.Exists()}");
            
            // 解密文件
            testFile.Decrypt("mysecretkey123");
            var decryptedContent = testFile.LoadAsText();
            Console.WriteLine($"解密成功: {decryptedContent.Contains("敏感数据")}");
            
            // 清理
            testFile.Remove();
        }
        
        private static void TestHash()
        {
            Console.WriteLine("\n--- 哈希计算测试 ---");
            
            // 创建测试文件
            var testFile = new ToolFile("test_hash.txt");
            testFile.SaveAsText("这是一个用于哈希测试的文件内容");
            
            // 计算不同算法的哈希值
            var md5Hash = testFile.CalculateHash("MD5");
            var sha256Hash = testFile.CalculateHash("SHA256");
            
            Console.WriteLine($"MD5 哈希: {md5Hash}");
            Console.WriteLine($"SHA256 哈希: {sha256Hash}");
            
            // 验证哈希值
            var isValid = testFile.VerifyHash(md5Hash, "MD5");
            Console.WriteLine($"哈希验证: {isValid}");
            
            // 保存哈希值到文件
            var hashFile = testFile.SaveHash("MD5");
            Console.WriteLine($"哈希文件保存: {hashFile.Exists()}");
            
            // 清理
            testFile.Remove();
            hashFile.Remove();
        }
        
        private static void TestBackup()
        {
            Console.WriteLine("\n--- 备份功能测试 ---");
            
            // 创建测试文件
            var testFile = new ToolFile("test_backup.txt");
            testFile.SaveAsText("这是一个用于备份测试的文件内容");
            
            // 创建备份
            var backupFile = testFile.CreateBackup();
            Console.WriteLine($"备份创建成功: {backupFile.Exists()}");
            
            // 列出备份
            var backups = testFile.ListBackups();
            Console.WriteLine($"备份数量: {backups.Count}");
            
            // 恢复备份
            var restoredFile = testFile.RestoreBackup(backupFile.GetFullPath(), "restored_file.txt");
            Console.WriteLine($"备份恢复成功: {restoredFile.Exists()}");
            
            // 清理
            testFile.Remove();
            backupFile.Remove();
            restoredFile.Remove();
        }
        
        private static void TestPermissions()
        {
            Console.WriteLine("\n--- 权限管理测试 ---");
            
            // 创建测试文件
            var testFile = new ToolFile("test_permissions.txt");
            testFile.SaveAsText("这是一个用于权限测试的文件内容");
            
            // 获取权限
            var permissions = testFile.GetPermissions();
            Console.WriteLine($"读取权限: {permissions["read"]}");
            Console.WriteLine($"写入权限: {permissions["write"]}");
            Console.WriteLine($"隐藏属性: {permissions["hidden"]}");
            
            // 设置权限
            testFile.SetPermissions(hidden: true);
            var newPermissions = testFile.GetPermissions();
            Console.WriteLine($"设置隐藏后: {newPermissions["hidden"]}");
            
            // 检查权限
            Console.WriteLine($"可读: {testFile.IsReadable()}");
            Console.WriteLine($"可写: {testFile.IsWritable()}");
            Console.WriteLine($"隐藏: {testFile.IsHidden()}");
            
            // 清理
            testFile.Remove();
        }
    }
} 