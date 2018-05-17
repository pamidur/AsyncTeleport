using System.Diagnostics;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    public static class AsyncTeleportExtensions
    {
        private static readonly TaskCompletionSource<bool> _exitTcs;

        static AsyncTeleportExtensions()
        {
            _exitTcs = new TaskCompletionSource<bool>();
            AssemblyLoadContext.Default.Unloading += c => _exitTcs.SetResult(true);
        }

        public static AsyncTeleport CancelOn(this AsyncTeleport runner, Action method)
        {
            runner.AddTerminator(ct => Task.Run(method, ct));
            return runner;
        }

        public static AsyncTeleport CancelOn(this AsyncTeleport runner, Func<CancellationToken, Task> task)
        {
            runner.AddTerminator(task);
            return runner;
        }

        public static AsyncTeleport CancelOnSigTerm(this AsyncTeleport runner, Action react = null)
        {
            var term = _exitTcs.Task;

            if (react != null)
                term = term.ContinueWith(t => { react(); return t.Result; });

            runner.AddTerminator(ct => _exitTcs.Task);
            return runner;
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        public static async Task Wait(this CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
                try
                {
                    await Task.Delay(Int32.MaxValue, ct).ContinueWith(t => { }, TaskContinuationOptions.OnlyOnCanceled);
                }
                catch (TaskCanceledException) { }
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        public static T Run<T>(this AsyncTeleport runner, Func<CancellationToken, Task<T>> task)
        {
            return ExecuteSync(runner, task);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        public static T Run<T, TA1>(this AsyncTeleport runner, Func<TA1, CancellationToken, Task<T>> task, TA1 a1)
        {
            return ExecuteSync(runner, ct => task(a1, ct));
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        public static T Run<T, TA1, TA2>(this AsyncTeleport runner, Func<TA1, TA2, CancellationToken, Task<T>> task, TA1 a1, TA2 a2)
        {
            return ExecuteSync(runner, ct => task(a1, a2, ct));
        }


        [DebuggerHidden]
        [DebuggerStepThrough]
        public static void Run(this AsyncTeleport runner, Func<CancellationToken, Task> task)
        {
            ExecuteSync(runner, task);
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        public static void Run<TA1>(this AsyncTeleport runner, Func<TA1, CancellationToken, Task> task, TA1 a1)
        {
            ExecuteSync(runner, ct => task(a1, ct));
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        public static void Run<TA1, TA2>(this AsyncTeleport runner, Func<TA1, TA2, CancellationToken, Task> task, TA1 a1, TA2 a2)
        {
            ExecuteSync(runner, ct => task(a1, a2, ct));
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        private static T ExecuteSync<T>(AsyncTeleport runner, Func<CancellationToken, Task<T>> task)
        {
            return runner.Execute(task).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        private static void ExecuteSync(AsyncTeleport runner, Func<CancellationToken, Task> task)
        {
            runner.Execute(task).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
