public class EasySaveDefaults
{
    public EasySaveSerializableSettings settings = new EasySaveSerializableSettings();

    public bool addMgrToSceneAutomatically = false;
    public bool autoUpdateReferences = true;
    public bool addAllPrefabsToManager = true;
    public int collectDependenciesDepth = 4;
    public int collectDependenciesTimeout = 10;
    public bool updateReferencesWhenSceneChanges = true;
    public bool updateReferencesWhenSceneIsSaved = true;
    public bool updateReferencesWhenSceneIsOpened = true;
    public string[] referenceFolders = new string[0];

    public bool logDebugInfo = false;
    public bool logWarnings = true;
    public bool logErrors = true;
}