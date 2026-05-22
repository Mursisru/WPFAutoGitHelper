using System;

namespace WpfAutoGitHelper.Models
{
    public sealed class DangerousOperationRecord
    {
        public DateTime Timestamp { get; set; }
        public DangerousOperationType OperationType { get; set; }
        public string Summary { get; set; } = "";
        public string Details { get; set; } = "";
        public string UndoHint { get; set; } = "";
    }
}
