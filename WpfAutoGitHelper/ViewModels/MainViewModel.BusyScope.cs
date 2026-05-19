using System;
using System.Threading;
using System.Threading.Tasks;

namespace WpfAutoGitHelper.ViewModels
{
    public sealed partial class MainViewModel
    {
        private int _busyScopeDepth;
        private bool _suppressGitCommandBusy;

        private void EnterBusyScope()
        {
            _busyScopeDepth++;
            if (_busyScopeDepth != 1)
                return;

            IsBusy = true;
            _operationCts?.Cancel();
            _operationCts = new CancellationTokenSource();
        }

        private void ExitBusyScope()
        {
            if (_busyScopeDepth <= 0)
                return;

            _busyScopeDepth--;
            if (_busyScopeDepth == 0)
                IsBusy = false;
        }

        private void SetBusyFromGitCommand(bool busy)
        {
            if (_suppressGitCommandBusy)
                return;

            if (busy)
                EnterBusyScope();
            else
                ExitBusyScope();
        }

        private async Task RunWithBusyAsync(Func<Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            EnterBusyScope();
            var previousSuppress = _suppressGitCommandBusy;
            _suppressGitCommandBusy = true;
            try
            {
                await action().ConfigureAwait(true);
            }
            finally
            {
                _suppressGitCommandBusy = previousSuppress;
                ExitBusyScope();
            }
        }
    }
}
