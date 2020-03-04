using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlaywrightSharp
{
    internal partial class Frame : IFrame
    {
        private readonly Page _page;
        private readonly string _frameId;
        private readonly Frame _parentFrame;
        private readonly IDictionary<ContextType, ContextData> _contextData;
        private bool _detached = false;

        public Frame(Page page, string frameId, Frame parentFrame)
        {
            _page = page;
            _frameId = frameId;
            _parentFrame = parentFrame;
            _contextData = new Dictionary<ContextType, ContextData>
            {
                [ContextType.Main] = new ContextData(),
                [ContextType.Utility] = new ContextData(),
            };
            SetContext(ContextType.Main, null);
            SetContext(ContextType.Utility, null);

            if (_parentFrame != null)
            {
                _parentFrame.ChildFrames.Add(this);
            }
        }

        IFrame[] IFrame.ChildFrames => ChildFrames.ToArray();

        public List<Frame> ChildFrames { get; } = new List<Frame>();

        public string Name { get; set; }

        public string Url { get; set; }

        public IFrame ParentFrame => null;

        public bool Detached { get; set; }

        public string Id { get; set; }

        public string LastDocumentId { get; set; }

        public Page Page => _page;

        public IList<string> FiredLifecycleEvents { get; } = new List<string>();

        public Task<IElementHandle> AddScriptTagAsync(AddTagOptions options)
        {
            throw new System.NotImplementedException();
        }

        public Task ClickAsync(string selector, ClickOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public async Task<T> EvaluateAsync<T>(string script, params object[] args)
        {
            var context = await GetMainContextAsync().ConfigureAwait(false);
            return await context.EvaluateAsync<T>(script, args).ConfigureAwait(false);
        }

        public Task<JsonElement?> EvaluateAsync(string script, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public Task<IJSHandle> EvaluateHandleAsync(string script, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public Task<IJSHandle> EvaluateHandleAsync(string expression)
        {
            throw new System.NotImplementedException();
        }

        public Task FillAsync(string selector, string text, WaitForSelectorOptions options = null)
        {
            throw new System.NotImplementedException();
        }

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

                var tasks = new List<Task>() { watcher.TimeoutOrTerminationTask };
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

        public Task<IResponse> GoToAsync(string url, WaitUntilNavigation waitUntil)
            => GoToAsync(url, new GoToOptions { WaitUntil = new[] { waitUntil } });

        public Task<IElementHandle> QuerySelectorAsync(string selector)
        {
            throw new System.NotImplementedException();
        }

        public Task QuerySelectorEvaluateAsync(string selector, string script, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public Task<T> QuerySelectorEvaluateAsync<T>(string selector, string script, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public Task SetContentAsync(string html, NavigationOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IResponse> WaitForNavigationAsync(WaitForNavigationOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IResponse> WaitForNavigationAsync(WaitUntilNavigation waitUntil)
        {
            throw new System.NotImplementedException();
        }

        public Task<IElementHandle> WaitForSelectorAsync(string selector, WaitForSelectorOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IFrameExecutionContext> GetUtilityContextAsync() => GetContextAsync(ContextType.Utility);

        public Task<IFrameExecutionContext> GetMainContextAsync() => GetContextAsync(ContextType.Main);

        internal void OnDetached()
        {
            _detached = true;
        }

        private void SetContext(ContextType contextType, IFrameExecutionContext context)
        {
            var data = _contextData[contextType];
            data.Context = context;
            if (context != null)
            {
                data.ContextResolveCallback.Invoke(context);

                // TODO: rerun rerunnableTasks
            }
            else
            {
                data.ContextResolveCallback = ctx => data.ContextTsc.TrySetResult(ctx);
            }
        }

        private Task<IFrameExecutionContext> GetContextAsync(ContextType contextType)
        {
            if (_detached)
            {
                throw new PlaywrightSharpException($"Execution Context is not available in detached frame \"{Url}\" (are you trying to evaluate?)");
            }

            return _contextData[contextType].ContextTask;
        }
    }
}
