using System;
using System.Threading.Tasks;
using WpfAutoGitHelper.Models;

namespace WpfAutoGitHelper.Services
{
    public sealed class DangerousOperationGuard
    {
        public Func<string, string, Task<bool>> ConfirmYesNoAsync { get; set; }
        public Action<DangerousOperationRecord> RecordOperation { get; set; }

        public async Task<bool> ConfirmAsync(
            DangerousOperationType type,
            string title,
            string preview,
            string undoHint = null)
        {
            var body = preview ?? "";
            if (!string.IsNullOrWhiteSpace(undoHint))
                body += Environment.NewLine + Environment.NewLine + undoHint;

            var ok = ConfirmYesNoAsync != null && await ConfirmYesNoAsync(title, body).ConfigureAwait(false);
            if (ok)
            {
                RecordOperation?.Invoke(new DangerousOperationRecord
                {
                    Timestamp = DateTime.Now,
                    OperationType = type,
                    Summary = title,
                    Details = preview ?? "",
                    UndoHint = undoHint ?? "",
                });
            }

            return ok;
        }
    }
}
