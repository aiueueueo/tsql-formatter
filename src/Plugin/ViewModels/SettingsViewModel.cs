using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TSqlFormatter.Core;

namespace TSqlFormatter.Extension.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings window.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsManager _settingsManager;
        private readonly PresetManager _presetManager;
        private readonly Formatter _formatter;

        private FormatterSettings _settings;
        private string _selectedPreset = "Default";
        private string _previewInput = "select id, name, email from users u inner join orders o on u.id = o.user_id where u.status = 'active' and o.amount > 100";
        private string _previewOutput = "";
        private string _shortcutKey = "Ctrl+Shift+K";
        private bool _applyToSelection = false;

        public SettingsViewModel()
        {
            _settingsManager = new SettingsManager();
            _presetManager = new PresetManager();
            _settings = _settingsManager.LoadSettings();
            _formatter = new Formatter(_settings);

            // Initialize commands
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            ResetToDefaultCommand = new RelayCommand(ResetToDefault);
            ApplyPresetCommand = new RelayCommand(ApplyPreset);

            // Initialize presets
            Presets = new ObservableCollection<string>(_presetManager.PresetNames);

            // Update preview
            UpdatePreview();
        }

        #region Properties

        /// <summary>
        /// Gets or sets whether to use tabs for indentation.
        /// </summary>
        public bool UseTab
        {
            get => _settings.UseTab;
            set
            {
                if (_settings.UseTab != value)
                {
                    _settings.UseTab = value;
                    OnPropertyChanged();
                    UpdatePreview();
                }
            }
        }

        /// <summary>
        /// Gets or sets the indent size (spaces).
        /// </summary>
        public int IndentSize
        {
            get => _settings.IndentSize;
            set
            {
                if (_settings.IndentSize != value)
                {
                    _settings.IndentSize = value;
                    OnPropertyChanged();
                    UpdatePreview();
                }
            }
        }

        /// <summary>
        /// Gets or sets the keyword casing option.
        /// </summary>
        public KeywordCasing KeywordCasing
        {
            get => _settings.KeywordCasing;
            set
            {
                if (_settings.KeywordCasing != value)
                {
                    _settings.KeywordCasing = value;
                    OnPropertyChanged();
                    UpdatePreview();
                }
            }
        }

        /// <summary>
        /// Gets or sets the comma placement option.
        /// </summary>
        public CommaPlacement CommaPlacement
        {
            get => _settings.CommaPlacement;
            set
            {
                if (_settings.CommaPlacement != value)
                {
                    _settings.CommaPlacement = value;
                    OnPropertyChanged();
                    UpdatePreview();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to add spaces around operators.
        /// </summary>
        public bool SpaceAroundOperators
        {
            get => _settings.SpaceAroundOperators;
            set
            {
                if (_settings.SpaceAroundOperators != value)
                {
                    _settings.SpaceAroundOperators = value;
                    OnPropertyChanged();
                    UpdatePreview();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to force the AS keyword for aliases.
        /// </summary>
        public bool ForceAsKeyword
        {
            get => _settings.ForceAsKeyword;
            set
            {
                if (_settings.ForceAsKeyword != value)
                {
                    _settings.ForceAsKeyword = value;
                    OnPropertyChanged();
                    UpdatePreview();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to place each clause on a new line.
        /// </summary>
        public bool NewLinePerClause
        {
            get => _settings.NewLinePerClause;
            set
            {
                if (_settings.NewLinePerClause != value)
                {
                    _settings.NewLinePerClause = value;
                    OnPropertyChanged();
                    UpdatePreview();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to place JOINs on separate lines.
        /// </summary>
        public bool JoinOnSeparateLine
        {
            get => _settings.JoinOnSeparateLine;
            set
            {
                if (_settings.JoinOnSeparateLine != value)
                {
                    _settings.JoinOnSeparateLine = value;
                    OnPropertyChanged();
                    UpdatePreview();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected preset name.
        /// </summary>
        public string SelectedPreset
        {
            get => _selectedPreset;
            set => SetProperty(ref _selectedPreset, value);
        }

        /// <summary>
        /// Gets the available presets.
        /// </summary>
        public ObservableCollection<string> Presets { get; }

        /// <summary>
        /// Gets or sets the shortcut key.
        /// </summary>
        public string ShortcutKey
        {
            get => _shortcutKey;
            set => SetProperty(ref _shortcutKey, value);
        }

        /// <summary>
        /// Gets or sets whether to apply formatting to selection only.
        /// </summary>
        public bool ApplyToSelection
        {
            get => _applyToSelection;
            set => SetProperty(ref _applyToSelection, value);
        }

        /// <summary>
        /// Gets or sets the preview input SQL.
        /// </summary>
        public string PreviewInput
        {
            get => _previewInput;
            set
            {
                if (SetProperty(ref _previewInput, value))
                {
                    UpdatePreview();
                }
            }
        }

        /// <summary>
        /// Gets the preview output SQL.
        /// </summary>
        public string PreviewOutput
        {
            get => _previewOutput;
            private set => SetProperty(ref _previewOutput, value);
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetToDefaultCommand { get; }
        public ICommand ApplyPresetCommand { get; }

        #endregion

        #region Events

        public event EventHandler? RequestClose;

        #endregion

        #region Methods

        private void Save()
        {
            _settingsManager.SavePersonalSettings(_settings);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void ResetToDefault()
        {
            _settings = FormatterSettings.Default;
            RefreshAllProperties();
            UpdatePreview();
        }

        private void ApplyPreset()
        {
            var preset = _presetManager.GetPreset(SelectedPreset);
            if (preset != null)
            {
                _settings = new FormatterSettings
                {
                    UseTab = preset.UseTab,
                    IndentSize = preset.IndentSize,
                    KeywordCasing = preset.KeywordCasing,
                    CommaPlacement = preset.CommaPlacement,
                    SpaceAroundOperators = preset.SpaceAroundOperators,
                    ForceAsKeyword = preset.ForceAsKeyword,
                    NewLinePerClause = preset.NewLinePerClause,
                    JoinOnSeparateLine = preset.JoinOnSeparateLine
                };
                RefreshAllProperties();
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            try
            {
                var formatter = new Formatter(_settings);
                PreviewOutput = formatter.Format(_previewInput) ?? string.Empty;
            }
            catch (Exception ex)
            {
                PreviewOutput = $"Error: {ex.Message}";
            }
        }

        private void RefreshAllProperties()
        {
            OnPropertyChanged(nameof(UseTab));
            OnPropertyChanged(nameof(IndentSize));
            OnPropertyChanged(nameof(KeywordCasing));
            OnPropertyChanged(nameof(CommaPlacement));
            OnPropertyChanged(nameof(SpaceAroundOperators));
            OnPropertyChanged(nameof(ForceAsKeyword));
            OnPropertyChanged(nameof(NewLinePerClause));
            OnPropertyChanged(nameof(JoinOnSeparateLine));
        }

        /// <summary>
        /// Gets the current settings.
        /// </summary>
        public FormatterSettings GetSettings() => _settings;

        #endregion
    }
}
