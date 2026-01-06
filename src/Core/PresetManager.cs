using System.Collections.Generic;

namespace TSqlFormatter.Core
{
    /// <summary>
    /// Manages formatting presets.
    /// </summary>
    public class PresetManager
    {
        private readonly Dictionary<string, FormatterSettings> _presets = new();

        public PresetManager()
        {
            InitializeBuiltInPresets();
        }

        /// <summary>
        /// Gets all available preset names.
        /// </summary>
        public IEnumerable<string> PresetNames => _presets.Keys;

        /// <summary>
        /// Gets a preset by name.
        /// </summary>
        /// <param name="name">The preset name.</param>
        /// <returns>The preset settings, or null if not found.</returns>
        public FormatterSettings? GetPreset(string name)
        {
            return _presets.TryGetValue(name, out var settings) ? settings : null;
        }

        /// <summary>
        /// Adds or updates a custom preset.
        /// </summary>
        /// <param name="name">The preset name.</param>
        /// <param name="settings">The preset settings.</param>
        public void SavePreset(string name, FormatterSettings settings)
        {
            _presets[name] = settings;
        }

        /// <summary>
        /// Removes a preset.
        /// </summary>
        /// <param name="name">The preset name.</param>
        /// <returns>True if removed, false if not found.</returns>
        public bool RemovePreset(string name)
        {
            return _presets.Remove(name);
        }

        private void InitializeBuiltInPresets()
        {
            // Default preset (as per specification)
            _presets["Default"] = new FormatterSettings
            {
                UseTab = true,
                IndentSize = 4,
                KeywordCasing = KeywordCasing.Uppercase,
                CommaPlacement = CommaPlacement.BeforeColumn,
                SpaceAroundOperators = true,
                ForceAsKeyword = true,
                NewLinePerClause = true,
                JoinOnSeparateLine = true
            };

            // Compact preset
            _presets["Compact"] = new FormatterSettings
            {
                UseTab = false,
                IndentSize = 2,
                KeywordCasing = KeywordCasing.Uppercase,
                CommaPlacement = CommaPlacement.AfterColumn,
                SpaceAroundOperators = true,
                ForceAsKeyword = false,
                NewLinePerClause = true,
                JoinOnSeparateLine = false
            };
        }
    }
}
