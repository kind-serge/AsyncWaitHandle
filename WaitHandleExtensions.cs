using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AsyncWaitHandle;

namespace System.Threading
{
    /// <summary>
    /// Async extension methods for any <see cref="System.Threading.WaitHandle"/>: <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, <see cref="System.Threading.Semaphore"/>
    /// </summary>
    public static class WaitHandleExtensions
    {
        /// <summary>
        /// Gets an awaiter for the <see cref="System.Threading.WaitHandle"/>, so you can write code like: "await waitHandle;"
        /// </summary>
        /// <param name="waitHandle">An instance of <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <returns>Returns an awaiter for given instance of the <see cref="System.Threading.WaitHandle"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitHandleAwaiter GetAwaiter(this WaitHandle waitHandle) => waitHandle.ConfigureAwait(timeoutMs: -1).GetAwaiter();

        /// <summary>
        /// Configures awaiter for the <see cref="System.Threading.WaitHandle"/>, so you can customize awaiting: "await waitHandle.ConfigureAwait(...);"
        /// </summary>
        /// <param name="waitHandle">An instance of <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the awaiter will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns an object that produces a configured awaiter for the <see cref="System.Threading.WaitHandle"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitHandleAwaitable ConfigureAwait(this WaitHandle waitHandle, CancellationToken cancellationToken) => new WaitHandleAwaitable(waitHandle, timeoutMs: -1, cancellationToken: cancellationToken);

        /// <summary>
        /// Configures awaiter for the <see cref="System.Threading.WaitHandle"/>, so you can customize awaiting: "await waitHandle.ConfigureAwait(...);"
        /// </summary>
        /// <param name="waitHandle">An instance of <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="timeoutMs">The wait timeout in milliseconds. Set to -1 to wait indefinitely. On timeout the awaiter throws <see cref="System.TimeoutException"/>.</param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the awaiter will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns an object that produces a configured awaiter for the <see cref="System.Threading.WaitHandle"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitHandleAwaitable ConfigureAwait(this WaitHandle waitHandle, int timeoutMs, CancellationToken cancellationToken = default(CancellationToken)) => new WaitHandleAwaitable(waitHandle, timeoutMs, cancellationToken);

        /// <summary>
        /// Configures awaiter for the <see cref="System.Threading.WaitHandle"/>, so you can customize awaiting: "await waitHandle.ConfigureAwait(...);"
        /// </summary>
        /// <param name="waitHandle">An instance of <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="timeout">A non-negative timeout. On timeout the awaiter throws <see cref="System.TimeoutException"/>.</param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the awaiter will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns an object that produces a configured awaiter for the <see cref="System.Threading.WaitHandle"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitHandleAwaitable ConfigureAwait(this WaitHandle waitHandle, TimeSpan timeout, CancellationToken cancellationToken = default(CancellationToken)) => new WaitHandleAwaitable(waitHandle, (int)timeout.TotalMilliseconds, cancellationToken);

        /// <summary>
        /// Creates a task for waiting the <see cref="System.Threading.WaitHandle"/>, so you can write: "if (await waitHandle.WaitOneAsync()) ... else ..." or "await waitHandle.WaitOneAsync().ContinueWith(...)"
        /// </summary>
        /// <param name="waitHandle">An instance of <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns a task which result tells if the <see cref="System.Threading.WaitHandle"/> signaled (True) or didn't (False), or throws the <see cref="System.OperationCanceledException"/></returns>
        /// <exception cref="System.OperationCanceledException">Throws <see cref="System.OperationCanceledException"/> if the cancellation was requested using given <paramref name="cancellationToken"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitOneAsync(this WaitHandle waitHandle, CancellationToken cancellationToken = default(CancellationToken)) => waitHandle.WaitOneAsync(timeoutMs: -1, cancellationToken: cancellationToken);

        /// <summary>
        /// Creates a task for waiting the <see cref="System.Threading.WaitHandle"/>, so you can write: "if (await waitHandle.WaitOneAsync()) ... else ..." or "await waitHandle.WaitOneAsync().ContinueWith(...)"
        /// </summary>
        /// <param name="waitHandle">An instance of <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="timeout">The wait timeout. On timeout the awaiter throws <see cref="System.TimeoutException"/>.</param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns a task which result tells if the <see cref="System.Threading.WaitHandle"/> signaled (True) or didn't (False), or throws the <see cref="System.OperationCanceledException"/></returns>
        /// <exception cref="System.OperationCanceledException">Throws <see cref="System.OperationCanceledException"/> if the cancellation was requested using given <paramref name="cancellationToken"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitOneAsync(this WaitHandle waitHandle, TimeSpan timeout, CancellationToken cancellationToken = default(CancellationToken)) => waitHandle.WaitOneAsync((int)timeout.TotalMilliseconds, cancellationToken);

        /// <summary>
        /// Creates a task for waiting the <see cref="System.Threading.WaitHandle"/>, so you can write: "if (await waitHandle.WaitOneAsync()) ... else ..." or "await waitHandle.WaitOneAsync().ContinueWith(...)"
        /// </summary>
        /// <param name="waitHandle">An instance of <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="timeoutMs">The wait timeout in milliseconds. Set to -1 to wait indefinitely. On timeout the awaiter throws <see cref="System.TimeoutException"/>.</param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns a task which result tells if the <see cref="System.Threading.WaitHandle"/> signaled (True) or didn't (False), or throws the <see cref="System.OperationCanceledException"/></returns>
        /// <exception cref="System.OperationCanceledException">Throws <see cref="System.OperationCanceledException"/> if the cancellation was requested using given <paramref name="cancellationToken"/></exception>
        public static Task<bool> WaitOneAsync(this WaitHandle waitHandle, int timeoutMs, CancellationToken cancellationToken = default(CancellationToken)) => WaitHandleAsyncOperations.WaitOneAsync(waitHandle, timeoutMs, cancellationToken);

        /// <summary>
        /// Waits for any of given <see cref="System.Threading.WaitHandle"/> to pulse. Unlike <see cref="System.Threading.WaitHandle.WaitAny"/>, this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one, and there is no restriction of maximum 64 handles.
        /// </summary>
        /// <param name="waitHandles">The collection of <see cref="System.Threading.WaitHandle"/>s to wait on: <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns the index of a <see cref="System.Threading.WaitHandle"/> in the given collection which pulsed first. Note that this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one.</returns>
        /// <exception cref="System.OperationCanceledException">Throws <see cref="System.OperationCanceledException"/> if the cancellation was requested using given <paramref name="cancellationToken"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<int> WaitAnyAsync(this IEnumerable<WaitHandle> waitHandles, CancellationToken cancellationToken = default(CancellationToken)) => waitHandles.WaitAnyAsync(timeoutMs: -1, cancellationToken: cancellationToken);

        /// <summary>
        /// Waits for any of given <see cref="System.Threading.WaitHandle"/> to pulse. Unlike <see cref="System.Threading.WaitHandle.WaitAny"/>, this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one, and there is no restriction of maximum 64 handles.
        /// </summary>
        /// <param name="waitHandles">The collection of <see cref="System.Threading.WaitHandle"/>s to wait on: <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="timeout">The wait timeout. If all time out this method returns <see cref="System.Threading.WaitHandle.WaitTimeout"/>.</param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns the index of a <see cref="System.Threading.WaitHandle"/> in the given collection which pulsed first, or <see cref="System.Threading.WaitHandle.WaitTimeout"/> if all of them timed out. Note that this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one.</returns>
        /// <exception cref="System.OperationCanceledException">Throws <see cref="System.OperationCanceledException"/> if the cancellation was requested using given <paramref name="cancellationToken"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<int> WaitAnyAsync(this IEnumerable<WaitHandle> waitHandles, TimeSpan timeout, CancellationToken cancellationToken = default(CancellationToken)) => waitHandles.WaitAnyAsync((int)timeout.TotalMilliseconds, cancellationToken);

        /// <summary>
        /// Waits for any of given <see cref="System.Threading.WaitHandle"/> to pulse. Unlike <see cref="System.Threading.WaitHandle.WaitAny"/>, this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one, and there is no restriction of maximum 64 handles.
        /// </summary>
        /// <param name="waitHandles">The collection of <see cref="System.Threading.WaitHandle"/>s to wait on: <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="timeoutMs">The wait timeout in milliseconds. Set to -1 to wait indefinitely. If all time out this method returns <see cref="System.Threading.WaitHandle.WaitTimeout"/>.</param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns the index of a <see cref="System.Threading.WaitHandle"/> in the given collection which pulsed first, or <see cref="System.Threading.WaitHandle.WaitTimeout"/> if all of them timed out. Note that this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one.</returns>
        /// <exception cref="System.OperationCanceledException">Throws <see cref="System.OperationCanceledException"/> if the cancellation was requested using given <paramref name="cancellationToken"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<int> WaitAnyAsync(this IEnumerable<WaitHandle> waitHandles, int timeoutMs, CancellationToken cancellationToken = default(CancellationToken)) => WaitHandleAsyncOperations.WaitAnyAsync(waitHandles, timeoutMs, cancellationToken);

        /// <summary>
        /// Waits for all of given <see cref="System.Threading.WaitHandle"/> to pulse. Unlike <see cref="System.Threading.WaitHandle.WaitAll"/>, this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one, and there is no restriction of maximum 64 handles.
        /// </summary>
        /// <param name="waitHandles">The collection of <see cref="System.Threading.WaitHandle"/>s to wait on: <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns a Task which completes only when all handles has pulsed</returns>
        /// <exception cref="System.OperationCanceledException">Throws <see cref="System.OperationCanceledException"/> if the cancellation was requested using given <paramref name="cancellationToken"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WaitAllAsync(this IEnumerable<WaitHandle> waitHandles, CancellationToken cancellationToken = default(CancellationToken)) => waitHandles.WaitAllAsync(timeoutMs: -1, cancellationToken: cancellationToken);

        /// <summary>
        /// Waits for all of given <see cref="System.Threading.WaitHandle"/> to pulse. Unlike <see cref="System.Threading.WaitHandle.WaitAll"/>, this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one, and there is no restriction of maximum 64 handles.
        /// </summary>
        /// <param name="waitHandles">The collection of <see cref="System.Threading.WaitHandle"/>s to wait on: <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="timeout">The wait timeout. If any handle times out this method returns False.</param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns True if all handles has pulsed, otherwise False</returns>
        /// <exception cref="System.OperationCanceledException">Throws <see cref="System.OperationCanceledException"/> if the cancellation was requested using given <paramref name="cancellationToken"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitAllAsync(this IEnumerable<WaitHandle> waitHandles, TimeSpan timeout, CancellationToken cancellationToken = default(CancellationToken)) => waitHandles.WaitAllAsync((int)timeout.TotalMilliseconds, cancellationToken);

        /// <summary>
        /// Waits for all of given <see cref="System.Threading.WaitHandle"/> to pulse. Unlike <see cref="System.Threading.WaitHandle.WaitAll"/>, this method does not wait on all of them as an atomic operation, but registers awaiters for given wait handles one by one, and there is no restriction of maximum 64 handles.
        /// </summary>
        /// <param name="waitHandles">The collection of <see cref="System.Threading.WaitHandle"/>s to wait on: <see cref="System.Threading.AutoResetEvent"/>, <see cref="System.Threading.ManualResetEvent"/>, or <see cref="System.Threading.Semaphore"/></param>
        /// <param name="timeoutMs">The wait timeout in milliseconds. Set to -1 to wait indefinitely. If any handle times out this method returns False.</param>
        /// <param name="cancellationToken">The cancellation token to stop waiting. If cancellation is requested, the Task will throw <see cref="System.OperationCanceledException"/>.</param>
        /// <returns>Returns True if all handles has pulsed, otherwise False</returns>
        /// <exception cref="System.OperationCanceledException">Throws <see cref="System.OperationCanceledException"/> if the cancellation was requested using given <paramref name="cancellationToken"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitAllAsync(this IEnumerable<WaitHandle> waitHandles, int timeoutMs, CancellationToken cancellationToken = default(CancellationToken)) => WaitHandleAsyncOperations.WaitAllAsync(waitHandles, timeoutMs, cancellationToken);
    }
}