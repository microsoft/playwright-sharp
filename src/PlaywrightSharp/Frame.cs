using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PlaywrightSharp
{
    /// <inheritdoc cref="IFrame"/>
    public class Frame : IFrame
    {
        private readonly IDictionary<ContextType, ContextData> _contextData;
        private readonly bool _detached = false;
        private int _setContentCounter = 0;
        private Frame _parentFrame;

        internal Frame(Page page, string frameId, Frame parentFrame)
        {
            Page = page;
            Id = frameId;
            _parentFrame = parentFrame;

            _contextData = new Dictionary<ContextType, ContextData>
            {
                [ContextType.Main] = new ContextData(),
                [ContextType.Utility] = new ContextData(),
            };
            SetContext(ContextType.Main, null);
            SetContext(ContextType.Utility, null);

            _parentFrame?.ChildFrames.Add(this);
        }

        /// <inheritdoc cref="IFrame.ChildFrames"/>
        IFrame[] IFrame.ChildFrames => ChildFrames.ToArray();

        /// <inheritdoc cref="IFrame.Name"/>
        public string Name { get; set; }

        /// <inheritdoc cref="IFrame.Url"/>
        public string Url { get; set; }

        /// <inheritdoc cref="IFrame.ParentFrame"/>
        public IFrame ParentFrame => _parentFrame;

        /// <inheritdoc cref="IFrame.Detached"/>
        public bool Detached { get; set; }

        /// <inheritdoc cref="IFrame.Id"/>
        public string Id { get; set; }

        internal Page Page { get; }

        internal IList<string> FiredLifecycleEvents { get; } = new List<string>();

        internal List<Frame> ChildFrames { get; } = new List<Frame>();

        internal string LastDocumentId { get; set; }

        internal List<Request> InflightRequests { get; set; } = new List<Request>();

        internal ConcurrentDictionary<string, CancellationTokenSource> NetworkIdleTimers { get; } = new ConcurrentDictionary<string, CancellationTokenSource>();

        /// <inheritdoc cref="IFrame.GetTitleAsync" />
        public async Task<string> GetTitleAsync()
        {
            var context = await GetUtilityContextAsync().ConfigureAwait(false);
            return await context.EvaluateAsync<string>("() => document.title").ConfigureAwait(false);
        }

        /// <inheritdoc cref="IFrame.AddScriptTagAsync(AddTagOptions)"/>
        public async Task<IElementHandle> AddScriptTagAsync(AddTagOptions options)
        {
            const string addScriptUrl = @"async function addScriptUrl(url, type) {
                const script = document.createElement('script');
                script.src = url;
                if (type)
                    script.type = type;
                const promise = new Promise((res, rej) => {
                    script.onload = res;
                    script.onerror = rej;
                });
                document.head.appendChild(script);
                await promise;
                return script;
            }";

            const string addScriptContent = @"function addScriptContent(content, type = 'text/javascript') {
                const script = document.createElement('script');
                script.type = type;
                script.text = content;
                let error = null;
                script.onerror = e => error = e;
                document.head.appendChild(script);
                if (error)
                    throw error;
                return script;
            }";

            if (options == null || (string.IsNullOrEmpty(options.Url) && string.IsNullOrEmpty(options.Path) && string.IsNullOrEmpty(options.Content)))
            {
                throw new PlaywrightSharpException("Provide an object with a `url`, `path` or `content` property");
            }

            var context = await GetMainContextAsync().ConfigureAwait(false);
            return await RaceWithCSPErrorAsync(async () =>
            {
                if (!string.IsNullOrEmpty(options.Url))
                {
                    return await context.EvaluateHandleAsync(addScriptUrl, options.Url, options.Type)
                        .ConfigureAwait(false) as ElementHandle;
                }

                if (!string.IsNullOrEmpty(options.Path))
                {
                    string content = File.ReadAllText(options.Path);
                    content += "//# sourceURL=" + options.Path.Replace("\n", string.Empty);
                    return await context.EvaluateHandleAsync(addScriptContent, content, options.Type)
                        .ConfigureAwait(false) as ElementHandle;
                }

                return await context.EvaluateHandleAsync(
                    addScriptContent,
                    options.Content,
                    string.IsNullOrEmpty(options.Type) ? "text/javascript" : options.Type).ConfigureAwait(false) as ElementHandle;
            }).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IFrame.ClickAsync(string, ClickOptions)"/>
        public async Task ClickAsync(string selector, ClickOptions options = null)
        {
            var handle = await OptionallyWaitForSelectorInUtilityContextAsync(selector, options).ConfigureAwait(false);
            await handle.ClickAsync(options).ConfigureAwait(false);
            await handle.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc cref="IFrame.EvaluateAsync{T}(string, object[])"/>
        public async Task<T> EvaluateAsync<T>(string script, params object[] args)
        {
            var context = await GetMainContextAsync().ConfigureAwait(false);
            return await context.EvaluateAsync<T>(script, args).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IFrame.EvaluateAsync(string, object[])"/>
        public Task<JsonElement?> EvaluateAsync(string script, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc cref="IFrame.EvaluateAsync{T}(string, object[])"/>
        public async Task<IJSHandle> EvaluateHandleAsync(string script, params object[] args)
        {
            var context = await GetMainContextAsync().ConfigureAwait(false);
            return await context.EvaluateHandleAsync(script, args).ConfigureAwait(false);
        }

        /// <inheritdoc cref="IFrame.FillAsync(string, string, WaitForSelectorOptions)"/>
        public Task FillAsync(string selector, string text, WaitForSelectorOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc cref="IFrame.GoToAsync(string, GoToOptions)"/>
        public async Task<IResponse> GoToAsync(string url, GoToOptions options = null)
        {
            Page.PageState.ExtraHTTPHeaders.TryGetValue("referer", out string referer);

            if (options?.Referer != null)
            {
                if (referer != null && referer != options.Referer)
                {
                    throw new ArgumentException("\"referer\" is already specified as extra HTTP header");
                }

                referer = options.Referer;
            }

            using var watcher = new LifecycleWatcher(this, options);

            try
            {
                var navigateTask = Page.Delegate.NavigateFrameAsync(this, url, referer);
                var task = await Task.WhenAny(
                    watcher.TimeoutOrTerminationTask,
                    navigateTask).ConfigureAwait(false);

                await task.ConfigureAwait(false);

                var tasks = new List<Task> { watcher.TimeoutOrTerminationTask };
                if (!string.IsNullOrEmpty(navigateTask.Result.NewDocumentId))
                {
                    watcher.SetExpectedDocumentId(navigateTask.Result.NewDocumentId, url);
                    tasks.Add(watcher.NewDocumentNavigationTask);
                }
                else if (navigateTask.Result.IsSameDocument)
                {
                    tasks.Add(watcher.SameDocumentNavigationTask);
                }
                else
                {
                    tasks.AddRange(new[] { watcher.SameDocumentNavigationTask, watcher.NewDocumentNavigationTask });
                }

                task = await Task.WhenAny(tasks).ConfigureAwait(false);

                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new NavigationException(ex.Message, ex);
            }

            return watcher.NavigationResponse;
        }

        /// <inheritdoc cref="IFrame.GoToAsync(string, WaitUntilNavigation)"/>
        public Task<IResponse> GoToAsync(string url, WaitUntilNavigation waitUntil)
            => GoToAsync(url, new GoToOptions { WaitUntil = new[] { waitUntil } });

        /// <inheritdoc cref="IJSHandle.GetPropertyAsync"/>
        public async Task<IElementHandle> QuerySelectorAsync(string selector)
        {
            var utilityContext = await GetUtilityContextAsync().ConfigureAwait(false);
            var mainContext = await GetMainContextAsync().ConfigureAwait(false);
            var handle = await utilityContext.QuerySelectorAsync(selector).ConfigureAwait(false) as ElementHandle;
            if (handle != null && handle.Context != mainContext)
            {
                var adopted = await Page.Delegate.AdoptElementHandleAsync(handle, mainContext).ConfigureAwait(false);
                await handle.DisposeAsync().ConfigureAwait(false);
                return adopted;
            }

            return handle;
        }

        /// <inheritdoc cref="IFrame.QuerySelectorEvaluateAsync(string, string, object[])"/>
        public Task QuerySelectorEvaluateAsync(string selector, string script, params object[] args) => QuerySelectorEvaluateAsync<object>(selector, script, args);

        /// <inheritdoc cref="IFrame.QuerySelectorEvaluateAsync{T}(string, string, object[])"/>
        public async Task<T> QuerySelectorEvaluateAsync<T>(string selector, string script, params object[] args)
        {
            var context = await GetMainContextAsync().ConfigureAwait(false);
            var elementHandle = await context.QuerySelectorAsync(selector).ConfigureAwait(false);
            if (elementHandle == null)
            {
                throw new SelectorException("failed to find element matching selector", selector);
            }

            var result = await elementHandle.EvaluateAsync<T>(script, args).ConfigureAwait(false);
            await elementHandle.DisposeAsync().ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc cref="IFrame.SetContentAsync(string, NavigationOptions)"/>
        public async Task SetContentAsync(string html, NavigationOptions options = null)
        {
            string tag = $"--playwright--set--content--{Id}--${++_setContentCounter}--";
            var context = await GetUtilityContextAsync().ConfigureAwait(false);
            LifecycleWatcher watcher = null;

            Page.FrameManager.ConsoleMessageTags.TryAdd(tag, () =>
            {
                // Clear lifecycle right after document.open() - see 'tag' below.
                Page.FrameManager.ClearFrameLifecycle(this);
                watcher = new LifecycleWatcher(this, options);
            });

            await context.EvaluateAsync(
                @"(html, tag) => {
                    window.stop();
                    document.open();
                    console.debug(tag);  // eslint-disable-line no-console
                    document.write(html);
                    document.close();
                }",
                html,
                tag).ConfigureAwait(false);

            if (watcher == null)
            {
                throw new PlaywrightSharpException("Was not able to clear lifecycle in SetContentAsync");
            }

            var timeoutTask = watcher.TimeoutOrTerminationTask;
            await Task.WhenAny(
                timeoutTask,
                watcher.LifecycleTask).ConfigureAwait(false);

            watcher.Dispose();
            if (timeoutTask.IsFaulted)
            {
                await timeoutTask.ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="IFrame.GetContentAsync"/>
        public async Task<string> GetContentAsync()
        {
            var context = await GetUtilityContextAsync().ConfigureAwait(false);
            return await context.EvaluateAsync<string>(@"() => {
                let retVal = '';
                if (document.doctype)
                    retVal = new XMLSerializer().serializeToString(document.doctype);
                if (document.documentElement)
                    retVal += document.documentElement.outerHTML;
                return retVal;
            }").ConfigureAwait(false);
        }

        /// <inheritdoc cref="IFrame.WaitForNavigationAsync(WaitForNavigationOptions)"/>
        public Task<IResponse> WaitForNavigationAsync(WaitForNavigationOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc cref="IFrame.WaitForNavigationAsync(WaitUntilNavigation)"/>
        public Task<IResponse> WaitForNavigationAsync(WaitUntilNavigation waitUntil)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc cref="IFrame.WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        public Task<IElementHandle> WaitForSelectorAsync(string selector, WaitForSelectorOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        internal Task<FrameExecutionContext> GetUtilityContextAsync() => GetContextAsync(ContextType.Utility);

        internal void OnDetached()
        {
            Detached = true;
            foreach (var data in _contextData.Values)
            {
                foreach (var rerunnableTask in data.RerunnableTasks)
                {
                    rerunnableTask.Terminate(new PlaywrightSharpException("waitForFunction failed: frame got detached."));
                }
            }

            _parentFrame?.ChildFrames.Remove(this);
            _parentFrame = null;
        }

        internal void ContextCreated(ContextType contextType, FrameExecutionContext context)
        {
            var data = _contextData[contextType];

            // In case of multiple sessions to the same target, there's a race between
            // connections so we might end up creating multiple isolated worlds.
            // We can use either.
            if (data.Context != null)
            {
                SetContext(contextType, null);
            }

            SetContext(contextType, context);
        }

        internal void ContextDestroyed(FrameExecutionContext context)
        {
            foreach (var contextType in _contextData.Keys)
            {
                var data = _contextData[contextType];
                if (data.Context == context)
                {
                    SetContext(contextType, null);
                }
            }
        }

        private async Task<IElementHandle> RaceWithCSPErrorAsync(Func<Task<ElementHandle>> func)
        {
            var errorTcs = new TaskCompletionSource<string>();
            var actionTask = func();

            void ConsoleEventHandler(object sender, ConsoleEventArgs e)
            {
                if (e.Message.Type == ConsoleType.Error && e.Message.Text.Contains("Content Security Policy"))
                {
                    errorTcs.TrySetResult(e.Message.Text);
                }
            }

            Page.Console += ConsoleEventHandler;

            await Task.WhenAny(actionTask, errorTcs.Task).ConfigureAwait(false);

            if (errorTcs.Task.IsCompleted)
            {
                throw new PlaywrightSharpException(errorTcs.Task.Result);
            }

            return await actionTask.ConfigureAwait(false);
        }

        private void SetContext(ContextType contextType, FrameExecutionContext context)
        {
            var data = _contextData[contextType];
            data.Context = context;

            if (context != null)
            {
                data.ContextTsc.TrySetResult(context);

                foreach (var rerunnableTask in data.RerunnableTasks)
                {
                    _ = rerunnableTask.RerunAsync(context);
                }
            }
            else
            {
                data.ContextTsc = new TaskCompletionSource<FrameExecutionContext>();
            }
        }

        private Task<FrameExecutionContext> GetMainContextAsync() => GetContextAsync(ContextType.Main);

        private Task<FrameExecutionContext> GetContextAsync(ContextType contextType)
        {
            if (_detached)
            {
                throw new PlaywrightSharpException(
                    $"Execution Context is not available in detached frame \"{Url}\" (are you trying to evaluate ?)");
            }

            return _contextData[contextType].ContextTask;
        }

        private async Task<IElementHandle> OptionallyWaitForSelectorInUtilityContextAsync(string selector, ClickOptions options)
        {
            options ??= new ClickOptions();
            options.Timeout ??= Page.DefaultTimeout;

            IElementHandle handle;

            if (options.WaitFor != WaitForOption.NoWait)
            {
                var maybeHandle = await WaitForSelectorInUtilityContextAsync(selector, options.WaitFor, options.Timeout)
                    .ConfigureAwait(false);

                handle = maybeHandle ?? throw new SelectorException($"No node found for selector", SelectorToString(selector, options.WaitFor));
            }
            else
            {
                var context = await GetContextAsync(ContextType.Utility).ConfigureAwait(false);
                var maybeHandle = await context.QuerySelectorAsync(selector).ConfigureAwait(false);

                if (maybeHandle == null)
                {
                    throw new SelectorException($"No node found for selector", selector);
                }

                handle = maybeHandle!;
            }

            return handle;
        }

        private string SelectorToString(string selector, WaitForOption waitFor)
        {
            string label = waitFor switch
            {
                WaitForOption.Visible => "[visible] ",
                WaitForOption.Hidden => "[hidden] ",
                _ => string.Empty,
            };
            return $"{label}{selector}";
        }

        private async Task<ElementHandle> WaitForSelectorInUtilityContextAsync(
            string selector,
            WaitForOption waitFor,
            int? timeout)
        {
            var task = Dom.GetWaitForSelectorFunction(selector, waitFor, timeout);
            var result = await ScheduleRerunnableTaskAsync(
                task,
                ContextType.Utility,
                timeout,
                $"selector \"{SelectorToString(selector, waitFor)}\"").ConfigureAwait(false);

            if (!(result is ElementHandle))
            {
                await result.DisposeAsync().ConfigureAwait(false);
                return null;
            }

            return result as ElementHandle;
        }

        private Task<IJSHandle> ScheduleRerunnableTaskAsync(
            Func<IFrameExecutionContext,
            Task<IJSHandle>> task,
            ContextType contextType,
            int? timeout,
            string title)
        {
            var data = _contextData[contextType];
            var rerunnableTask = new RerunnableTask(data, task, timeout, title);
            data.RerunnableTasks.Add(rerunnableTask);
            if (data.Context != null)
            {
                _ = rerunnableTask.RerunAsync(data.Context);
            }

            return rerunnableTask.Task;
        }
    }
}
