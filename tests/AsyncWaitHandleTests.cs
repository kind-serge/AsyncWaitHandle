// Copyright (c) Serge Semenov.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
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
        // event that is quickly set results in a NullReferenceException in version 1.0.1.
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
        public async Task WaitOneWithCancellationTokenHonorsCancellation()
        {
            using var completionEvent = new ManualResetEvent(false);
            using var cts = new CancellationTokenSource();
            Task waitTask = completionEvent.WaitOneAsync(cts.Token);
            Assert.IsFalse(waitTask.IsCompleted);
            cts.Cancel();
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => waitTask);
        }

        [TestMethod]
        public async Task WaitOneWithTimeoutTimesOut()
        {
            using var completionEvent = new ManualResetEvent(false);
            Stopwatch sw = Stopwatch.StartNew();
            await completionEvent.WaitOneAsync(TimeSpan.FromMilliseconds(200));
            Assert.IsTrue(sw.ElapsedMilliseconds > 100);
        }

        [TestMethod]
        public async Task AwaitAny()
        {
            using var completionEvent1 = new ManualResetEvent(false);
            using var completionEvent2 = new ManualResetEvent(false);
            var events = new[] {completionEvent1, completionEvent2};
            Task waitTask = events.WaitAnyAsync();
            Assert.IsFalse(waitTask.IsCompleted);
            completionEvent1.Set();
            await waitTask;
        }

        [TestMethod]
        public async Task AwaitAll()
        {
            using var completionEvent1 = new ManualResetEvent(false);
            using var completionEvent2 = new ManualResetEvent(false);
            var events = new[] { completionEvent1, completionEvent2 };
            Task waitTask = events.WaitAllAsync();
            Assert.IsFalse(waitTask.IsCompleted);

            completionEvent1.Set();
            Assert.IsFalse(waitTask.IsCompleted);

            completionEvent2.Set();
            await waitTask;
        }
    }
}
