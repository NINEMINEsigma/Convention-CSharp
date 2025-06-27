using System;
using System.Collections;
using System.Collections.Generic;
using Convention.EasySave.Internal;

namespace Convention.EasySave
{
    /// <summary>
    /// The main class for Easy Save methods. All methods in this class are static.
    /// </summary> 
    public class EasySave
    {
        public enum Location { File, InternalMS, Cache };
        public enum Directory { PersistentDataPath, DataPath }
        public enum EncryptionType { None, AES };
        public enum CompressionType { None, Gzip };
        public enum Format { JSON };
        public enum ReferenceMode { ByRef, ByValue, ByRefAndValue };
        public enum ImageType { JPEG, PNG };

        #region EasySave.Save

        // <summary>Saves the value to the default file with the given key.</summary>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        public static void Save(string key, object value)
        {
            Save<object>(key, value, new EasySaveSettings());
        }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our value to.</param>
        public static void Save(string key, object value, string filePath)
        {
            Save<object>(key, value, new EasySaveSettings(filePath));
        }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our value to.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void Save(string key, object value, string filePath, EasySaveSettings settings)
        {
            Save<object>(key, value, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void Save(string key, object value, EasySaveSettings settings)
        {
            Save<object>(key, value, settings);
        }

        /// <summary>Saves the value to the default file with the given key.</summary>
        /// <param name="T">The type of the data that we want to save.</param>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        public static void Save<T>(string key, T value)
        {
            Save<T>(key, value, new EasySaveSettings());
        }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <param name="T">The type of the data that we want to save.</param>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our value to.</param>
        public static void Save<T>(string key, T value, string filePath)
        {
            Save<T>(key, value, new EasySaveSettings(filePath));
        }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <param name="T">The type of the data that we want to save.</param>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our value to.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void Save<T>(string key, T value, string filePath, EasySaveSettings settings)
        {
            Save<T>(key, value, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Saves the value to a file with the given key.</summary>
        /// <param name="T">The type of the data that we want to save.</param>
        /// <param name="key">The key we want to use to identify our value in the file.</param>
        /// <param name="value">The value we want to save.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void Save<T>(string key, T value, EasySaveSettings settings)
        {
            if (settings.location == Location.Cache)
            {
                EasySaveFile.GetOrCreateCachedFile(settings).Save(key, value);
                return;
            }

            using (var writer = EasySaveWriter.Create(settings))
            {
                writer.Write<T>(key, value);
                writer.Save();
            }
        }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="bytes">The bytes we want to store.</param>
        public static void SaveRaw(byte[] bytes)
        {
            SaveRaw(bytes, new EasySaveSettings());
        }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="bytes">The bytes we want to store.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our bytes to.</param>
        public static void SaveRaw(byte[] bytes, string filePath)
        {
            SaveRaw(bytes, new EasySaveSettings(filePath));
        }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="bytes">The bytes we want to store.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our bytes to.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void SaveRaw(byte[] bytes, string filePath, EasySaveSettings settings)
        {
            SaveRaw(bytes, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="bytes">The bytes we want to store.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void SaveRaw(byte[] bytes, EasySaveSettings settings)
        {
            if (settings.location == Location.Cache)
            {
                EasySaveFile.GetOrCreateCachedFile(settings).SaveRaw(bytes, settings);
                return;
            }

            using (var stream = EasySaveStream.CreateStream(settings, EasySaveFileMode.Write))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            EasySaveIO.CommitBackup(settings);
        }

        /// <summary>Creates or overwrites the default file with the specified raw bytes.</summary>
        /// <param name="str">The string we want to store.</param>
        public static void SaveRaw(string str)
        {
            SaveRaw(str, new EasySaveSettings());
        }

        /// <summary>Creates or overwrites the default file with the specified raw bytes.</summary>
        /// <param name="str">The string we want to store.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our bytes to.</param>
        public static void SaveRaw(string str, string filePath)
        {
            SaveRaw(str, new EasySaveSettings(filePath));
        }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="str">The string we want to store.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to store our bytes to.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void SaveRaw(string str, string filePath, EasySaveSettings settings)
        {
            SaveRaw(str, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Creates or overwrites a file with the specified raw bytes.</summary>
        /// <param name="str">The string we want to store.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void SaveRaw(string str, EasySaveSettings settings)
        {
            var bytes = settings.encoding.GetBytes(str);
            SaveRaw(bytes, settings);
        }

        /// <summary>Creates or appends the specified bytes to a file.</summary>
        /// <param name="bytes">The bytes we want to append.</param>
        public static void AppendRaw(byte[] bytes)
        {
            AppendRaw(bytes, new EasySaveSettings());
        }

        /// <summary>Creates or appends the specified bytes to a file.</summary>
        /// <param name="bytes">The bytes we want to append.</param>
        /// <param name="filepath">The relative or absolute path of the file we want to append our bytes to.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void AppendRaw(byte[] bytes, string filePath, EasySaveSettings settings)
        {
            AppendRaw(bytes, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Creates or appends the specified bytes to a file.</summary>
        /// <param name="bytes">The bytes we want to append.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void AppendRaw(byte[] bytes, EasySaveSettings settings)
        {
            if (settings.location == Location.Cache)
            {
                EasySaveFile.GetOrCreateCachedFile(settings).AppendRaw(bytes);
                return;
            }

            EasySaveSettings newSettings = new EasySaveSettings(settings.path, settings);
            newSettings.encryptionType = EncryptionType.None;
            newSettings.compressionType = CompressionType.None;

            using (var stream = EasySaveStream.CreateStream(newSettings, EasySaveFileMode.Append))
                stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>Creates or appends the specified string to the default file.</summary>
        /// <param name="str">The string we want to append.</param>
        public static void AppendRaw(string str)
        {
            AppendRaw(str, new EasySaveSettings());
        }

        /// <summary>Creates or appends the specified string to the default file.</summary>
        /// <param name="str">The string we want to append.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to append our string to.</param>
        public static void AppendRaw(string str, string filePath)
        {
            AppendRaw(str, new EasySaveSettings(filePath));
        }

        /// <summary>Creates or appends the specified string to the default file.</summary>
        /// <param name="str">The string we want to append.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to append our string to.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void AppendRaw(string str, string filePath, EasySaveSettings settings)
        {
            AppendRaw(str, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Creates or appends the specified string to the default file.</summary>
        /// <param name="str">The string we want to append.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void AppendRaw(string str, EasySaveSettings settings)
        {
            var bytes = settings.encoding.GetBytes(str);
            EasySaveSettings newSettings = new EasySaveSettings(settings.path, settings);
            newSettings.encryptionType = EncryptionType.None;
            newSettings.compressionType = CompressionType.None;

            if (settings.location == Location.Cache)
            {
                EasySaveFile.GetOrCreateCachedFile(settings).SaveRaw(bytes);
                return;
            }

            using (var stream = EasySaveStream.CreateStream(newSettings, EasySaveFileMode.Append))
                stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="imagePath">The relative or absolute path of the PNG or JPG file we want to create.</param>
        public static void SaveImage(Texture2D texture, string imagePath)
        {
            SaveImage(texture, new EasySaveSettings(imagePath));
        }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="imagePath">The relative or absolute path of the PNG or JPG file we want to create.</param>
        public static void SaveImage(Texture2D texture, string imagePath, EasySaveSettings settings)
        {
            SaveImage(texture, new EasySaveSettings(imagePath, settings));
        }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void SaveImage(Texture2D texture, EasySaveSettings settings)
        {
            SaveImage(texture, 75, settings);
        }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="quality">Quality to encode with, where 1 is minimum and 100 is maximum. Note that this only applies to JPGs.</param>
        /// <param name="imagePath">The relative or absolute path of the PNG or JPG file we want to create.</param>
        public static void SaveImage(Texture2D texture, int quality, string imagePath)
        {
            SaveImage(texture, quality, new EasySaveSettings(imagePath));
        }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="quality">Quality to encode with, where 1 is minimum and 100 is maximum. Note that this only applies to JPGs.</param>
        /// <param name="imagePath">The relative or absolute path of the PNG or JPG file we want to create.</param>
        public static void SaveImage(Texture2D texture, int quality, string imagePath, EasySaveSettings settings)
        {
            SaveImage(texture, quality, new EasySaveSettings(imagePath, settings));
        }

        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="quality">Quality to encode with, where 1 is minimum and 100 is maximum. Note that this only applies to JPGs.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void SaveImage(Texture2D texture, int quality, EasySaveSettings settings)
        {
            // Get the file extension to determine what format we want to save the image as.
            string extension = EasySaveIO.GetExtension(settings.path).ToLower();
            if (string.IsNullOrEmpty(extension))
                throw new System.ArgumentException("File path must have a file extension when using EasySave.SaveImage.");
            byte[] bytes;
            if (extension == ".jpg" || extension == ".jpeg")
                bytes = texture.EncodeToJPG(quality);
            else if (extension == ".png")
                bytes = texture.EncodeToPNG();
            else
                throw new System.ArgumentException("File path must have extension of .png, .jpg or .jpeg when using EasySave.SaveImage.");

            EasySave.SaveRaw(bytes, settings);
        }


        /// <summary>Saves a Texture2D as a PNG or JPG, depending on the file extension used for the filePath.</summary>
        /// <param name="texture">The Texture2D we want to save as a JPG or PNG.</param>
        /// <param name="quality">Quality to encode with, where 1 is minimum and 100 is maximum. Note that this only applies to JPGs.</param>
        public static byte[] SaveImageToBytes(Texture2D texture, int quality, EasySave.ImageType imageType)
        {
            if (imageType == ImageType.JPEG)
                return texture.EncodeToJPG(quality);
            else
                return texture.EncodeToPNG();
        }

        #endregion

        #region EasySave.Load<T>

        /* Standard load methods */

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        public static object Load(string key)
        {
            return Load<object>(key, new EasySaveSettings());
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        public static object Load(string key, string filePath)
        {
            return Load<object>(key, new EasySaveSettings(filePath));
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static object Load(string key, string filePath, EasySaveSettings settings)
        {
            return Load<object>(key, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static object Load(string key, EasySaveSettings settings)
        {
            return Load<object>(key, settings);
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        public static object Load(string key, object defaultValue)
        {
            return Load<object>(key, defaultValue, new EasySaveSettings());
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        public static object Load(string key, string filePath, object defaultValue)
        {
            return Load<object>(key, defaultValue, new EasySaveSettings(filePath));
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static object Load(string key, string filePath, object defaultValue, EasySaveSettings settings)
        {
            return Load<object>(key, defaultValue, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static object Load(string key, object defaultValue, EasySaveSettings settings)
        {
            return Load<object>(key, defaultValue, settings);
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        public static T Load<T>(string key)
        {
            return Load<T>(key, new EasySaveSettings());
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        public static T Load<T>(string key, string filePath)
        {
            return Load<T>(key, new EasySaveSettings(filePath));
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static T Load<T>(string key, string filePath, EasySaveSettings settings)
        {
            return Load<T>(key, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static T Load<T>(string key, EasySaveSettings settings)
        {
            if (settings.location == Location.Cache)
                return EasySaveFile.GetOrCreateCachedFile(settings).Load<T>(key);

            using (var reader = EasySaveReader.Create(settings))
            {
                if (reader == null)
                    throw new System.IO.FileNotFoundException("File \"" + settings.FullPath + "\" could not be found.");
                return reader.Read<T>(key);
            }
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        public static T Load<T>(string key, T defaultValue)
        {
            return Load<T>(key, defaultValue, new EasySaveSettings());
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        public static T Load<T>(string key, string filePath, T defaultValue)
        {
            return Load<T>(key, defaultValue, new EasySaveSettings(filePath));
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static T Load<T>(string key, string filePath, T defaultValue, EasySaveSettings settings)
        {
            return Load<T>(key, defaultValue, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static T Load<T>(string key, T defaultValue, EasySaveSettings settings)
        {
            if (settings.location == Location.Cache)
                return EasySaveFile.GetOrCreateCachedFile(settings).Load<T>(key, defaultValue);

            using (var reader = EasySaveReader.Create(settings))
            {
                if (reader == null)
                    return defaultValue;
                return reader.Read<T>(key, defaultValue);
            }
        }

        /* Self-assigning load methods */

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new instance.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        public static void LoadInto<T>(string key, object obj) where T : class
        {
            LoadInto<object>(key, obj, new EasySaveSettings());
        }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new instance.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        public static void LoadInto(string key, string filePath, object obj)
        {
            LoadInto<object>(key, obj, new EasySaveSettings(filePath));
        }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new instance.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void LoadInto(string key, string filePath, object obj, EasySaveSettings settings)
        {
            LoadInto<object>(key, obj, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new instance.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void LoadInto(string key, object obj, EasySaveSettings settings)
        {
            LoadInto<object>(key, obj, settings);
        }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new instance.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        public static void LoadInto<T>(string key, T obj) where T : class
        {
            LoadInto<T>(key, obj, new EasySaveSettings());
        }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new instance.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        public static void LoadInto<T>(string key, string filePath, T obj) where T : class
        {
            LoadInto<T>(key, obj, new EasySaveSettings(filePath));
        }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new instance.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void LoadInto<T>(string key, string filePath, T obj, EasySaveSettings settings) where T : class
        {
            LoadInto<T>(key, obj, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Loads the value from a file with the given key into an existing object, rather than creating a new instance.</summary>
        /// <param name="T">The type of the data that we want to load.</param>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="obj">The object we want to load the value into.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void LoadInto<T>(string key, T obj, EasySaveSettings settings) where T : class
        {
            if (EasySaveReflection.IsValueType(obj.GetType()))
                throw new InvalidOperationException("EasySave.LoadInto can only be used with reference types, but the data you're loading is a value type. Use EasySave.Load instead.");

            if (settings.location == Location.Cache)
            {
                EasySaveFile.GetOrCreateCachedFile(settings).LoadInto<T>(key, obj);
                return;
            }

            if (settings == null) settings = new EasySaveSettings();
            using (var reader = EasySaveReader.Create(settings))
            {
                if (reader == null)
                    throw new System.IO.FileNotFoundException("File \"" + settings.FullPath + "\" could not be found.");
                reader.ReadInto<T>(key, obj);
            }
        }

        /* LoadString method, as this can be difficult with overloads. */

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static string LoadString(string key, string defaultValue, EasySaveSettings settings)
        {
            return Load<string>(key, null, defaultValue, settings);
        }

        /// <summary>Loads the value from a file with the given key.</summary>
        /// <param name="key">The key which identifies the value we want to load.</param>
        /// <param name="defaultValue">The value we want to return if the file or key does not exist.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to load from.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static string LoadString(string key, string defaultValue, string filePath = null, EasySaveSettings settings = null)
        {
            return Load<string>(key, filePath, defaultValue, settings);
        }

        #endregion

        #region Other EasySave.Load Methods

        /// <summary>Loads the default file as a byte array.</summary>
        public static byte[] LoadRawBytes()
        {
            return LoadRawBytes(new EasySaveSettings());
        }

        /// <summary>Loads a file as a byte array.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to load as a byte array.</param>
        public static byte[] LoadRawBytes(string filePath)
        {
            return LoadRawBytes(new EasySaveSettings(filePath));
        }

        /// <summary>Loads a file as a byte array.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to load as a byte array.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static byte[] LoadRawBytes(string filePath, EasySaveSettings settings)
        {
            return LoadRawBytes(new EasySaveSettings(filePath, settings));
        }

        /// <summary>Loads the default file as a byte array.</summary>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static byte[] LoadRawBytes(EasySaveSettings settings)
        {
            if (settings.location == Location.Cache)
                return EasySaveFile.GetOrCreateCachedFile(settings).LoadRawBytes();

            using (var stream = EasySaveStream.CreateStream(settings, EasySaveFileMode.Read))
            {
                if (stream == null)
                    throw new System.IO.FileNotFoundException("File " + settings.path + " could not be found");

                if (stream.GetType() == typeof(System.IO.Compression.GZipStream))
                {
                    var gZipStream = (System.IO.Compression.GZipStream)stream;
                    using (var ms = new System.IO.MemoryStream())
                    {
                        EasySaveStream.CopyTo(gZipStream, ms);
                        return ms.ToArray();
                    }
                }
                else
                {
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    return bytes;
                }
            }

            /*if(settings.location == Location.File)
                return EasySaveIO.ReadAllBytes(settings.FullPath);
            else if(settings.location == Location.PlayerPrefs)
                return System.Convert.FromBase64String(PlayerPrefs.GetString(settings.FullPath));
            else if(settings.location == Location.Resources)
            {
                var textAsset = Resources.Load<TextAsset>(settings.FullPath);
                return textAsset.bytes;
            }
            return null;*/
        }

        /// <summary>Loads the default file as a byte array.</summary>
        public static string LoadRawString()
        {
            return LoadRawString(new EasySaveSettings());
        }

        /// <summary>Loads a file as a byte array.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to load as a byte array.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static string LoadRawString(string filePath)
        {
            return LoadRawString(new EasySaveSettings(filePath));
        }

        /// <summary>Loads a file as a byte array.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to load as a byte array.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static string LoadRawString(string filePath, EasySaveSettings settings)
        {
            return LoadRawString(new EasySaveSettings(filePath, settings));
        }

        /// <summary>Loads the default file as a byte array.</summary>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static string LoadRawString(EasySaveSettings settings)
        {
            var bytes = EasySave.LoadRawBytes(settings);
            return settings.encoding.GetString(bytes, 0, bytes.Length);
        }

        #endregion

        #region Serialize/Deserialize

        public static byte[] Serialize<T>(T value, EasySaveSettings settings = null)
        {
            return Serialize(value, EasySaveTypeMgr.GetOrCreateEasySaveType(typeof(T)), settings);
        }

        internal static byte[] Serialize(object value, Convention.EasySave.Types.EasySaveType type, EasySaveSettings settings = null)
        {
            if (settings == null) settings = new EasySaveSettings();

            using (var ms = new System.IO.MemoryStream())
            {
                using (var stream = EasySaveStream.CreateStream(ms, settings, EasySaveFileMode.Write))
                {
                    using (var baseWriter = EasySaveWriter.Create(stream, settings, false, false))
                    {
                        // If T is object, use the value to get it's type. Otherwise, use T so that it works with inheritence.
                        //var type = typeof(T) != typeof(object) ? typeof(T) : (value == null ? typeof(T) : value.GetType());
                        baseWriter.Write(value, type, settings.referenceMode);
                    }

                    return ms.ToArray();
                }
            }
        }

        public static T Deserialize<T>(byte[] bytes, EasySaveSettings settings = null)
        {
            return (T)Deserialize(EasySaveTypeMgr.GetOrCreateEasySaveType(typeof(T)), bytes, settings);
        }

        internal static object Deserialize(Convention.EasySave.Types.EasySaveType type, byte[] bytes, EasySaveSettings settings = null)
        {
            if (settings == null)
                settings = new EasySaveSettings();

            using (var ms = new System.IO.MemoryStream(bytes, false))
            using (var stream = EasySaveStream.CreateStream(ms, settings, EasySaveFileMode.Read))
            using (var reader = EasySaveReader.Create(stream, settings, false))
                return reader.Read<object>(type);
        }

        public static void DeserializeInto<T>(byte[] bytes, T obj, EasySaveSettings settings = null) where T : class
        {
            DeserializeInto(EasySaveTypeMgr.GetOrCreateEasySaveType(typeof(T)), bytes, obj, settings);
        }

        public static void DeserializeInto<T>(Convention.EasySave.Types.EasySaveType type, byte[] bytes, T obj, EasySaveSettings settings = null) where T : class
        {
            if (settings == null)
                settings = new EasySaveSettings();

            using (var ms = new System.IO.MemoryStream(bytes, false))
            using (var reader = EasySaveReader.Create(ms, settings, false))
                reader.ReadInto<T>(obj, type);
        }

        #endregion

        #region Other EasySave Methods

        public static byte[] EncryptBytes(byte[] bytes, string password = null)
        {
            if (string.IsNullOrEmpty(password))
                password = EasySaveSettings.defaultSettings.encryptionPassword;
            return new AESEncryptionAlgorithm().Encrypt(bytes, password, EasySaveSettings.defaultSettings.bufferSize);
        }

        public static byte[] DecryptBytes(byte[] bytes, string password = null)
        {
            if (string.IsNullOrEmpty(password))
                password = EasySaveSettings.defaultSettings.encryptionPassword;
            return new AESEncryptionAlgorithm().Decrypt(bytes, password, EasySaveSettings.defaultSettings.bufferSize);
        }

        public static string EncryptString(string str, string password = null)
        {
            return Convert.ToBase64String(EncryptBytes(EasySaveSettings.defaultSettings.encoding.GetBytes(str), password));
        }

        public static string DecryptString(string str, string password = null)
        {
            return EasySaveSettings.defaultSettings.encoding.GetString(DecryptBytes(Convert.FromBase64String(str), password));
        }

        public static byte[] CompressBytes(byte[] bytes)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                var settings = new EasySaveSettings();
                settings.location = EasySave.Location.InternalMS;
                settings.compressionType = EasySave.CompressionType.Gzip;
                settings.encryptionType = EncryptionType.None;

                using (var stream = EasySaveStream.CreateStream(ms, settings, EasySaveFileMode.Write))
                    stream.Write(bytes, 0, bytes.Length);

                return ms.ToArray();
            }
        }

        public static byte[] DecompressBytes(byte[] bytes)
        {
            using (var ms = new System.IO.MemoryStream(bytes))
            {
                var settings = new EasySaveSettings();
                settings.location = EasySave.Location.InternalMS;
                settings.compressionType = EasySave.CompressionType.Gzip;
                settings.encryptionType = EncryptionType.None;

                using (var output = new System.IO.MemoryStream())
                {
                    using (var input = EasySaveStream.CreateStream(ms, settings, EasySaveFileMode.Read))
                        EasySaveStream.CopyTo(input, output);
                    return output.ToArray();
                }
            }
        }

        public static string CompressString(string str)
        {
            return Convert.ToBase64String(CompressBytes(EasySaveSettings.defaultSettings.encoding.GetBytes(str)));
        }

        public static string DecompressString(string str)
        {
            return EasySaveSettings.defaultSettings.encoding.GetString(DecompressBytes(Convert.FromBase64String(str)));
        }

        /// <summary>Deletes the default file.</summary>
        public static void DeleteFile()
        {
            DeleteFile(new EasySaveSettings());
        }

        /// <summary>Deletes the file at the given path using the default settings.</summary>
        /// <param name="filePath">The relative or absolute path of the file we wish to delete.</param>
        public static void DeleteFile(string filePath)
        {
            DeleteFile(new EasySaveSettings(filePath));
        }

        /// <summary>Deletes the file at the given path using the settings provided.</summary>
        /// <param name="filePath">The relative or absolute path of the file we wish to delete.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void DeleteFile(string filePath, EasySaveSettings settings)
        {
            DeleteFile(new EasySaveSettings(filePath, settings));
        }

        /// <summary>Deletes the file specified by the EasySaveSettings object provided as a parameter.</summary>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void DeleteFile(EasySaveSettings settings)
        {
            if (settings.location == Location.File)
                EasySaveIO.DeleteFile(settings.FullPath);
            else if (settings.location == Location.Cache)
                EasySaveFile.RemoveCachedFile(settings);
        }

        /// <summary>Copies a file from one path to another.</summary>
        /// <param name="oldFilePath">The relative or absolute path of the file we want to copy.</param>
        /// <param name="newFilePath">The relative or absolute path of the copy we want to create.</param>
        public static void CopyFile(string oldFilePath, string newFilePath)
        {
            CopyFile(new EasySaveSettings(oldFilePath), new EasySaveSettings(newFilePath));
        }

        /// <summary>Copies a file from one location to another, using the EasySaveSettings provided to override any default settings.</summary>
        /// <param name="oldFilePath">The relative or absolute path of the file we want to copy.</param>
        /// <param name="newFilePath">The relative or absolute path of the copy we want to create.</param>
        /// <param name="oldSettings">The settings we want to use when copying the old file.</param>
        /// <param name="newSettings">The settings we want to use when creating the new file.</param>
        public static void CopyFile(string oldFilePath, string newFilePath, EasySaveSettings oldSettings, EasySaveSettings newSettings)
        {
            CopyFile(new EasySaveSettings(oldFilePath, oldSettings), new EasySaveSettings(newFilePath, newSettings));
        }

        /// <summary>Copies a file from one location to another, using the EasySaveSettings provided to determine the locations.</summary>
        /// <param name="oldSettings">The settings we want to use when copying the old file.</param>
        /// <param name="newSettings">The settings we want to use when creating the new file.</param>
        public static void CopyFile(EasySaveSettings oldSettings, EasySaveSettings newSettings)
        {
            if (oldSettings.location != newSettings.location)
                throw new InvalidOperationException("Cannot copy file from " + oldSettings.location + " to " + newSettings.location + ". Location must be the same for both source and destination.");

            if (oldSettings.location == Location.File)
            {
                if (EasySaveIO.FileExists(oldSettings.FullPath))
                {
                    // Create the new directory if it doesn't exist.
                    string newDirectory = EasySaveIO.GetDirectoryPath(newSettings.FullPath);
                    if (!EasySaveIO.DirectoryExists(newDirectory))
                        EasySaveIO.CreateDirectory(newDirectory);
                    // Otherwise delete the existing file so that we can overwrite it.
                    else
                        EasySaveIO.DeleteFile(newSettings.FullPath);
                    EasySaveIO.CopyFile(oldSettings.FullPath, newSettings.FullPath);
                }
            }
            else if (oldSettings.location == Location.Cache)
            {
                EasySaveFile.CopyCachedFile(oldSettings, newSettings);
            }
        }

        /// <summary>Renames a file.</summary>
        /// <param name="oldFilePath">The relative or absolute path of the file we want to rename.</param>
        /// <param name="newFilePath">The relative or absolute path we want to rename the file to.</param>
        public static void RenameFile(string oldFilePath, string newFilePath)
        {
            RenameFile(new EasySaveSettings(oldFilePath), new EasySaveSettings(newFilePath));
        }

        /// <summary>Renames a file.</summary>
        /// <param name="oldFilePath">The relative or absolute path of the file we want to rename.</param>
        /// <param name="newFilePath">The relative or absolute path we want to rename the file to.</param>
        /// <param name="oldSettings">The settings for the file we want to rename.</param>
        /// <param name="newSettings">The settings for the file we want our source file to be renamed to.</param>
        public static void RenameFile(string oldFilePath, string newFilePath, EasySaveSettings oldSettings, EasySaveSettings newSettings)
        {
            RenameFile(new EasySaveSettings(oldFilePath, oldSettings), new EasySaveSettings(newFilePath, newSettings));
        }

        /// <summary>Renames a file.</summary>
        /// <param name="oldSettings">The settings for the file we want to rename.</param>
        /// <param name="newSettings">The settings for the file we want our source file to be renamed to.</param>
        public static void RenameFile(EasySaveSettings oldSettings, EasySaveSettings newSettings)
        {
            if (oldSettings.location != newSettings.location)
                throw new InvalidOperationException("Cannot rename file in " + oldSettings.location + " to " + newSettings.location + ". Location must be the same for both source and destination.");

            if (oldSettings.location == Location.File)
            {
                if (EasySaveIO.FileExists(oldSettings.FullPath))
                {
                    EasySaveIO.DeleteFile(newSettings.FullPath);
                    EasySaveIO.MoveFile(oldSettings.FullPath, newSettings.FullPath);
                }
            }
            else if (oldSettings.location == Location.Cache)
            {
                EasySaveFile.CopyCachedFile(oldSettings, newSettings);
                EasySaveFile.RemoveCachedFile(oldSettings);
            }
        }

        /// <summary>Copies a file from one path to another.</summary>
        /// <param name="oldDirectoryPath">The relative or absolute path of the directory we want to copy.</param>
        /// <param name="newDirectoryPath">The relative or absolute path of the copy we want to create.</param>
        public static void CopyDirectory(string oldDirectoryPath, string newDirectoryPath)
        {
            CopyDirectory(new EasySaveSettings(oldDirectoryPath), new EasySaveSettings(newDirectoryPath));
        }

        /// <summary>Copies a file from one location to another, using the EasySaveSettings provided to override any default settings.</summary>
        /// <param name="oldDirectoryPath">The relative or absolute path of the directory we want to copy.</param>
        /// <param name="newDirectoryPath">The relative or absolute path of the copy we want to create.</param>
        /// <param name="oldSettings">The settings we want to use when copying the old directory.</param>
        /// <param name="newSettings">The settings we want to use when creating the new directory.</param>
        public static void CopyDirectory(string oldDirectoryPath, string newDirectoryPath, EasySaveSettings oldSettings, EasySaveSettings newSettings)
        {
            CopyDirectory(new EasySaveSettings(oldDirectoryPath, oldSettings), new EasySaveSettings(newDirectoryPath, newSettings));
        }

        /// <summary>Copies a file from one location to another, using the EasySaveSettings provided to determine the locations.</summary>
        /// <param name="oldSettings">The settings we want to use when copying the old file.</param>
        /// <param name="newSettings">The settings we want to use when creating the new file.</param>
        public static void CopyDirectory(EasySaveSettings oldSettings, EasySaveSettings newSettings)
        {
            if (oldSettings.location != Location.File)
                throw new InvalidOperationException("EasySave.CopyDirectory can only be used when the save location is 'File'");

            if (!DirectoryExists(oldSettings))
                throw new System.IO.DirectoryNotFoundException("Directory " + oldSettings.FullPath + " not found");

            if (!DirectoryExists(newSettings))
                EasySaveIO.CreateDirectory(newSettings.FullPath);

            foreach (var fileName in EasySave.GetFiles(oldSettings))
                CopyFile(EasySaveIO.CombinePathAndFilename(oldSettings.path, fileName),
                            EasySaveIO.CombinePathAndFilename(newSettings.path, fileName));

            foreach (var directoryName in GetDirectories(oldSettings))
                CopyDirectory(EasySaveIO.CombinePathAndFilename(oldSettings.path, directoryName),
                                EasySaveIO.CombinePathAndFilename(newSettings.path, directoryName));
        }

        /// <summary>Renames a file.</summary>
        /// <param name="oldDirectoryPath">The relative or absolute path of the file we want to rename.</param>
        /// <param name="newDirectoryPath">The relative or absolute path we want to rename the file to.</param>
        public static void RenameDirectory(string oldDirectoryPath, string newDirectoryPath)
        {
            RenameDirectory(new EasySaveSettings(oldDirectoryPath), new EasySaveSettings(newDirectoryPath));
        }

        /// <summary>Renames a file.</summary>
        /// <param name="oldDirectoryPath">The relative or absolute path of the file we want to rename.</param>
        /// <param name="newDirectoryPath">The relative or absolute path we want to rename the file to.</param>
        /// <param name="oldSettings">The settings for the file we want to rename.</param>
        /// <param name="newSettings">The settings for the file we want our source file to be renamed to.</param>
        public static void RenameDirectory(string oldDirectoryPath, string newDirectoryPath, EasySaveSettings oldSettings, EasySaveSettings newSettings)
        {
            RenameDirectory(new EasySaveSettings(oldDirectoryPath, oldSettings), new EasySaveSettings(newDirectoryPath, newSettings));
        }

        /// <summary>Renames a file.</summary>
        /// <param name="oldSettings">The settings for the file we want to rename.</param>
        /// <param name="newSettings">The settings for the file we want our source file to be renamed to.</param>
        public static void RenameDirectory(EasySaveSettings oldSettings, EasySaveSettings newSettings)
        {
            if (oldSettings.location == Location.File)
            {
                if (EasySaveIO.DirectoryExists(oldSettings.FullPath))
                {
                    EasySaveIO.DeleteDirectory(newSettings.FullPath);
                    EasySaveIO.MoveDirectory(oldSettings.FullPath, newSettings.FullPath);
                }
            }
            else if (oldSettings.location == Location.Cache)
                throw new System.NotSupportedException("Directories cannot be renamed when saving to Cache, PlayerPrefs, tvOS or using WebGL.");
        }

        /// <summary>Deletes the directory at the given path using the settings provided.</summary>
        /// <param name="directoryPath">The relative or absolute path of the folder we wish to delete.</param>
        public static void DeleteDirectory(string directoryPath)
        {
            DeleteDirectory(new EasySaveSettings(directoryPath));
        }

        /// <summary>Deletes the directory at the given path using the settings provided.</summary>
        /// <param name="directoryPath">The relative or absolute path of the folder we wish to delete.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void DeleteDirectory(string directoryPath, EasySaveSettings settings)
        {
            DeleteDirectory(new EasySaveSettings(directoryPath, settings));
        }

        /// <summary>Deletes the directory at the given path using the settings provided.</summary>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void DeleteDirectory(EasySaveSettings settings)
        {
            if (settings.location == Location.File)
                EasySaveIO.DeleteDirectory(settings.FullPath);
            else if (settings.location == Location.Cache)
                throw new System.NotSupportedException("Deleting Directories using Cache or PlayerPrefs is not supported.");
        }

        /// <summary>Deletes a key in the default file.</summary>
        /// <param name="key">The key we want to delete.</param>
        public static void DeleteKey(string key)
        {
            DeleteKey(key, new EasySaveSettings());
        }

        public static void DeleteKey(string key, string filePath)
        {
            DeleteKey(key, new EasySaveSettings(filePath));
        }

        /// <summary>Deletes a key in the file specified.</summary>
        /// <param name="key">The key we want to delete.</param>
        /// <param name="key">The relative or absolute path of the file we want to delete the key from.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void DeleteKey(string key, string filePath, EasySaveSettings settings)
        {
            DeleteKey(key, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Deletes a key in the file specified by the EasySaveSettings object.</summary>
        /// <param name="key">The key we want to delete.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void DeleteKey(string key, EasySaveSettings settings)
        {
            if (settings.location == Location.Cache)
                EasySaveFile.DeleteKey(key, settings);
            else if (EasySave.FileExists(settings))
            {
                using (var writer = EasySaveWriter.Create(settings))
                {
                    writer.MarkKeyForDeletion(key);
                    writer.Save();
                }
            }
        }

        /// <summary>Checks whether a key exists in the default file.</summary>
        /// <param name="key">The key we want to check the existence of.</param>
        /// <returns>True if the key exists, otherwise False.</returns>
        public static bool KeyExists(string key)
        {
            return KeyExists(key, new EasySaveSettings());
        }

        /// <summary>Checks whether a key exists in the specified file.</summary>
        /// <param name="key">The key we want to check the existence of.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to search.</param>
        /// <returns>True if the key exists, otherwise False.</returns>
        public static bool KeyExists(string key, string filePath)
        {
            return KeyExists(key, new EasySaveSettings(filePath));
        }

        /// <summary>Checks whether a key exists in the default file.</summary>
        /// <param name="key">The key we want to check the existence of.</param>
        /// <param name="filePath">The relative or absolute path of the file we want to search.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        /// <returns>True if the key exists, otherwise False.</returns>
        public static bool KeyExists(string key, string filePath, EasySaveSettings settings)
        {
            return KeyExists(key, new EasySaveSettings(filePath, settings));
        }

        /// <summary>Checks whether a key exists in a file.</summary>
        /// <param name="key">The key we want to check the existence of.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        /// <returns>True if the file exists, otherwise False.</returns>
        public static bool KeyExists(string key, EasySaveSettings settings)
        {
            if (settings.location == Location.Cache)
                return EasySaveFile.KeyExists(key, settings);

            using (var reader = EasySaveReader.Create(settings))
            {
                if (reader == null)
                    return false;
                return reader.Goto(key);
            }
        }

        /// <summary>Checks whether the default file exists.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to check the existence of.</param>
        /// <returns>True if the file exists, otherwise False.</returns>
        public static bool FileExists()
        {
            return FileExists(new EasySaveSettings());
        }

        /// <summary>Checks whether a file exists.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to check the existence of.</param>
        /// <returns>True if the file exists, otherwise False.</returns>
        public static bool FileExists(string filePath)
        {
            return FileExists(new EasySaveSettings(filePath));
        }

        /// <summary>Checks whether a file exists.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to check the existence of.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        /// <returns>True if the file exists, otherwise False.</returns>
        public static bool FileExists(string filePath, EasySaveSettings settings)
        {
            return FileExists(new EasySaveSettings(filePath, settings));
        }

        /// <summary>Checks whether a file exists.</summary>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        /// <returns>True if the file exists, otherwise False.</returns>
        public static bool FileExists(EasySaveSettings settings)
        {
            if (settings.location == Location.File)
                return EasySaveIO.FileExists(settings.FullPath);
            else if (settings.location == Location.Cache)
                return EasySaveFile.FileExists(settings);
            return false;
        }

        /// <summary>Checks whether a folder exists.</summary>
        /// <param name="folderPath">The relative or absolute path of the folder we want to check the existence of.</param>
        /// <returns>True if the folder exists, otherwise False.</returns>
        public static bool DirectoryExists(string folderPath)
        {
            return DirectoryExists(new EasySaveSettings(folderPath));
        }

        /// <summary>Checks whether a file exists.</summary>
        /// <param name="folderPath">The relative or absolute path of the folder we want to check the existence of.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        /// <returns>True if the folder exists, otherwise False.</returns>

        public static bool DirectoryExists(string folderPath, EasySaveSettings settings)
        {
            return DirectoryExists(new EasySaveSettings(folderPath, settings));
        }

        /// <summary>Checks whether a folder exists.</summary>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        /// <returns>True if the folder exists, otherwise False.</returns>
        public static bool DirectoryExists(EasySaveSettings settings)
        {
            if (settings.location == Location.File)
                return EasySaveIO.DirectoryExists(settings.FullPath);
            else if (settings.location == Location.Cache)
                throw new System.NotSupportedException("Directories are not supported for the Cache and PlayerPrefs location.");
            return false;
        }

        /// <summary>Gets an array of all of the key names in the default file.</summary>
        public static string[] GetKeys()
        {
            return GetKeys(new EasySaveSettings());
        }

        /// <summary>Gets an array of all of the key names in a file.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to get the key names from.</param>
        public static string[] GetKeys(string filePath)
        {
            return GetKeys(new EasySaveSettings(filePath));
        }

        /// <summary>Gets an array of all of the key names in a file.</summary>
        /// <param name="filePath">The relative or absolute path of the file we want to get the key names from.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static string[] GetKeys(string filePath, EasySaveSettings settings)
        {
            return GetKeys(new EasySaveSettings(filePath, settings));
        }

        /// <summary>Gets an array of all of the key names in a file.</summary>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static string[] GetKeys(EasySaveSettings settings)
        {

            if (settings.location == Location.Cache)
                return EasySaveFile.GetKeys(settings);

            var keys = new List<string>();
            using (var reader = EasySaveReader.Create(settings))
            {
                foreach (string key in reader.Properties)
                {
                    keys.Add(key);
                    reader.Skip();
                }
            }
            return keys.ToArray();
        }

        /// <summary>Gets an array of all of the file names in a directory.</summary>
        public static string[] GetFiles()
        {
            var settings = new EasySaveSettings();
            if (settings.location == EasySave.Location.File)
            {
                if (settings.directory == EasySave.Directory.PersistentDataPath)
                    settings.path = EasySaveIO.persistentDataPath;
                else
                    settings.path = EasySaveIO.dataPath;
            }
            return GetFiles(new EasySaveSettings());
        }

        /// <summary>Gets an array of all of the file names in a directory.</summary>
        /// <param name="directoryPath">The relative or absolute path of the directory we want to get the file names from.</param>
        public static string[] GetFiles(string directoryPath)
        {
            return GetFiles(new EasySaveSettings(directoryPath));
        }

        /// <summary>Gets an array of all of the file names in a directory.</summary>
        /// <param name="directoryPath">The relative or absolute path of the directory we want to get the file names from.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static string[] GetFiles(string directoryPath, EasySaveSettings settings)
        {
            return GetFiles(new EasySaveSettings(directoryPath, settings));
        }

        /// <summary>Gets an array of all of the file names in a directory.</summary>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static string[] GetFiles(EasySaveSettings settings)
        {
            if (settings.location == Location.Cache)
                return EasySaveFile.GetFiles();
            else if (settings.location != EasySave.Location.File)
                throw new System.NotSupportedException("EasySave.GetFiles can only be used when the location is set to File or Cache.");
            return EasySaveIO.GetFiles(settings.FullPath, false);
        }

        /// <summary>Gets an array of all of the sub-directory names in a directory.</summary>
        public static string[] GetDirectories()
        {
            return GetDirectories(new EasySaveSettings());
        }

        /// <summary>Gets an array of all of the sub-directory names in a directory.</summary>
        /// <param name="directoryPath">The relative or absolute path of the directory we want to get the sub-directory names from.</param>
        public static string[] GetDirectories(string directoryPath)
        {
            return GetDirectories(new EasySaveSettings(directoryPath));
        }

        /// <summary>Gets an array of all of the sub-directory names in a directory.</summary>
        /// <param name="directoryPath">The relative or absolute path of the directory we want to get the sub-directory names from.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static string[] GetDirectories(string directoryPath, EasySaveSettings settings)
        {
            return GetDirectories(new EasySaveSettings(directoryPath, settings));
        }

        /// <summary>Gets an array of all of the sub-directory names in a directory.</summary>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static string[] GetDirectories(EasySaveSettings settings)
        {
            if (settings.location != EasySave.Location.File)
                throw new System.NotSupportedException("EasySave.GetDirectories can only be used when the location is set to File.");
            return EasySaveIO.GetDirectories(settings.FullPath, false);
        }

        /// <summary>Creates a backup of the default file .</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        public static void CreateBackup()
        {
            CreateBackup(new EasySaveSettings());
        }

        /// <summary>Creates a backup of a file.</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        /// <param name="filePath">The relative or absolute path of the file we wish to create a backup of.</param>
        public static void CreateBackup(string filePath)
        {
            CreateBackup(new EasySaveSettings(filePath));
        }

        /// <summary>Creates a backup of a file.</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        /// <param name="filePath">The relative or absolute path of the file we wish to create a backup of.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void CreateBackup(string filePath, EasySaveSettings settings)
        {
            CreateBackup(new EasySaveSettings(filePath, settings));
        }

        /// <summary>Creates a backup of a file.</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        public static void CreateBackup(EasySaveSettings settings)
        {
            var backupSettings = new EasySaveSettings(settings.path + EasySaveIO.backupFileSuffix, settings);
            EasySave.CopyFile(settings, backupSettings);
        }

        /// <summary>Restores a backup of a file.</summary>
        /// <param name="filePath">The relative or absolute path of the file we wish to restore the backup of.</param>
        /// <returns>True if a backup was restored, or False if no backup could be found.</returns>
        public static bool RestoreBackup(string filePath)
        {
            return RestoreBackup(new EasySaveSettings(filePath));
        }

        /// <summary>Restores a backup of a file.</summary>
        /// <param name="filePath">The relative or absolute path of the file we wish to restore the backup of.</param>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        /// <returns>True if a backup was restored, or False if no backup could be found.</returns>
        public static bool RestoreBackup(string filePath, EasySaveSettings settings)
        {
            return RestoreBackup(new EasySaveSettings(filePath, settings));
        }

        /// <summary>Restores a backup of a file.</summary>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        /// <returns>True if a backup was restored, or False if no backup could be found.</returns>
        public static bool RestoreBackup(EasySaveSettings settings)
        {
            var backupSettings = new EasySaveSettings(settings.path + EasySaveIO.backupFileSuffix, settings);

            if (!FileExists(backupSettings))
                return false;

            EasySave.RenameFile(backupSettings, settings);

            return true;
        }

        public static DateTime GetTimestamp()
        {
            return GetTimestamp(new EasySaveSettings());
        }

        public static DateTime GetTimestamp(string filePath)
        {
            return GetTimestamp(new EasySaveSettings(filePath));
        }

        public static DateTime GetTimestamp(string filePath, EasySaveSettings settings)
        {
            return GetTimestamp(new EasySaveSettings(filePath, settings));
        }

        /// <summary>Gets the date and time the file was last updated, in the UTC timezone.</summary>
        /// <param name="settings">The settings we want to use to override the default settings.</param>
        /// <returns>A DateTime object represeting the UTC date and time the file was last updated.</returns>
        public static DateTime GetTimestamp(EasySaveSettings settings)
        {
            if (settings.location == Location.File)
                return EasySaveIO.GetTimestamp(settings.FullPath);
            else if (settings.location == Location.Cache)
                return EasySaveFile.GetTimestamp(settings);
            else
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>Stores the default cached file to persistent storage.</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        public static void StoreCachedFile()
        {
            EasySaveFile.Store();
        }

        /// <summary>Stores a cached file to persistent storage.</summary>
        /// <param name="filePath">The filename or path of the file we want to store the cached file to.</param>
        public static void StoreCachedFile(string filePath)
        {
            StoreCachedFile(new EasySaveSettings(filePath));
        }

        /// <summary>Creates a backup of a file.</summary>
        /// <param name="filePath">The filename or path of the file we want to store the cached file to.</param>
        /// <param name="settings">The settings of the file we want to store to.</param>
        public static void StoreCachedFile(string filePath, EasySaveSettings settings)
        {
            StoreCachedFile(new EasySaveSettings(filePath, settings));
        }

        /// <summary>Stores a cached file to persistent storage.</summary>
        /// <param name="settings">The settings of the file we want to store to.</param>
        public static void StoreCachedFile(EasySaveSettings settings)
        {
            EasySaveFile.Store(settings);
        }

        /// <summary>Loads the default file in persistent storage into the cache.</summary>
        /// <remarks>A backup is created by copying the file and giving it a .bak extension. 
        /// If a backup already exists it will be overwritten, so you will need to ensure that the old backup will not be required before calling this method.</remarks>
        public static void CacheFile()
        {
            CacheFile(new EasySaveSettings());
        }

        /// <summary>Loads a file from persistent storage into the cache.</summary>
        /// <param name="filePath">The filename or path of the file we want to store the cached file to.</param>
        public static void CacheFile(string filePath)
        {
            CacheFile(new EasySaveSettings(filePath));
        }

        /// <summary>Creates a backup of a file.</summary>
        /// <param name="filePath">The filename or path of the file we want to store the cached file to.</param>
        /// <param name="settings">The settings of the file we want to store to.</param>
        public static void CacheFile(string filePath, EasySaveSettings settings)
        {
            CacheFile(new EasySaveSettings(filePath, settings));
        }

        /// <summary>Stores a cached file to persistent storage.</summary>
        /// <param name="settings">The settings of the file we want to store to.</param>
        public static void CacheFile(EasySaveSettings settings)
        {
            EasySaveFile.CacheFile(settings);
        }

        /// <summary>Initialises Easy Save. This happens automatically when any EasySave methods are called, but is useful if you want to perform initialisation before calling an EasySave method.</summary>
        public static void Init()
        {
            var settings = EasySaveSettings.defaultSettings;
            var pdp = EasySaveIO.persistentDataPath; // Call this to initialise EasySaveIO for threading purposes.
            EasySaveTypeMgr.Init();
        }

        #endregion
    }
}
