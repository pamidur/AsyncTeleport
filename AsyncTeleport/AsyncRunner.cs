using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Linq;

namespace System
{
    public class AsyncTeleport
    {
        private readonly List<Func<CancellationToken, Task>> _waitList = new List<Func<CancellationToken, Task>>();

        private AsyncTeleport()
        { }

        internal void AddTerminator(Func<CancellationToken, Task> task)
        {
            _waitList.Add(task);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        internal async Task Execute(Func<CancellationToken, Task> task)
        {
            await Execute(ct => VoidWrapper(task, ct));
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        private async Task<bool> VoidWrapper(Func<CancellationToken, Task> task, CancellationToken ct)
        {
            await task(ct);
            return false;
        }

#if !DEBUG
        [DebuggerHidden]
        [DebuggerStepThrough]
#endif
        internal async Task<T> Execute<T>(Func<CancellationToken, Task<T>> task)
        {
            var result = default(T);

            var tcs = new CancellationTokenSource();

            var mainTask = task(tcs.Token);

            var tasksToWait = new List<Task> { mainTask };
            tasksToWait.AddRange(_waitList.Select(t => t(tcs.Token)));

            var terminator = WaitForTerminators(tasksToWait, tcs);

            await Task.WhenAll(new[] { terminator, mainTask });

            if (mainTask.Status == TaskStatus.RanToCompletion)
                result = mainTask.GetAwaiter().GetResult();

            return result;
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        private async Task WaitForTerminators(List<Task> terminators, CancellationTokenSource cts)
        {
            await Task.WhenAny(terminators);
            cts.Cancel();
        }

        public static AsyncTeleport New()
        {
            return new AsyncTeleport();
        }
    }
}
