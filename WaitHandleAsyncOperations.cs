using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWaitHandle
{
    public static class WaitHandleAsyncOperations
    {
        /// <summary>
        /// Creates a task for waiting the <see cref="System.Threading.WaitHandle"/>, so you can write: "if (await waitHandle.WaitOneAsync()) ... else ..." or "await waitHandle.WaitOneAsync().ContinueWith(...)"
        /// </summary>
        /// <param name="waitHandle">An instance of <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, <see cref="System.Threading.Mutex"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="timeoutMs">The wait timeout in milliseconds. Set to -1 to wait indefinitely. On timeout the awaiter throws <see cref="System.TimeoutException"/>.</param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns a task which result tells if the <see cref="System.Threading.WaitHandle"/> signaled (True) or didn't (False), or throws the <see cref="System.OperationCanceledException"/></returns>
        public static Task<bool> WaitOneAsync(WaitHandle waitHandle, int timeoutMs, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<bool>();
            var awaiter = waitHandle.ConfigureAwait(timeoutMs, cancellationToken).GetAwaiter();
            Action completionAction = () => {
                if (awaiter.IsCanceled)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(!awaiter.IsTimedOut);
            };
            awaiter.OnCompleted(completionAction);
            return tcs.Task;
        }

        /// <summary>
        /// Waits for any of given <see cref="System.Threading.WaitHandle"/> to pulse. Unlike <see cref="System.Threading.WaitHandle.WaitAny"/>, this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one, and there is no restriction of maximum 64 handles.
        /// </summary>
        /// <param name="waitHandles">The collection of <see cref="System.Threading.WaitHandle"/>s to wait on: <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="timeoutMs">The wait timeout in milliseconds. Set to -1 to wait indefinitely. If all time out this method returns <see cref="System.Threading.WaitHandle.WaitTimeout"/>.</param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns the index of a <see cref="System.Threading.WaitHandle"/> in the given collection which pulsed first, or <see cref="System.Threading.WaitHandle.WaitTimeout"/> if all of them timed out. Note that this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one.</returns>
        public static Task<int> WaitAnyAsync(IEnumerable<WaitHandle> waitHandles, int timeoutMs, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (waitHandles == null)
                throw new ArgumentNullException(nameof(waitHandles));

            int totalCount = 0;
            foreach (var waitHandle in waitHandles) {
                if (waitHandle == null)
                    throw new ArgumentException($"The WaitHandle reference at index {totalCount} is NULL", nameof(waitHandles));
                ThrowIfMutex(waitHandle);
                totalCount++;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<int>();
            var awaiters = new WaitHandleAwaiter[totalCount];
            int failedCount = 0;
            int state = 0;

            var ctRegistration = default(CancellationTokenRegistration);
            if (cancellationToken.CanBeCanceled)
                ctRegistration = cancellationToken.Register(() => {
                    if (Interlocked.CompareExchange(ref state, 1, 0) == 0) {
                        ctRegistration.Dispose();
                        tcs.SetCanceled();
                    }
                },
                useSynchronizationContext: false);

            int index = 0;
            foreach (var waitHandle in waitHandles) {

                if (state != 0)
                    break;

                var awaiter = awaiters[index] = waitHandle.ConfigureAwait(timeoutMs, cancellationToken).GetAwaiter();

                if (state != 0) {
                    CancelAwaiter(awaiter);
                    break;
                }

                var awaiterIndex = index;

                awaiter.OnCompleted(() => {
                    if (cancellationToken.IsCancellationRequested) {
                        if (TryChangeState(ref state, expectedState: 0, newState: 1)) {
                            ctRegistration.Dispose();
                            tcs.SetCanceled();
                        }
                    } else if (awaiter.IsSignaled) {
                        if (TryChangeState(ref state, expectedState: 0, newState: 1)) {
                            CancelAwaiters(awaiters);
                            ctRegistration.Dispose();
                            tcs.SetResult(awaiterIndex);
                        }
                    } else {
                        if (Interlocked.Increment(ref failedCount) == totalCount) {
                            if (TryChangeState(ref state, expectedState: 0, newState: 1)) {
                                ctRegistration.Dispose();
                                tcs.SetResult(WaitHandle.WaitTimeout);
                            }
                        }
                    }
                });

                index++;
            }

            return tcs.Task;
        }

        /// <summary>
        /// Waits for all of given <see cref="System.Threading.WaitHandle"/> to pulse. Unlike <see cref="System.Threading.WaitHandle.WaitAll"/>, this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one, and there is no restriction of maximum 64 handles.
        /// </summary>
        /// <param name="waitHandles">The collection of <see cref="System.Threading.WaitHandle"/>s to wait on: <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="timeoutMs">The wait timeout in milliseconds. Set to -1 to wait indefinitely. If any handle times out this method returns False.</param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns True if all handles has pulsed, otherwise False</returns>
        public static Task<bool> WaitAllAsync(IEnumerable<WaitHandle> waitHandles, int timeoutMs, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (waitHandles == null)
                throw new ArgumentNullException(nameof(waitHandles));

            int totalCount = 0;
            foreach (var waitHandle in waitHandles) {
                if (waitHandle == null)
                    throw new ArgumentException($"The WaitHandle reference at index {totalCount} is NULL", nameof(waitHandles));
                ThrowIfMutex(waitHandle);
                totalCount++;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<bool>();
            var awaiters = new WaitHandleAwaiter[totalCount];
            int signaledCount = 0;
            int state = 0;

            var ctRegistration = default(CancellationTokenRegistration);
            if (cancellationToken.CanBeCanceled)
                ctRegistration = cancellationToken.Register(() => {
                    if (Interlocked.CompareExchange(ref state, 1, 0) == 0) {
                        tcs.SetCanceled();
                    }
                },
                useSynchronizationContext: false);

            int index = 0;
            foreach (var waitHandle in waitHandles) {

                if (state != 0)
                    break;

                var awaiter = awaiters[index] = waitHandle.ConfigureAwait(timeoutMs, cancellationToken).GetAwaiter();

                if (state != 0) {
                    CancelAwaiter(awaiter);
                    break;
                }

                awaiter.OnCompleted(() => {
                    if (cancellationToken.IsCancellationRequested) {
                        if (TryChangeState(ref state, expectedState: 0, newState: 1)) {
                            ctRegistration.Dispose();
                            tcs.SetCanceled();
                        }
                    } else if (awaiter.IsSignaled) {
                        if (Interlocked.Increment(ref signaledCount) == totalCount) {
                            if (TryChangeState(ref state, expectedState: 0, newState: 1)) {
                                ctRegistration.Dispose();
                                tcs.SetResult(true);
                            }
                        }
                    } else {
                        if (TryChangeState(ref state, expectedState: 0, newState: 1)) {
                            CancelAwaiters(awaiters);
                            ctRegistration.Dispose();
                            tcs.SetResult(false);
                        }
                    }
                });

                index++;
            }

            return tcs.Task;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowIfMutex(WaitHandle waitHandle)
        {
            if (waitHandle is Mutex)
                throw new InvalidOperationException("You cannot await on a Mutex, because the thread that acquires a mutex must release it, where current implementation uses ThreadPool - i.e. acquire/release cannot be guaranteed to be on one particular thread. If you wait to use a Mutex in async-await manner, use a Semaphore with maximum count of 1 instead. However, be aware of behavior differences of Mutex and Semaphore, because there are not fully interchangeable.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryChangeState(ref int state, int expectedState, int newState)
        {
            return Interlocked.CompareExchange(ref state, newState, expectedState) == expectedState;
        }

        private static void CancelAwaiters(WaitHandleAwaiter[] awaiters)
        {
            for (int i = 0; i < awaiters.Length; i++) {
                var awaiter = awaiters[i];
                if (awaiter != null)
                    CancelAwaiter(awaiter);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CancelAwaiter(WaitHandleAwaiter awaiter)
        {
            awaiter.OnCompleted(null);
            awaiter.OnCancelRequested();
        }
    }
}
