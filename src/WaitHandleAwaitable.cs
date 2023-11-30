// Copyright (c) Serge Semenov.
// Licensed under the MIT License.

using System.Threading;

namespace AsyncWaitHandle;

/// <summary>
/// A configuration of an awaiter for a <see cref="System.Threading.WaitHandle"/>
/// </summary>
public struct WaitHandleAwaitable
{
    private WaitHandle _waitHandle;
    private int _timeoutMs;
    private CancellationToken _cancellationToken;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="waitHandle">The <see cref="System.Threading.WaitHandle"/> to wait for</param>
    /// <param name="timeoutMs">The wait timeout in milliseconds, or -1 for indefinite wait</param>
    /// <param name="cancellationToken">The wait cancellation token</param>
    internal WaitHandleAwaitable(WaitHandle waitHandle, int timeoutMs, CancellationToken cancellationToken)
    {
        _waitHandle = waitHandle;
        _timeoutMs = timeoutMs;
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets an awaiter used to await the <see cref="System.Threading.WaitHandle"/>
    /// </summary>
    /// <returns>An awaiter instance</returns>
    public WaitHandleAwaiter GetAwaiter() => WaitHandleAwaiter.StartWaiting(_waitHandle, _timeoutMs, _cancellationToken);
}