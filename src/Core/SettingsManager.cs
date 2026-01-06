using Newtonsoft.Json;
using System;
using System.IO;

namespace TSqlFormatter.Core
{
    /// <summary>
    /// Manages loading and saving of formatter settings.
    /// Implements hybrid settings: project settings override personal settings.
    /// </summary>
    public class SettingsManager
    {
        private const string ProjectSettingsFileName = ".sqlformatter.json";
        private const string PersonalSettingsFolderName = "T-SQL Formatter";
        private const string PersonalSettingsFileName = "settings.json";

        /// <summary>
        /// Gets the path to the personal settings file.
        /// </summary>
        public static string PersonalSettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            PersonalSettingsFolderName,
            PersonalSettingsFileName);

        /// <summary>
        /// Loads settings using the hybrid approach.
        /// Project settings take priority over personal settings.
        /// </summary>
        /// <param name="projectPath">The path to the project directory.</param>
        /// <returns>The loaded settings, or default settings if none found.</returns>
        public FormatterSettings LoadSettings(string? projectPath = null)
        {
            // Try project settings first
            if (!string.IsNullOrEmpty(projectPath))
            {
                var projectSettingsPath = Path.Combine(projectPath, ProjectSettingsFileName);
                if (File.Exists(projectSettingsPath))
                {
                    var settings = LoadFromFile(projectSettingsPath);
                    if (settings != null) return settings;
                }
            }

            // Fall back to personal settings
            if (File.Exists(PersonalSettingsPath))
            {
                var settings = LoadFromFile(PersonalSettingsPath);
                if (settings != null) return settings;
            }

            // Return default settings
            return FormatterSettings.Default;
        }

        /// <summary>
        /// Saves settings to the personal settings file.
        /// </summary>
        /// <param name="settings">The settings to save.</param>
        public void SavePersonalSettings(FormatterSettings settings)
        {
            var directory = Path.GetDirectoryName(PersonalSettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            SaveToFile(settings, PersonalSettingsPath);
        }

        /// <summary>
        /// Saves settings to a project settings file.
        /// </summary>
        /// <param name="settings">The settings to save.</param>
        /// <param name="projectPath">The project directory path.</param>
        public void SaveProjectSettings(FormatterSettings settings, string projectPath)
        {
            var projectSettingsPath = Path.Combine(projectPath, ProjectSettingsFileName);
            SaveToFile(settings, projectSettingsPath);
        }

        private FormatterSettings? LoadFromFile(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<FormatterSettings>(json);
            }
            catch
            {
                return null;
            }
        }

        private void SaveToFile(FormatterSettings settings, string path)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
