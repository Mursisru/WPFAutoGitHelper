using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfAutoGitHelper.Localization;
using WpfAutoGitHelper.Models;
using WpfAutoGitHelper.Services;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed class NewRepositoryDialogViewModel : INotifyPropertyChanged
    {
        private string _parentDirectory;
        private string _name = "";
        private string _description = "";
        private TemplateOption _selectedGitignore;
        private TemplateOption _selectedLicense;
        private bool _addReadme = true;
        private bool _isPublic = true;

        public NewRepositoryDialogViewModel(string initialParentDirectory)
        {
            _parentDirectory = string.IsNullOrWhiteSpace(initialParentDirectory)
                ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                : initialParentDirectory.Trim();

            GitignoreOptions = new ObservableCollection<TemplateOption>(RepoScaffoldCatalog.GitignoreOptions);
            LicenseOptions = new ObservableCollection<TemplateOption>(RepoScaffoldCatalog.LicenseOptions);
            _selectedGitignore = GitignoreOptions.FirstOrDefault();
            _selectedLicense = LicenseOptions.FirstOrDefault();

            GhAvailable = GitHubCliRunner.IsAvailable();

            BrowseParentCommand = new RelayCommand(BrowseParent);
        }

        public ObservableCollection<TemplateOption> GitignoreOptions { get; }
        public ObservableCollection<TemplateOption> LicenseOptions { get; }

        public bool GhAvailable { get; }

        public bool CanCreate => GhAvailable;

        public string Title => Loc.Get("Dlg_NewRepo_Title");

        public string LabelName => Loc.Get("NewRepo_Name");
        public string LabelDescription => Loc.Get("NewRepo_Description");
        public string LabelParent => Loc.Get("NewRepo_ParentFolder");
        public string LabelGitignore => Loc.Get("NewRepo_Gitignore");
        public string LabelLicense => Loc.Get("NewRepo_License");
        public string LabelReadme => Loc.Get("NewRepo_Readme");
        public string LabelVisibility => Loc.Get("NewRepo_Visibility");
        public string VisibilityPublic => Loc.Get("NewRepo_Visibility_Public");
        public string VisibilityPrivate => Loc.Get("NewRepo_Visibility_Private");
        public string GhHint => GhAvailable ? Loc.Get("NewRepo_GhHint") : Loc.Get("NewRepo_GhMissing");
        public string TargetPathHint => string.Format(Loc.Get("NewRepo_TargetPath"), PreviewPath);
        public string BtnBrowse => Loc.Get("Btn_Browse");
        public string BtnCreate => Loc.Get("NewRepo_BtnCreate");
        public string BtnCancel => Loc.Get("NewRepo_BtnCancel");

        public string ParentDirectory
        {
            get => _parentDirectory;
            set
            {
                if (_parentDirectory == value)
                    return;
                _parentDirectory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TargetPathHint));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;
                _name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TargetPathHint));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description == value)
                    return;
                _description = value;
                OnPropertyChanged();
            }
        }

        public TemplateOption SelectedGitignore
        {
            get => _selectedGitignore;
            set
            {
                if (_selectedGitignore == value)
                    return;
                _selectedGitignore = value;
                OnPropertyChanged();
            }
        }

        public TemplateOption SelectedLicense
        {
            get => _selectedLicense;
            set
            {
                if (_selectedLicense == value)
                    return;
                _selectedLicense = value;
                OnPropertyChanged();
            }
        }

        public bool AddReadme
        {
            get => _addReadme;
            set
            {
                if (_addReadme == value)
                    return;
                _addReadme = value;
                OnPropertyChanged();
            }
        }

        public bool IsPublic
        {
            get => _isPublic;
            set
            {
                if (_isPublic == value)
                    return;
                _isPublic = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPrivate));
            }
        }

        public bool IsPrivate
        {
            get => !_isPublic;
            set => IsPublic = !value;
        }

        public string PreviewPath
        {
            get
            {
                var safeName = RepoScaffoldService.SanitizeRepoName(Name);
                if (string.IsNullOrWhiteSpace(ParentDirectory) || string.IsNullOrWhiteSpace(safeName))
                    return "—";
                return Path.Combine(ParentDirectory.Trim(), safeName);
            }
        }

        public ICommand BrowseParentCommand { get; }

        public bool TryBuildRequest(out NewRepositoryRequest request, out string errorKey)
        {
            request = null;
            errorKey = null;

            if (!GhAvailable)
            {
                errorKey = "Msg_NewRepo_GhRequired";
                return false;
            }

            var safeName = RepoScaffoldService.SanitizeRepoName(Name);
            if (string.IsNullOrWhiteSpace(safeName))
            {
                errorKey = "Msg_NewRepo_InvalidName";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ParentDirectory) || !Directory.Exists(ParentDirectory))
            {
                errorKey = "Msg_NewRepo_InvalidParent";
                return false;
            }

            var parent = ParentDirectory.Trim();
            var fullPath = Path.Combine(parent, safeName);

            if (GitRunner.IsGitRepository(parent))
            {
                errorKey = "Msg_NewRepo_ParentIsGit";
                return false;
            }

            var outerGit = RepoFileCopy.FindContainingGitRoot(parent);
            if (!string.IsNullOrEmpty(outerGit) &&
                !string.Equals(Path.GetFullPath(outerGit), Path.GetFullPath(fullPath), StringComparison.OrdinalIgnoreCase))
            {
                errorKey = "Msg_NewRepo_NestedInGit";
                return false;
            }

            if (Directory.Exists(fullPath))
            {
                errorKey = GitRunner.IsGitRepository(fullPath)
                    ? "Msg_FolderAlreadyGit"
                    : "Msg_NewRepo_FolderExists";
                return false;
            }

            request = new NewRepositoryRequest
            {
                ParentDirectory = parent,
                Name = safeName,
                Description = Description?.Trim() ?? "",
                GitignoreId = SelectedGitignore?.Id ?? "none",
                LicenseId = SelectedLicense?.Id ?? "none",
                AddReadme = AddReadme,
                IsPrivate = IsPrivate,
            };
            return true;
        }

        private void BrowseParent()
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = Loc.Get("Dlg_NewRepo_ParentFolder"),
                SelectedPath = Directory.Exists(ParentDirectory)
                    ? ParentDirectory
                    : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ShowNewFolderButton = true,
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ParentDirectory = dlg.SelectedPath;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
