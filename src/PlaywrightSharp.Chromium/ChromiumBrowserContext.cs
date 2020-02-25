﻿using System;
using System.Threading.Tasks;
using PlaywrightSharp.Chromium;
using PlaywrightSharp.Chromium.Messaging.Target;

namespace PlaywrightSharp
{
    /// <inheritdoc cref="IBrowserContextDelegate"/>
    public class ChromiumBrowserContext : IBrowserContextDelegate
    {
        private readonly ChromiumSession _client;
        private readonly string _contextId;

        internal ChromiumBrowserContext(
            ChromiumSession client,
            ChromiumBrowser chromiumBrowser,
            string contextId,
            BrowserContextOptions options)
        {
            _client = client;
            Browser = chromiumBrowser;
            _contextId = contextId;
            Options = options;
        }

        /// <inheritdoc cref="IBrowserContext"/>
        public BrowserContextOptions Options { get; }

        internal ChromiumBrowser Browser { get; }

        /// <inheritdoc cref="IBrowserContext"/>
        public async Task<IPage> NewPage()
        {
            var createTargetRequest = new TargetCreateTargetRequest
            {
                Url = "about:blank",
            };

            if (_contextId != null)
            {
                createTargetRequest.BrowserContextId = _contextId;
            }

            string targetId = (await _client.SendAsync<TargetCreateTargetResponse>("Target.createTarget", createTargetRequest)
                .ConfigureAwait(false)).TargetId;
            var target = Browser.TargetsMap[targetId];
            await target.InitializedTask.ConfigureAwait(false);
            return await target.PageAsync().ConfigureAwait(false);
        }
    }
}