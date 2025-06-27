using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Convention.EasySave.Types;
using Convention.EasySave.Internal;
using System.Linq;

/// <summary>Represents a cached file which can be saved to and loaded from, and commited to storage when necessary.</summary>
public class EasySaveFile
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static Dictionary<string, EasySaveFile> cachedFiles = new Dictionary<string, EasySaveFile>();

    public EasySaveSettings settings;
    private Dictionary<string, EasySaveData> cache = new Dictionary<string, EasySaveData>();
    private bool syncWithFile = false;
    private DateTime timestamp = DateTime.UtcNow;

    /// <summary>Creates a new EasySaveFile and loads the default file into the EasySaveFile if there is data to load.</summary>
    public EasySaveFile() : this(new EasySaveSettings(), true) { }

    /// <summary>Creates a new EasySaveFile and loads the specified file into the EasySaveFile if there is data to load.</summary>
    /// <param name="filepath">The relative or absolute path of the file in storage our EasySaveFile is associated with.</param>
    public EasySaveFile(string filePath) : this(new EasySaveSettings(filePath), true) { }

    /// <summary>Creates a new EasySaveFile and loads the specified file into the EasySaveFile if there is data to load.</summary>
    /// <param name="filepath">The relative or absolute path of the file in storage our EasySaveFile is associated with.</param>
    /// <param name="settings">The settings we want to use to override the default settings.</param>
    public EasySaveFile(string filePath, EasySaveSettings settings) : this(new EasySaveSettings(filePath, settings), true) { }

    /// <summary>Creates a new EasySaveFile and loads the specified file into the EasySaveFile if there is data to load.</summary>
    /// <param name="settings">The settings we want to use to override the default settings.</param>
    public EasySaveFile(EasySaveSettings settings) : this(settings, true) { }

    /// <summary>Creates a new EasySaveFile and only loads the default file into it if syncWithFile is set to true.</summary>
    /// <param name="syncWithFile">Whether we should sync this EasySaveFile with the one in storage immediately after creating it.</param>
    public EasySaveFile(bool syncWithFile) : this(new EasySaveSettings(), syncWithFile) { }
    /// <summary>Creates a new EasySaveFile and only loads the specified file into it if syncWithFile is set to true.</summary>
    /// <param name="filepath">The relative or absolute path of the file in storage our EasySaveFile is associated with.</param>
    /// <param name="syncWithFile">Whether we should sync this EasySaveFile with the one in storage immediately after creating it.</param>
    public EasySaveFile(string filePath, bool syncWithFile) : this(new EasySaveSettings(filePath), syncWithFile) { }
    /// <summary>Creates a new EasySaveFile and only loads the specified file into it if syncWithFile is set to true.</summary>
    /// <param name="filepath">The relative or absolute path of the file in storage our EasySaveFile is associated with.</param>
    /// <param name="settings">The settings we want to use to override the default settings.</param>
    /// <param name="syncWithFile">Whether we should sync this EasySaveFile with the one in storage immediately after creating it.</param>
    public EasySaveFile(string filePath, EasySaveSettings settings, bool syncWithFile) : this(new EasySaveSettings(filePath, settings), syncWithFile) { }

    /// <summary>Creates a new EasySaveFile and loads the specified file into the EasySaveFile if there is data to load.</summary>
    /// <param name="settings">The settings we want to use to override the default settings.</param>
    /// <param name="syncWithFile">Whether we should sync this EasySaveFile with the one in storage immediately after creating it.</param>
    public EasySaveFile(EasySaveSettings settings, bool syncWithFile)
    {
        this.settings = settings;
        this.syncWithFile = syncWithFile;
        if (syncWithFile)
        {
            // Type checking must be enabled when syncing.
            var settingsWithTypeChecking = (EasySaveSettings)settings.Clone();
            settingsWithTypeChecking.typeChecking = true;

            using (var reader = EasySaveReader.Create(settingsWithTypeChecking))
            {
                if (reader == null)
                    return;
                foreach (KeyValuePair<string, EasySaveData> kvp in reader.RawEnumerator)
                    cache[kvp.Key] = kvp.Value;
            }

            timestamp = EasySave.GetTimestamp(settingsWithTypeChecking);
        }
    }

    /// <summary>Creates a new EasySaveFile and loads the bytes into the EasySaveFile. Note the bytes must represent that of a file.</summary>
    /// <param name="bytes">The bytes representing our file.</param>
    /// <param name="settings">The settings we want to use to override the default settings.</param>
    /// <param name="syncWithFile">Whether we should sync this EasySaveFile with the one in storage immediately after creating it.</param>
    public EasySaveFile(byte[] bytes, EasySaveSettings settings = null)
    {
        if (settings == null)
            this.settings = new EasySaveSettings();
        else
            this.settings = settings;

        syncWithFile = true; // This ensures that the file won't be merged, which would prevent deleted keys from being deleted.

        SaveRaw(bytes, settings);
    }

    /// <summary>Synchronises this EasySaveFile with a file in storage.</summary>
    public void Sync()
    {
        Sync(this.settings);
    }

    /// <summary>Synchronises this EasySaveFile with a file in storage.</summary>
    /// <param name="filepath">The relative or absolute path of the file in storage we want to synchronise with.</param>
    /// <param name="settings">The settings we want to use to override the default settings.</param>
    public void Sync(string filePath, EasySaveSettings settings = null)
    {
        Sync(new EasySaveSettings(filePath, settings));
    }

    /// <summary>Synchronises this EasySaveFile with a file in storage.</summary>
    /// <param name="settings">The settings we want to use to override the default settings.</param>
    public void Sync(EasySaveSettings settings = null)
    {
        if (settings == null)
            settings = new EasySaveSettings();

        if (cache.Count == 0)
        {
            EasySave.DeleteFile(settings);
            return;
        }

        using (var baseWriter = EasySaveWriter.Create(settings, true, !syncWithFile, false))
        {
            foreach (var kvp in cache)
            {
                // If we change the name of a type, the type may be null.
                // In this case, use System.Object as the type.
                Type type;
                if (kvp.Value.type == null)
                    type = typeof(System.Object);
                else
                    type = kvp.Value.type.type;
                baseWriter.Write(kvp.Key, type, kvp.Value.bytes);
            }
            baseWriter.Save(!syncWithFile);
        }
    }

    /// <summary>Removes the data stored in this EasySaveFile. The EasySaveFile will be empty after calling this method.</summary>
    public void Clear()
    {
        cache.Clear();
    }

    /// <summary>Returns an array of all of the key names in this EasySaveFile.</summary>
    public string[] GetKeys()
    {
        var keyCollection = cache.Keys;
        var keys = new string[keyCollection.Count];
        keyCollection.CopyTo(keys, 0);
        return keys;
    }

    #region Save Methods

    /// <summary>Saves a value to a key in this EasySaveFile.</summary>
    /// <param name="key">The key we want to use to identify our value in the file.</param>
    /// <param name="value">The value we want to save.</param>
    public void Save<T>(string key, T value)
    {
        var unencryptedSettings = (EasySaveSettings)settings.Clone();
        unencryptedSettings.encryptionType = EasySave.EncryptionType.None;
        unencryptedSettings.compressionType = EasySave.CompressionType.None;

        // If T is object, use the value to get it's type. Otherwise, use T so that it works with inheritence.
        Type type;
        if (value == null)
            type = typeof(T);
        else
            type = value.GetType();

        cache[key] = new EasySaveData(EasySaveTypeMgr.GetOrCreateEasySaveType(type), EasySave.Serialize(value, unencryptedSettings));
    }

    /// <summary>Merges the data specified by the bytes parameter into this EasySaveFile.</summary>
    /// <param name="bytes">The bytes we want to merge with this EasySaveFile.</param>
    /// <param name="settings">The settings we want to use to override the default settings.</param>
    public void SaveRaw(byte[] bytes, EasySaveSettings settings = null)
    {
        if (settings == null)
            settings = new EasySaveSettings();

        // Type checking must be enabled when syncing.
        var settingsWithTypeChecking = (EasySaveSettings)settings.Clone();
        settingsWithTypeChecking.typeChecking = true;

        using (var reader = EasySaveReader.Create(bytes, settingsWithTypeChecking))
        {
            if (reader == null)
                return;
            foreach (KeyValuePair<string, EasySaveData> kvp in reader.RawEnumerator)
                cache[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>Merges the data specified by the bytes parameter into this EasySaveFile.</summary>
    /// <param name="bytes">The bytes we want to merge with this EasySaveFile.</param>
    /// <param name="settings">The settings we want to use to override the default settings.</param>
    public void AppendRaw(byte[] bytes, EasySaveSettings settings = null)
    {
        if (settings == null)
            settings = new EasySaveSettings();
        // AppendRaw just does the same thing as SaveRaw in EasySaveFile.
        SaveRaw(bytes, settings);
    }

    #endregion

    #region Load Methods

    /* Standard load methods */

    /// <summary>Loads the value from this EasySaveFile with the given key.</summary>
    /// <param name="key">The key which identifies the value we want to load.</param>
    public object Load(string key)
    {
        return Load<object>(key);
    }

    /// <summary>Loads the value from this EasySaveFile with the given key.</summary>
    /// <param name="key">The key which identifies the value we want to load.</param>
    /// <param name="defaultValue">The value we want to return if the key does not exist in this EasySaveFile.</param>
    public object Load(string key, object defaultValue)
    {
        return Load<object>(key, defaultValue);
    }

    /// <summary>Loads the value from this EasySaveFile with the given key.</summary>
    /// <param name="key">The key which identifies the value we want to load.</param>
    public T Load<T>(string key)
    {
        EasySaveData es3Data;

        if (!cache.TryGetValue(key, out es3Data))
            throw new KeyNotFoundException("Key \"" + key + "\" was not found in this EasySaveFile. Use Load<T>(key, defaultValue) if you want to return a default value if the key does not exist.");

        var unencryptedSettings = (EasySaveSettings)this.settings.Clone();
        unencryptedSettings.encryptionType = EasySave.EncryptionType.None;
        unencryptedSettings.compressionType = EasySave.CompressionType.None;

        if (typeof(T) == typeof(object))
            return (T)EasySave.Deserialize(es3Data.type, es3Data.bytes, unencryptedSettings);
        return EasySave.Deserialize<T>(es3Data.bytes, unencryptedSettings);
    }

    /// <summary>Loads the value from this EasySaveFile with the given key.</summary>
    /// <param name="key">The key which identifies the value we want to load.</param>
    /// <param name="defaultValue">The value we want to return if the key does not exist in this EasySaveFile.</param>
    public T Load<T>(string key, T defaultValue)
    {
        EasySaveData es3Data;

        if (!cache.TryGetValue(key, out es3Data))
            return defaultValue;
        var unencryptedSettings = (EasySaveSettings)this.settings.Clone();
        unencryptedSettings.encryptionType = EasySave.EncryptionType.None;
        unencryptedSettings.compressionType = EasySave.CompressionType.None;

        if (typeof(T) == typeof(object))
            return (T)EasySave.Deserialize(es3Data.type, es3Data.bytes, unencryptedSettings);
        return EasySave.Deserialize<T>(es3Data.bytes, unencryptedSettings);
    }

    /// <summary>Loads the value from this EasySaveFile with the given key into an existing object.</summary>
    /// <param name="key">The key which identifies the value we want to load.</param>
    /// <param name="obj">The object we want to load the value into.</param>
    public void LoadInto<T>(string key, T obj) where T : class
    {
        EasySaveData es3Data;

        if (!cache.TryGetValue(key, out es3Data))
            throw new KeyNotFoundException("Key \"" + key + "\" was not found in this EasySaveFile. Use Load<T>(key, defaultValue) if you want to return a default value if the key does not exist.");

        var unencryptedSettings = (EasySaveSettings)this.settings.Clone();
        unencryptedSettings.encryptionType = EasySave.EncryptionType.None;
        unencryptedSettings.compressionType = EasySave.CompressionType.None;

        if (typeof(T) == typeof(object))
            EasySave.DeserializeInto(es3Data.type, es3Data.bytes, obj, unencryptedSettings);
        else
            EasySave.DeserializeInto(es3Data.bytes, obj, unencryptedSettings);
    }

    #endregion

    #region Load Raw Methods

    /// <summary>Loads the EasySaveFile as a raw, unencrypted, uncompressed byte array.</summary>
    public byte[] LoadRawBytes()
    {
        var newSettings = (EasySaveSettings)settings.Clone();
        if (!newSettings.postprocessRawCachedData)
        {
            newSettings.encryptionType = EasySave.EncryptionType.None;
            newSettings.compressionType = EasySave.CompressionType.None;
        }
        return GetBytes(newSettings);
    }

    /// <summary>Loads the EasySaveFile as a raw, unencrypted, uncompressed string, using the encoding defined in the EasySaveFile's settings variable.</summary>
    public string LoadRawString()
    {
        if (cache.Count == 0)
            return "";
        return settings.encoding.GetString(LoadRawBytes());
    }

    /*
     * Same as LoadRawString, except it will return an encrypted/compressed file if these are enabled.
     */
    internal byte[] GetBytes(EasySaveSettings settings = null)
    {
        if (cache.Count == 0)
            return new byte[0];

        if (settings == null)
            settings = this.settings;

        using (var ms = new System.IO.MemoryStream())
        {
            var memorySettings = (EasySaveSettings)settings.Clone();
            memorySettings.location = EasySave.Location.InternalMS;
            // Ensure we return unencrypted bytes.
            if (!memorySettings.postprocessRawCachedData)
            {
                memorySettings.encryptionType = EasySave.EncryptionType.None;
                memorySettings.compressionType = EasySave.CompressionType.None;
            }

            using (var baseWriter = EasySaveWriter.Create(EasySaveStream.CreateStream(ms, memorySettings, EasySaveFileMode.Write), memorySettings, true, false))
            {
                foreach (var kvp in cache)
                    baseWriter.Write(kvp.Key, kvp.Value.type.type, kvp.Value.bytes);
                baseWriter.Save(false);
            }

            return ms.ToArray();
        }
    }

    #endregion

    #region Other EasySave Methods

    /// <summary>Deletes a key from this EasySaveFile.</summary>
    /// <param name="key">The key we want to delete.</param>
    public void DeleteKey(string key)
    {
        cache.Remove(key);
    }

    /// <summary>Checks whether a key exists in this EasySaveFile.</summary>
    /// <param name="key">The key we want to check the existence of.</param>
    /// <returns>True if the key exists, otherwise False.</returns>
    public bool KeyExists(string key)
    {
        return cache.ContainsKey(key);
    }

    /// <summary>Gets the size of the cached data in bytes.</summary>
    public int Size()
    {
        int size = 0;
        foreach (var kvp in cache)
            size += kvp.Value.bytes.Length;
        return size;
    }

    public Type GetKeyType(string key)
    {
        EasySaveData es3data;
        if (!cache.TryGetValue(key, out es3data))
            throw new KeyNotFoundException("Key \"" + key + "\" was not found in this EasySaveFile. Use Load<T>(key, defaultValue) if you want to return a default value if the key does not exist.");

        return es3data.type.type;
    }

    #endregion

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static EasySaveFile GetOrCreateCachedFile(EasySaveSettings settings)
    {
        EasySaveFile cachedFile;
        if (!cachedFiles.TryGetValue(settings.path, out cachedFile))
        {
            cachedFile = new EasySaveFile(settings, false);
            cachedFiles.Add(settings.path, cachedFile);
            cachedFile.syncWithFile = true; // This ensures that the file won't be merged, which would prevent deleted keys from being deleted.
        }
        // Settings might refer to the same file, but might have changed.
        // To account for this, we update the settings of the EasySaveFile each time we access it.
        cachedFile.settings = settings;
        return cachedFile;
    }

    internal static void CacheFile(EasySaveSettings settings)
    {
        // If we're still using cached settings, set it to the default location.
        if (settings.location == EasySave.Location.Cache)
        {
            settings = (EasySaveSettings)settings.Clone();
            // If the default settings are also set to cache, assume EasySave.Location.File. Otherwise, set it to the default location.
            settings.location = EasySaveSettings.defaultSettings.location == EasySave.Location.Cache ? EasySave.Location.File : EasySaveSettings.defaultSettings.location;
        }

        if (!EasySave.FileExists(settings))
            return;

        // Disable compression and encryption when loading the raw bytes, and the EasySaveFile constructor will expect encrypted/compressed bytes.
        var loadSettings = (EasySaveSettings)settings.Clone();
        loadSettings.compressionType = EasySave.CompressionType.None;
        loadSettings.encryptionType = EasySave.EncryptionType.None;

        cachedFiles[settings.path] = new EasySaveFile(EasySave.LoadRawBytes(loadSettings), settings);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static void Store(EasySaveSettings settings = null)
    {
        if (settings == null)
            settings = new EasySaveSettings(EasySave.Location.File);
        // If we're still using cached settings, set it to the default location.
        else if (settings.location == EasySave.Location.Cache)
        {
            settings = (EasySaveSettings)settings.Clone();
            // If the default settings are also set to cache, assume EasySave.Location.File. Otherwise, set it to the default location.
            settings.location = EasySaveSettings.defaultSettings.location == EasySave.Location.Cache ? EasySave.Location.File : EasySaveSettings.defaultSettings.location;
        }

        EasySaveFile cachedFile;
        if (!cachedFiles.TryGetValue(settings.path, out cachedFile))
            throw new FileNotFoundException("The file '" + settings.path + "' could not be stored because it could not be found in the cache.");
        cachedFile.Sync(settings);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static void RemoveCachedFile(EasySaveSettings settings)
    {
        cachedFiles.Remove(settings.path);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static void CopyCachedFile(EasySaveSettings oldSettings, EasySaveSettings newSettings)
    {
        EasySaveFile cachedFile;
        if (!cachedFiles.TryGetValue(oldSettings.path, out cachedFile))
            throw new FileNotFoundException("The file '" + oldSettings.path + "' could not be copied because it could not be found in the cache.");
        if (cachedFiles.ContainsKey(newSettings.path))
            throw new InvalidOperationException("Cannot copy file '" + oldSettings.path + "' to '" + newSettings.path + "' because '" + newSettings.path + "' already exists");

        cachedFiles.Add(newSettings.path, (EasySaveFile)cachedFile.MemberwiseClone());
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static void DeleteKey(string key, EasySaveSettings settings)
    {
        EasySaveFile cachedFile;
        if (cachedFiles.TryGetValue(settings.path, out cachedFile))
            cachedFile.DeleteKey(key);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static bool KeyExists(string key, EasySaveSettings settings)
    {
        EasySaveFile cachedFile;
        if (cachedFiles.TryGetValue(settings.path, out cachedFile))
            return cachedFile.KeyExists(key);
        return false;
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static bool FileExists(EasySaveSettings settings)
    {
        return cachedFiles.ContainsKey(settings.path);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static string[] GetKeys(EasySaveSettings settings)
    {
        EasySaveFile cachedFile;
        if (!cachedFiles.TryGetValue(settings.path, out cachedFile))
            throw new FileNotFoundException("Could not get keys from the file '" + settings.path + "' because it could not be found in the cache.");
        return cachedFile.cache.Keys.ToArray();
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal static string[] GetFiles()
    {
        return cachedFiles.Keys.ToArray();
    }

    internal static DateTime GetTimestamp(EasySaveSettings settings)
    {
        EasySaveFile cachedFile;
        if (!cachedFiles.TryGetValue(settings.path, out cachedFile))
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        return cachedFile.timestamp;
    }
}

namespace Convention.EasySave.Internal
{
    public struct EasySaveData
    {
        public EasySaveType type;
        public byte[] bytes;

        public EasySaveData(Type type, byte[] bytes)
        {
            this.type = type == null ? null : EasySaveTypeMgr.GetOrCreateEasySaveType(type);
            this.bytes = bytes;
        }

        public EasySaveData(EasySaveType type, byte[] bytes)
        {
            this.type = type;
            this.bytes = bytes;
        }
    }
}
