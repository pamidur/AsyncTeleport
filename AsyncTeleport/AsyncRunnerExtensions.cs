using System.Diagnostics;
#if NETCOREAPP2_0
using System.Runtime.Loader;
#endif
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
#if NETCOREAPP2_0
            AssemblyLoadContext.Default.Unloading += c => _exitTcs.SetResult(true);
#endif
            Console.CancelKeyPress += (s, o) => _exitTcs.SetResult(true);
        }

        /// <summary>
        /// Add sync method to terminators. Cancellation token will be triggered when the method exits.
        /// </summary>
        /// <param name="teleport">Existing teleport</param>
        /// <param name="method">Method to wait until cancel</param>
        public static AsyncTeleport CancelOn(this AsyncTeleport teleport, Action method)
        {
            teleport.AddTerminator(ct => Task.Run(method, ct));
            return teleport;
        }

        /// <summary>
        /// Add async task to terminators. Cancellation token will be triggered when the task exits.
        /// </summary>
        /// <param name="teleport">Existing teleport</param>
        /// <param name="task">Task to wait until cancel</param>
        public static AsyncTeleport CancelOn(this AsyncTeleport teleport, Func<CancellationToken, Task> task)
        {
            teleport.AddTerminator(task);
            return teleport;
        }

        /// <summary>
        /// Add graceful shutdown monitor to terminators. Cancellation token will be triggered when the process receives exit graceful signal.
        /// </summary>
        /// <param name="teleport">Existing teleport</param>
        /// <param name="react">Method to ecexute before SigTerm or CTRL+C is passed further</param>
        public static AsyncTeleport CancelOnGracefulShutdown(this AsyncTeleport teleport, Action react = null)
        {
            var term = _exitTcs.Task;

            if (react != null)
                term = term.ContinueWith(t => { react(); return true; });

            teleport.AddTerminator(ct => term);
            return teleport;
        }

        /// <summary>
        /// Creates a task that wait for cancellation.
        /// </summary>
        /// <param name="ct">existing CancellationToken</param>
        /// <returns>a task</returns>
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

        /// <summary>
        /// Run async Task and wait for it to finish.
        /// </summary>
        /// <param name="task">Task with cancellation token and return value</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static T Run<T>(this AsyncTeleport teleport, Func<CancellationToken, Task<T>> task)
        {
            return ExecuteSync(teleport, task);
        }

        /// <summary>
        /// Run async Task and wait for it to finish.
        /// </summary>
        /// <param name="task">Task with argument, cancellation token and return value</param>
        /// <param name="a1">Task's first argument.</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static T Run<T, TA1>(this AsyncTeleport teleport, Func<TA1, CancellationToken, Task<T>> task, TA1 a1)
        {
            return ExecuteSync(teleport, ct => task(a1, ct));
        }

        /// <summary>
        /// Run async Task and wait for it to finish.
        /// </summary>
        /// <param name="task">Task with two arguments, cancellation token and return value</param>
        /// <param name="a1">Task's first argument.</param>
        /// <param name="a2">Task's second argument.</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static T Run<T, TA1, TA2>(this AsyncTeleport teleport, Func<TA1, TA2, CancellationToken, Task<T>> task, TA1 a1, TA2 a2)
        {
            return ExecuteSync(teleport, ct => task(a1, a2, ct));
        }

        /// <summary>
        /// Run async Task and wait for it to finish.
        /// </summary>
        /// <param name="task">Task with cancellation token</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static void Run(this AsyncTeleport teleport, Func<CancellationToken, Task> task)
        {
            ExecuteSync(teleport, task);
        }

        /// <summary>
        /// Run async Task and wait for it to finish.
        /// </summary>
        /// <param name="task">Task with argument, cancellation token</param>
        /// <param name="a1">Task's first argument.</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static void Run<TA1>(this AsyncTeleport teleport, Func<TA1, CancellationToken, Task> task, TA1 a1)
        {
            ExecuteSync(teleport, ct => task(a1, ct));
        }

        /// <summary>
        /// Run async Task and wait for it to finish.
        /// </summary>
        /// <param name="task">Task with two arguments, cancellation token</param>
        /// <param name="a1">Task's first argument.</param>
        /// <param name="a2">Task's second argument.</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static void Run<TA1, TA2>(this AsyncTeleport teleport, Func<TA1, TA2, CancellationToken, Task> task, TA1 a1, TA2 a2)
        {
            ExecuteSync(teleport, ct => task(a1, a2, ct));
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        private static T ExecuteSync<T>(AsyncTeleport teleport, Func<CancellationToken, Task<T>> task)
        {
            return teleport.Execute(task).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        private static void ExecuteSync(AsyncTeleport teleport, Func<CancellationToken, Task> task)
        {
            teleport.Execute(task).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
