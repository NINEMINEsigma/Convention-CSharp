using Convention.EasySave.Internal;

public class EasySaveSettings : System.ICloneable
{

    #region Default settings
    private static EasySaveSettings _defaults = null;
    private static EasySaveDefaults _defaultSettingsScriptableObject;
    private const string defaultSettingsPath = "EasySave/EasySaveDefaults";

    public static EasySaveDefaults defaultSettingsScriptableObject
    {
        get
        {
            if (_defaultSettingsScriptableObject == null)
            {
                _defaultSettingsScriptableObject = Resources.Load<EasySaveDefaults>(defaultSettingsPath);

#if UNITY_EDITOR
                if (_defaultSettingsScriptableObject == null)
                {
                    _defaultSettingsScriptableObject = ScriptableObject.CreateInstance<ES3Defaults>();

                    // If this is the version being submitted to the Asset Store, don't include ES3Defaults.
                    if (Application.productName.Contains("EasySave Release"))
                    {
                        Debug.Log("This has been identified as a release build as the title contains 'EasySave Release', so ES3Defaults will not be created.");
                        return _defaultSettingsScriptableObject;
                    }

                    // Convert the old settings to the new settings if necessary.
                    var oldSettings = GetOldSettings();
                    if (oldSettings != null)
                    {
                        oldSettings.CopyInto(_defaultSettingsScriptableObject.settings);
                        // Only enable warning logs by default for new installs as this may look like unexpected behaviour to some.
                        _defaultSettingsScriptableObject.logWarnings = false;
                        RemoveOldSettings();
                    }

                    CreateDefaultSettingsFolder();
                    AssetDatabase.CreateAsset(_defaultSettingsScriptableObject, PathToDefaultSettings());
                    AssetDatabase.SaveAssets();
                }
#endif
            }
            return _defaultSettingsScriptableObject;
        }
    }

    public static EasySaveSettings defaultSettings
    {
        get
        {
            if(_defaults == null)
            {
                if(defaultSettingsScriptableObject != null)
                    _defaults = defaultSettingsScriptableObject.settings;
            }
            return _defaults;
        }
    }

    private static EasySaveSettings _unencryptedUncompressedSettings = null; 
    internal static EasySaveSettings unencryptedUncompressedSettings
    {
        get
        {
            if (_unencryptedUncompressedSettings == null)
                _unencryptedUncompressedSettings = new EasySaveSettings(EasySave.EncryptionType.None, EasySave.CompressionType.None);
            return _unencryptedUncompressedSettings;
        }
    }

    #endregion

    #region Fields

    private static readonly string[] resourcesExtensions = new string[]{".txt", ".htm", ".html", ".xml", ".bytes", ".json", ".csv", ".yaml", ".fnt" };

	[SerializeField]
	private EasySave.Location _location;
	/// <summary>The location where we wish to store data. As it's not possible to save/load from File in WebGL, if the default location is File it will use PlayerPrefs instead.</summary>
	public EasySave.Location location
	{
		get
		{
			if(_location == EasySave.Location.File && (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.tvOS))
				return EasySave.Location.PlayerPrefs;
			return _location;
		}
		set{ _location = value; }
	}

	/// <summary>The path associated with this EasySaveSettings object, if any.</summary>
	public string path = "SaveFile.es3";
	/// <summary>The type of encryption to use when encrypting data, if any.</summary>
	public EasySave.EncryptionType encryptionType = EasySave.EncryptionType.None;
    /// <summary>The type of encryption to use when encrypting data, if any.</summary>
	public EasySave.CompressionType compressionType = EasySave.CompressionType.None;
    /// <summary>The password to use when encrypting data.</summary>
    public string encryptionPassword = "password";
	/// <summary>The default directory in which to store files, and the location which relative paths should be relative to.</summary>
	public EasySave.Directory directory = EasySave.Directory.PersistentDataPath;
	/// <summary>What format to use when serialising and deserialising data.</summary>
	public EasySave.Format format = EasySave.Format.JSON;
    /// <summary>Whether we want to pretty print JSON.</summary>
	public bool prettyPrint = true;
    /// <summary>Any stream buffers will be set to this length in bytes.</summary>
    public int bufferSize = 2048;
	/// <summary>The text encoding to use for text-based format. Note that changing this may invalidate previous save data.</summary>
	public System.Text.Encoding encoding = System.Text.Encoding.UTF8;
    // <summary>Whether we should serialise children when serialising a GameObject.</summary>
    public bool saveChildren = true;
    // <summary>Whether we should apply encryption and/or compression to raw cached data if they're specified in the cached data's settings.</summary>
    public bool postprocessRawCachedData = false;

    /// <summary>Whether we should check that the data we are loading from a file matches the method we are using to load it.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool typeChecking = true;

	/// <summary>Enabling this ensures that only serialisable fields are serialised. Otherwise, possibly unsafe fields and properties will be serialised.</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool safeReflection = true;
	/// <summary>Whether UnityEngine.Object members should be stored by value, reference or both.</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public EasySave.ReferenceMode memberReferenceMode = EasySave.ReferenceMode.ByRef;
	/// <summary>Whether the main save methods should save UnityEngine.Objects by value, reference, or both.</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public EasySave.ReferenceMode referenceMode = EasySave.ReferenceMode.ByRefAndValue;

    /// <summary>How many levels of hierarchy Easy Save will serialise. This is used to protect against cyclic references.</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public int serializationDepthLimit = 64;

    /// <summary>The names of the Assemblies we should try to load our Convention.EasySave.Types from.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public string[] assemblyNames = new string[] { "Assembly-CSharp-firstpass", "Assembly-CSharp"};

    /// <summary>Gets the full, absolute path which this EasySaveSettings object identifies.</summary>
    public string FullPath
	{
		get
		{
            if (path == null)
                throw new System.NullReferenceException("The 'path' field of this EasySaveSettings is null, indicating that it was not possible to load the default settings from Resources. Please check that the EasySave Default Settings.prefab exists in Assets/Plugins/Resources/EasySave/");

			if(IsAbsolute(path))
				return path;

			if(location == EasySave.Location.File)
			{
				if(directory == EasySave.Directory.PersistentDataPath)
					return EasySaveIO.persistentDataPath + "/" + path;
				if(directory == EasySave.Directory.DataPath)
					return Application.dataPath + "/" + path;
				throw new System.NotImplementedException("File directory \""+directory+"\" has not been implemented.");
			}
			if(location == EasySave.Location.Resources)
			{
                // Check that it has valid extension
                var extension = System.IO.Path.GetExtension(path);
                bool hasValidExtension = false;
                foreach (var ext in resourcesExtensions)
                {
                    if (extension == ext)
                    {
                        hasValidExtension = true;
                        break;
                    }
                }

                if(!hasValidExtension)
                    throw new System.ArgumentException("Extension of file in Resources must be .json, .bytes, .txt, .csv, .htm, .html, .xml, .yaml or .fnt, but path given was \"" + path + "\"");

                // Remove extension
                string resourcesPath = path.Replace(extension, "");
				return resourcesPath;
			}
			return path;
		}
	}

    #endregion

    #region Constructors

    /// <summary>Creates a new EasySaveSettings object with the given path.</summary>
    /// <param name="path">The path associated with this EasySaveSettings object.</param>
    /// <param name="settings">The settings we want to use to override the default settings.</param>
    public EasySaveSettings(string path = null, EasySaveSettings settings = null) : this(true)
    {
        // if there are settings to merge, merge them.
        if (settings != null)
            settings.CopyInto(this);

        if (path != null)
            this.path = path;
    }

    /// <summary>Creates a new EasySaveSettings object with the given path.</summary>
    /// <param name="path">The path associated with this EasySaveSettings object.</param>
    /// <param name="enums">Accepts an EasySave.EncryptionType, EasySave.CompressionType, EasySave.Location, EasySave.Directory or EasySave.ReferenceMode.</param>
    public EasySaveSettings(string path, params System.Enum[] enums) : this(enums)
    {
        if (path != null)
            this.path = path;
    }


    /// <summary>Creates a new EasySaveSettings object with the given path.</summary>
    /// <param name="path">The path associated with this EasySaveSettings object.</param>
    /// <param name="enums">Accepts an EasySave.EncryptionType, EasySave.CompressionType, EasySave.Location, EasySave.Directory or EasySave.ReferenceMode.</param>
    public EasySaveSettings(params System.Enum[] enums) : this(true)
    {
        foreach (var setting in enums)
        {
            if (setting is EasySave.EncryptionType)
                this.encryptionType = (EasySave.EncryptionType)setting;
            else if (setting is EasySave.Location)
                this.location = (EasySave.Location)setting;
            else if (setting is EasySave.CompressionType)
                this.compressionType = (EasySave.CompressionType)setting;
            else if (setting is EasySave.ReferenceMode)
                this.referenceMode = (EasySave.ReferenceMode)setting;
            else if (setting is EasySave.Format)
                this.format = (EasySave.Format)setting;
            else if (setting is EasySave.Directory)
                this.directory = (EasySave.Directory)setting;
        }
    }

    /// <summary>Creates a new EasySaveSettings object with the given encryption settings.</summary>
    /// <param name="encryptionType">The type of encryption to use, if any.</param>
    /// <param name="encryptionPassword">The password to use when encrypting data.</param>
    public EasySaveSettings(EasySave.EncryptionType encryptionType, string encryptionPassword) : this(true)
    {
        this.encryptionType = encryptionType;
        this.encryptionPassword = encryptionPassword;
    }

    /// <summary>Creates a new EasySaveSettings object with the given path and encryption settings.</summary>
    /// <param name="path">The path associated with this EasySaveSettings object.</param>
    /// <param name="encryptionType">The type of encryption to use, if any.</param>
    /// <param name="encryptionPassword">The password to use when encrypting data.</param>
    /// <param name="settings">The settings we want to use to override the default settings.</param>
    public EasySaveSettings(string path, EasySave.EncryptionType encryptionType, string encryptionPassword, EasySaveSettings settings = null) : this(path, settings)
    {
        this.encryptionType = encryptionType;
        this.encryptionPassword = encryptionPassword;
    }

    /* Base constructor which allows us to bypass defaults so it can be called by Editor serialization */
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public EasySaveSettings(bool applyDefaults)
    {
        if (applyDefaults)
            if (defaultSettings != null)
                _defaults.CopyInto(this);
    }

    #endregion

    #region Editor methods
#if UNITY_EDITOR
    public static string pathToEasySaveFolder = null;

    public static string PathToEasySaveFolder()
    {
        // If the path has not yet been cached, get the path and cache it.
        if (string.IsNullOrEmpty(pathToEasySaveFolder))
        {
            string[] guids = AssetDatabase.FindAssets("ES3Window");
            if (guids.Length == 0)
                ES3Debug.LogError("Could not locate the Easy Save 3 folder because the ES3Window script has been moved or removed.");
            if (guids.Length > 1)
                ES3Debug.LogError("Could not locate the Easy Save 3 folder because more than one ES3Window script exists in the project, but this needs to be unique to locate the folder.");

            pathToEasySaveFolder = AssetDatabase.GUIDToAssetPath(guids[0]).Split(new string[] { "Editor" }, System.StringSplitOptions.RemoveEmptyEntries)[0];
        }
        return pathToEasySaveFolder;
    }

    internal static string PathToDefaultSettings()
    {
        return PathToEasySaveFolder() + "Resources/"+defaultSettingsPath+".asset";
    }

    internal static void CreateDefaultSettingsFolder()
    {
        if (AssetDatabase.IsValidFolder(PathToEasySaveFolder() + "Resources/EasySave"))
            return;
        // Remove leading slash from PathToEasySaveFolder.
        AssetDatabase.CreateFolder(PathToEasySaveFolder().Remove(PathToEasySaveFolder().Length - 1, 1), "Resources");
        AssetDatabase.CreateFolder(PathToEasySaveFolder() + "Resources", "EasySave");
    }

    private static ES3SerializableSettings GetOldSettings()
    {
        var go = Resources.Load<GameObject>(defaultSettingsPath.Replace("ES3Defaults", "EasySave Default Settings"));
        if(go != null)
        {
            var c = go.GetComponent<ES3DefaultSettings>();
            if (c != null && c.settings != null)
                return c.settings;
        }
        return null;
    }

    private static void RemoveOldSettings()
    {
        AssetDatabase.DeleteAsset(PathToDefaultSettings().Replace("ES3Defaults.asset", "EasySave Default Settings.prefab"));
    }
#endif
    #endregion

    #region Utility methods

    private static bool IsAbsolute(string path)
    {
        if (path.Length > 0 && (path[0] == '/' || path[0] == '\\'))
            return true;
        if (path.Length > 1 && path[1] == ':')
            return true;
        return false;
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public object Clone()
    {
        var settings = new EasySaveSettings();
        CopyInto(settings);
        return settings;
    }

    private void CopyInto(EasySaveSettings newSettings)
    {
        newSettings._location = _location;
        newSettings.directory = directory;
        newSettings.format = format;
        newSettings.prettyPrint = prettyPrint;
        newSettings.path = path;
        newSettings.encryptionType = encryptionType;
        newSettings.encryptionPassword = encryptionPassword;
        newSettings.compressionType = compressionType;
        newSettings.bufferSize = bufferSize;
        newSettings.encoding = encoding;
        newSettings.typeChecking = typeChecking;
        newSettings.safeReflection = safeReflection;
        newSettings.referenceMode = referenceMode;
        newSettings.memberReferenceMode = memberReferenceMode;
        newSettings.assemblyNames = assemblyNames;
        newSettings.saveChildren = saveChildren;
        newSettings.serializationDepthLimit = serializationDepthLimit;
        newSettings.postprocessRawCachedData = postprocessRawCachedData;
    }

    #endregion
}

/*
 * 	A serializable version of the settings we can use as a field in the Editor, which doesn't automatically
 * 	assign defaults to itself, so we get no serialization errors.
 */
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[System.Serializable]
public class EasySaveSerializableSettings : EasySaveSettings
{
	public EasySaveSerializableSettings() : base(false){}
	public EasySaveSerializableSettings(bool applyDefaults) : base(applyDefaults){}
    public EasySaveSerializableSettings(string path) : base(false) { this.path = path; }
    public EasySaveSerializableSettings(string path, EasySave.Location location) : base(false) { this.location = location; }
}