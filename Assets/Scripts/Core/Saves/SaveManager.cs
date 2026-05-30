namespace STSLite.Core.Saves
{
    public class LocalSaveStore : ISaveStore
    {
        public LocalSaveStore(string profileId)
        {
        }
    }

    public interface ISaveStore
    {

    }

    public class SaveManager
    {
        
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ConstructDefault();
                }
                return _instance;
            }
        }
        private static SaveManager _instance;

        public SettingsSaveData SettingsSaveData { get; }

        public SaveManager(ISaveStore saveStore)
        {
            SettingsSaveData = new SettingsSaveData();
        }

        public static SaveManager ConstructDefault()
        {
            ISaveStore saveStore = new LocalSaveStore("user");
            return new SaveManager(saveStore);
        }

        public void TryFirstTimeCloudSync()
        {
        }
        public void SyncCloudToLocal()
        {
        }
    }

    public class SettingsSaveData
    {
        public float VolumeMaster { get; set; } = 0.5f;
        public float VolumeBGM { get; set; } = 0.5f;
        public float VolumeSFX { get; set; } = 0.5f;
    }


    public class AccountScopeUserDataMigrator
    {

    }
}