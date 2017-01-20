# Diagnostic Analyzers

The following are the diagnostic analyzers installed with the [Microsoft.VisualStudio.Threading.Analyzers][1]
NuGet package.

ID | Title
---- | ---
[VSTHRD001](VSTHRD001.md) | Avoid problematic synchronous waits
[VSTHRD002](VSTHRD002.md) | Use VS services from UI thread
[VSTHRD003](VSTHRD003.md) | Avoid `async void` methods
[VSTHRD004](VSTHRD004.md) | Avoid unsupported async delegates
[VSTHRD005](VSTHRD005.md) | Use `InvokeAsync` to raise async events
[VSTHRD006](VSTHRD006.md) | Avoid awaiting non-joinable tasks in join contexts
[VSTHRD007](VSTHRD007.md) | Avoid using `Lazy<T>` where `T` is `Task<T2>`
[VSTHRD008](VSTHRD008.md) | Call async methods when in an async method
[VSTHRD009](VSTHRD009.md) | Implement internal logic asynchronously
[VSTHRD010](VSTHRD010.md) | Use `Async` suffix for async methods
[VSTHRD011](VSTHRD011.md) | Avoid method overloads that assume TaskScheduler.Current
[VSTHRD012](VSTHRD012.md) | Provide JoinableTaskFactory where allowed
[VSTHRD013](VSTHRD013.md) | Offer async option
[VSTHRD014](VSTHRD014.md) | Avoid legacy thread switching methods

[1]: https://nuget.org/packages/microsoft.visualstudio.threading.analyzers
