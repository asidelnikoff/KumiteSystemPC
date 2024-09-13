using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Text.Json;

namespace SharedComponentsLibrary
{
    public partial class UserSettings : ObservableObject
    {
        [ObservableProperty]
        string dataPath;

        [ObservableProperty]
        string databasePath;

        [ObservableProperty]
        string endOfMatchSound;

        [ObservableProperty]
        string warningSound;

        [ObservableProperty]
        int externalMonitorIndex;

        [ObservableProperty]
        int tatami;

        [ObservableProperty]
        bool isAutoLoadNextMatchEnabled;

        [ObservableProperty]
        bool isNextMatchShownOnExternalBoard;

        [ObservableProperty]
        Language language;

        [ObservableProperty]
        int externaBoardDesign;

        public static string DefaultSettingsFileName = "defaultsettings.json";
        public static string UserSettingsFileName = "usersettings.json";
        public static UserSettings GetUserSettings()
        {
            try
            {
                using (var fs = new FileStream(UserSettingsFileName, FileMode.Open))
                {
                    var result = JsonSerializer.Deserialize<UserSettings>(fs);
                    if (result == null)
                        throw new Exception();

                    return result;
                }
            }
            catch
            {
                try
                {
                    using (var fs = new FileStream(DefaultSettingsFileName, FileMode.Open))
                    {
                        var result = JsonSerializer.Deserialize<UserSettings>(fs);
                        if (result != null)
                            return result;
                    }
                }
                catch
                {
                    using (FileStream fs = new FileStream(DefaultSettingsFileName, FileMode.Create))
                    {
                        var defaultSettings = new UserSettings()
                        {
                            DataPath = "",
                            DatabasePath = "",
                            EndOfMatchSound = "",
                            WarningSound = "",
                            ExternalMonitorIndex = 0,
                            Tatami = 1,
                            IsAutoLoadNextMatchEnabled = false,
                            IsNextMatchShownOnExternalBoard = false,
                            Language = new Language() { Name = "English", CultureInfo = "en-GB" },
                            ExternaBoardDesign = 1
                        };
                        JsonSerializer.Serialize<UserSettings>(fs, defaultSettings);
                        return defaultSettings;
                    }

                }
            }

            return null;
        }

        public void Save()
        {
            using (FileStream fs = new FileStream(UserSettingsFileName, FileMode.Create))
                JsonSerializer.Serialize<UserSettings>(fs, this);
        }
    }
}
