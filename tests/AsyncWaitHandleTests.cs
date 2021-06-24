// Copyright (c) Serge Semenov.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using AsyncWaitHandle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncWaitHandle.Tests
{
    [TestClass]
    public class AsyncWaitHandleTests
    {
        // Regression test for the case where a WaitOneAsync() on a manual-reset
        // event that is already set result in a NullReferenceException in version 1.0.1.
        [TestMethod]
        public async Task StressTestFastCompletion()
        {
            using var completionEvent = new ManualResetEvent(false);
            for (int i = 0; i < 100_000; i++)
            {
                completionEvent.Reset();
                Task waitTask = WaitEventAsync(completionEvent);
                Assert.IsFalse(waitTask.IsCompleted);
                completionEvent.Set();
                await waitTask;
            }
        }

        private static async Task WaitEventAsync(ManualResetEvent ev)
        {
            await Task.Yield();
            await ev.WaitOneAsync();
        }
        
        [TestMethod]
        public async Task AwaitWithCancellationTokenHonorsCancellation()
        {
            using var completionEvent = new ManualResetEvent(false);
            using var cts = new CancellationTokenSource();
            Task waitTask = completionEvent.WaitOneAsync(cts.Token);
            Assert.IsFalse(waitTask.IsCompleted);
            cts.Cancel();
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => waitTask);
        }
    }
}
