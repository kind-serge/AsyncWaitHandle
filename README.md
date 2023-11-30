Use C#/VB await keyword with AutoResetEvent, ManualResetEvent, or Semaphore in .NET apps

Extension methods for the `WaitHandle` class in the System.Threading namespace for your convenience.
Available as NuGet package 'AsyncWaitHandle2' on nuget.org: https://www.nuget.org/packages/AsyncWaitHandle2/


Example 1 (simple await):

```c#
var e = new AutoResetEvent();
...
await e;
```

Example 2 (configured await):

```c#
var e = new AutoResetEvent();
var cts = new CancellationTokenSource();
...
var timeout = TimeSpan.FromSeconds(30);
try
{
  await e.ConfigureAwait(timeout, cts.Token);
}
catch (TimeOutException)
{
  ...
}
catch (OperationCanceledException)
{
  ...
}
```


Example 3 (WaitOneAsync):

```c#
var e = new AutoResetEvent();
var cts = new CancellationTokenSource();
...
var timeout = TimeSpan.FromSeconds(30);
bool fSignaled;
try
{
  fSignaled = await e.WaitOneAsync(timeout, cts.Token);
}
catch (OperationCanceledException)
{
  ...
}
```


Example 4 (WaitAnyAsync):

```c#
var events = new WaitHandle[] { new AutoResetEvent(), new ManualResetEvent() };
var cts = new CancellationTokenSource();
...
var timeout = TimeSpan.FromSeconds(30);
int signaledIndex;
try
{
  signaledIndex = await events.WaitAnyAsync(timeout, cts.Token);
}
catch (OperationCanceledException)
{
  ...
}
switch (signaledIndex)
{
  case 0: ...
  case 1: ...
  case WaitHandle.Wait...
}
```


Example 5 (WaitAllAsync):

```c#
var events = new WaitHandle[] { new AutoResetEvent(), new ManualResetEvent() };
var cts = new CancellationTokenSource();
...
var timeout = TimeSpan.FromSeconds(30);
bool allSignaled;
try
{
  allSignaled = await events.WaitAllAsync(timeout, cts.Token);
}
catch (OperationCanceledException)
{
  ...
}
```
