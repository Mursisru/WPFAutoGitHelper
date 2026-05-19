using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfAutoGitHelper.Models
{
    public sealed class ChangedFileEntry : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string StatusCode { get; set; } = "";
        public string FilePath { get; set; } = "";
        public bool IsUntracked => StatusCode == "??";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public string StatusDisplay => string.IsNullOrEmpty(StatusCode) ? "?" : StatusCode.Trim();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
