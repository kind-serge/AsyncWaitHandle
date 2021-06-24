// Copyright (c) Serge Semenov.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncWaitHandle
{
    /// <summary>
    /// Awaiter for a <see cref="System.Threading.WaitHandle"/>
    /// </summary>
    public sealed class WaitHandleAwaiter : INotifyCompletion
    {
        private const int State_Waiting = 0;
        private const int State_Completing = 1;
        private const int State_Signaled = 2;
        private const int State_TimedOut = 3;
        private const int State_Canceled = 4;

        private WaitHandle _waitHandle;
        private RegisteredWaitHandle _waitRegistration;
        private int _state = State_Waiting;
        private int _timeoutMs;
        private Action _continuation;
        private CancellationTokenRegistration _ctRegistration;

        /// <summary>
        /// Starts waiting on the <see cref="System.Threading.WaitHandle"/> to pulse on a separate thread
        /// </summary>
        /// <param name="waitHandle">The <see cref="System.Threading.WaitHandle"/> to wait on</param>
        /// <param name="timeoutMs">The wait timeout, or -1 to wait indefinitely</param>
        /// <param name="cancellationToken">The wait cancellation token</param>
        /// <returns>An instance of the awaiter</returns>
        internal static WaitHandleAwaiter StartWaiting(WaitHandle waitHandle, int timeoutMs, CancellationToken cancellationToken)
        {
            WaitHandleAsyncOperations.ThrowIfMutex(waitHandle);

            cancellationToken.ThrowIfCancellationRequested();

            var awaiter = new WaitHandleAwaiter() { _waitHandle = waitHandle, _timeoutMs = timeoutMs };
            awaiter._waitRegistration = ThreadPool.RegisterWaitForSingleObject(waitHandle, WaitCallback, awaiter, timeoutMs, executeOnlyOnce: true);

            if (cancellationToken.CanBeCanceled)
                awaiter._ctRegistration = cancellationToken.Register(awaiter.OnCancelRequested, useSynchronizationContext: false);

            return awaiter;
        }

        /// <summary>
        /// Fired when either the WaitHandle pulses or waiting times out
        /// </summary>
        private static void WaitCallback(object state, bool timedOut) => ((WaitHandleAwaiter)state).OnWaitHandleComplete(timedOut);

        /// <summary>
        /// Fired when either the WaitHandle pulses or waiting times out
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnWaitHandleComplete(bool timedOut)
        {
            DoCompletion(timedOut ? State_TimedOut : State_Signaled);
        }

        /// <summary>
        /// Fired when a cancellation is requested via the CancellationToken or externally
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnCancelRequested()
        {
            DoCompletion(State_Canceled);
        }

        private void DoCompletion(int targetState)
        {
            if (WaitHandleAsyncOperations.TryChangeState(ref _state, expectedState: State_Waiting, newState: State_Completing)) {
                _waitRegistration.Unregister(null);
                _ctRegistration.Dispose();
                _state = targetState;
                InvokeContinuation();
            }
        }

        private void InvokeContinuation()
        {
            // TO DO: optionally invoke on the SynchronizationContext
            _continuation?.Invoke();
        }

        /// <summary>
        /// Tells if the waiting has timed out
        /// </summary>
        public bool IsTimedOut => _state == State_TimedOut;

        /// <summary>
        /// Tells if the waiting has been canceled
        /// </summary>
        public bool IsCanceled => _state == State_Canceled;

        /// <summary>
        /// Tells if the <see cref="System.Threading.WaitHandle"/> has pulsed
        /// </summary>
        public bool IsSignaled => _state == State_Signaled;

        /// <summary>
        /// Tells if the waiting is completed: the <see cref="System.Threading.WaitHandle"/> has pulsed, or the waiting has timed out, or the waiting has been canceled
        /// </summary>
        public bool IsCompleted => _state >= State_Signaled;

        /// <summary>
        /// Sets the action to invoke when the waiting is complete
        /// </summary>
        /// <param name="continuation">The action to invoke on completion</param>
        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;
            if (IsCompleted)
                InvokeContinuation();
        }

        /// <summary>
        /// Gets the result of waiting operation. Returns nothing in <see cref="System.TimeoutException"/> has signaled, or throws an exception in other cases.
        /// </summary>
        /// <exception cref="System.TimeoutException">Throws <see cref="System.TimeoutException"/> if the <see cref="System.Threading.WaitHandle"/> didn't pulse within given time frame</exception>
        /// <exception cref="System.OperationCanceledException">Throws <see cref="System.OperationCanceledException"/> if the cancellation was requested using given cancellation token</exception>
        public void GetResult()
        {
            if (!IsCompleted) {
                bool isTimedOut = !_waitHandle.WaitOne(_timeoutMs);
                OnWaitHandleComplete(isTimedOut);
            }

            if (IsTimedOut)
                throw new TimeoutException($"The wait handle {_waitHandle.SafeWaitHandle.DangerousGetHandle()} has timeout out after {_timeoutMs} ms");

            if (IsCanceled)
                throw new OperationCanceledException($"The waiting on handle {_waitHandle.SafeWaitHandle.DangerousGetHandle()} has been canceled");
        }
    }
}
