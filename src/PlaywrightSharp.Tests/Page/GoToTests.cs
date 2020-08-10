using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using PlaywrightSharp.Tests.Attributes;
using PlaywrightSharp.Tests.BaseTests;
using PlaywrightSharp.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace PlaywrightSharp.Tests.Page
{
    ///<playwright-file>navigation.spec.js</playwright-file>
    ///<playwright-describe>Page.goto</playwright-describe>
    [Collection(TestConstants.TestFixtureBrowserCollectionName)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1000:Test classes must be public", Justification = "Disabled")]
    class GoToTests : PlaywrightSharpPageBaseTest
    {
        /// <inheritdoc/>
        public GoToTests(ITestOutputHelper output) : base(output)
        {
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should work</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldWork()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            Assert.Equal(TestConstants.EmptyPage, Page.Url);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should work cross-process</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldWorkCrossProcess()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            Assert.Equal(TestConstants.EmptyPage, Page.Url);

            string url = TestConstants.CrossProcessHttpPrefix + "/empty.html";
            IFrame requestFrame = null;
            Page.Request += (sender, e) =>
            {
                if (e.Request.Url == url)
                {
                    requestFrame = e.Request.Frame;
                }
            };

            var response = await Page.GoToAsync(url);
            Assert.Equal(url, Page.Url);
            Assert.Same(Page.MainFrame, response.Frame);
            Assert.Same(Page.MainFrame, requestFrame);
            Assert.Equal(url, response.Url);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should capture iframe navigation request</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldCaptureIframeNavigationRequest()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            Assert.Equal(TestConstants.EmptyPage, Page.Url);

            IFrame requestFrame = null;
            Page.Request += (sender, e) =>
            {
                if (e.Request.Url == TestConstants.ServerUrl + "/frames/frame.html")
                {
                    requestFrame = e.Request.Frame;
                }
            };

            var response = await Page.GoToAsync(TestConstants.ServerUrl + "/frames/one-frame.html");
            Assert.Equal(TestConstants.ServerUrl + "/frames/one-frame.html", Page.Url);
            Assert.Same(Page.MainFrame, response.Frame);
            Assert.Equal(TestConstants.ServerUrl + "/frames/one-frame.html", response.Url);

            Assert.Equal(2, Page.Frames.Length);
            Assert.Same(Page.FirstChildFrame(), requestFrame);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should capture cross-process iframe navigation request</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldCaptureCrossProcessIframeNavigationRequest()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            Assert.Equal(TestConstants.EmptyPage, Page.Url);

            IFrame requestFrame = null;
            Page.Request += (sender, e) =>
            {
                if (e.Request.Url == TestConstants.CrossProcessHttpPrefix + "/frames/frame.html")
                {
                    requestFrame = e.Request.Frame;
                }
            };

            var response = await Page.GoToAsync(TestConstants.CrossProcessHttpPrefix + "/frames/one-frame.html");
            Assert.Equal(TestConstants.CrossProcessHttpPrefix + "/frames/one-frame.html", Page.Url);
            Assert.Same(Page.MainFrame, response.Frame);
            Assert.Equal(TestConstants.CrossProcessHttpPrefix + "/frames/one-frame.html", response.Url);

            Assert.Equal(2, Page.Frames.Length);
            Assert.Same(Page.FirstChildFrame(), requestFrame);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should work with anchor navigation</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldWorkWithAnchorNavigation()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            Assert.Equal(TestConstants.EmptyPage, Page.Url);
            await Page.GoToAsync($"{TestConstants.EmptyPage}#foo");
            Assert.Equal($"{TestConstants.EmptyPage}#foo", Page.Url);
            await Page.GoToAsync($"{TestConstants.EmptyPage}#bar");
            Assert.Equal($"{TestConstants.EmptyPage}#bar", Page.Url);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should work with redirects</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldWorkWithRedirects()
        {
            Server.SetRedirect("/redirect/1.html", "/redirect/2.html");
            Server.SetRedirect("/redirect/2.html", "/empty.html");

            await Page.GoToAsync(TestConstants.ServerUrl + "/redirect/1.html");
            await Page.GoToAsync(TestConstants.EmptyPage);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should navigate to about:blank</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldNavigateToAboutBlank()
        {
            var response = await Page.GoToAsync(TestConstants.AboutBlank);
            Assert.Null(response);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should return response when page changes its URL after load</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldReturnResponseWhenPageChangesItsURLAfterLoad()
        {
            var response = await Page.GoToAsync(TestConstants.ServerUrl + "/historyapi.html");
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should work with subframes return 204</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldWorkWithSubframesReturn204()
        {
            Server.SetRoute("/frames/frame.html", context =>
            {
                context.Response.StatusCode = 204;
                return Task.CompletedTask;
            });
            await Page.GoToAsync(TestConstants.ServerUrl + "/frames/one-frame.html");
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should work with subframes return 204</playwright-it>
        [SkipBrowserAndPlatformFact(skipWebkit: true)]
        public async Task ShouldFailWhenServerReturns204()
        {
            Server.SetRoute("/empty.html", context =>
            {
                context.Response.StatusCode = 204;
                return Task.CompletedTask;
            });
            var exception = await Assert.ThrowsAnyAsync<PlaywrightSharpException>(
                () => Page.GoToAsync(TestConstants.EmptyPage));
            Assert.Contains(TestConstants.IsChromium ? "net::ERR_ABORTED" : "NS_BINDING_ABORTED", exception.Message);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should navigate to empty page with domcontentloaded</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldNavigateToEmptyPageWithDOMContentLoaded()
        {
            var response = await Page.GoToAsync(TestConstants.EmptyPage, waitUntil: new[]
            {
                LifecycleEvent.DOMContentLoaded
            });
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should work when page calls history API in beforeunload</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldWorkWhenPageCallsHistoryAPIInBeforeunload()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            await Page.EvaluateAsync(@"() =>
            {
                window.addEventListener('beforeunload', () => history.replaceState(null, 'initial', window.location.href), false);
            }");
            var response = await Page.GoToAsync(TestConstants.ServerUrl + "/grid.html");
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should fail when navigating to bad url</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldFailWhenNavigatingToBadUrl()
        {
            var exception = await Assert.ThrowsAnyAsync<PlaywrightSharpException>(async () => await Page.GoToAsync("asdfasdf"));
            if (TestConstants.IsChromium || TestConstants.IsWebKit)
            {
                Assert.Contains("Cannot navigate to invalid URL", exception.Message);
            }
            else
            {
                Assert.Contains("Invalid url", exception.Message);
            }
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should fail when navigating to bad SSL</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldFailWhenNavigatingToBadSSL()
        {
            Page.Request += (sender, e) => Assert.NotNull(e.Request);
            Page.RequestFinished += (sender, e) => Assert.NotNull(e.Request);
            Page.RequestFailed += (sender, e) => Assert.NotNull(e.Request);

            var exception = await Assert.ThrowsAnyAsync<PlaywrightSharpException>(async () => await Page.GoToAsync(TestConstants.HttpsPrefix + "/empty.html"));
            TestUtils.AssertSSLError(exception.Message);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should fail when navigating to bad SSL after redirects</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldFailWhenNavigatingToBadSSLAfterRedirects()
        {
            Server.SetRedirect("/redirect/1.html", "/redirect/2.html");
            Server.SetRedirect("/redirect/2.html", "/empty.html");
            var exception = await Assert.ThrowsAnyAsync<PlaywrightSharpException>(async () => await Page.GoToAsync(TestConstants.HttpsPrefix + "/redirect/1.html"));
            TestUtils.AssertSSLError(exception.Message);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should not crash when navigating to bad SSL after a cross origin navigation</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldNotCrashWhenNavigatingToBadSSLAfterACrossOriginNavigation()
        {
            await Page.GoToAsync(TestConstants.CrossProcessHttpPrefix + "/empty.html");
            await Page.GoToAsync(TestConstants.HttpsPrefix + "/empty.html").ContinueWith(_ => { });
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should throw if networkidle is passed as an option</playwright-it>
        [Fact(Skip = "We don't need this test")]
        public void ShouldThrowIfNetworkidleIsPassedAsAnOption() { }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should throw if networkidle is passed as an option</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldFailWhenMainResourcesFailedToLoad()
        {
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () => await Page.GoToAsync("http://localhost:44123/non-existing-url"));

            if (TestConstants.IsChromium)
            {
                Assert.Contains("net::ERR_CONNECTION_REFUSED", exception.Message);
            }
            else if (TestConstants.IsWebKit && TestConstants.IsWindows)
            {
                Assert.Contains("Couldn't connect to server", exception.Message);
            }
            else if (TestConstants.IsWebKit)
            {
                Assert.Contains("Could not connect", exception.Message);
            }
            else
            {
                Assert.Contains("NS_ERROR_CONNECTION_REFUSED", exception.Message);
            }
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should fail when exceeding maximum navigation timeout</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldFailWhenExceedingMaximumNavigationTimeout()
        {
            Server.SetRoute("/empty.html", context => Task.Delay(-1));
            var exception = await Assert.ThrowsAsync<TimeoutException>(async ()
                => await Page.GoToAsync(TestConstants.EmptyPage, new GoToOptions { Timeout = 1 }));
            Assert.Contains("Timeout of 1 ms exceeded", exception.Message);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should fail when exceeding maximum navigation timeout</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldFailWhenExceedingDefaultMaximumNavigationTimeout()
        {
            Server.SetRoute("/empty.html", context => Task.Delay(-1));
            Page.DefaultNavigationTimeout = 1;
            var exception = await Assert.ThrowsAsync<TimeoutException>(async () => await Page.GoToAsync(TestConstants.EmptyPage));
            Assert.Contains("Timeout of 1 ms exceeded", exception.Message);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should fail when exceeding default maximum timeout</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldFailWhenExceedingDefaultMaximumTimeout()
        {
            // Hang for request to the empty.html
            Server.SetRoute("/empty.html", context => Task.Delay(-1));
            Page.DefaultTimeout = 1;
            var exception = await Assert.ThrowsAsync<TimeoutException>(async () => await Page.GoToAsync(TestConstants.EmptyPage));
            Assert.Contains("Timeout of 1 ms exceeded", exception.Message);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should fail when exceeding default maximum timeout</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldPrioritizeDefaultNavigationTimeoutOverDefaultTimeout()
        {
            // Hang for request to the empty.html
            Server.SetRoute("/empty.html", context => Task.Delay(-1));
            Page.DefaultTimeout = 0;
            Page.DefaultNavigationTimeout = 1;
            var exception = await Assert.ThrowsAnyAsync<TimeoutException>(async () => await Page.GoToAsync(TestConstants.EmptyPage));
            Assert.Contains("Timeout of 1 ms exceeded", exception.Message);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should disable timeout when its set to 0</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldDisableTimeoutWhenItsSetTo0()
        {
            bool loaded = false;
            void OnLoad(object sender, EventArgs e)
            {
                loaded = true;
                Page.Load -= OnLoad;
            }
            Page.Load += OnLoad;

            await Page.GoToAsync(TestConstants.ServerUrl + "/grid.html", new GoToOptions { Timeout = 0, WaitUntil = LifecycleEvent.Load });
            Assert.True(loaded);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should work when navigating to valid url</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldWorkWhenNavigatingToValidUrl()
        {
            var response = await Page.GoToAsync(TestConstants.EmptyPage);
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should work when navigating to data url</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldWorkWhenNavigatingToDataUrl()
        {
            var response = await Page.GoToAsync("data:text/html,hello");
            Assert.Null(response);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should work when navigating to 404</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldWorkWhenNavigatingTo404()
        {
            var response = await Page.GoToAsync(TestConstants.ServerUrl + "/not-found");
            Assert.Equal(HttpStatusCode.NotFound, response.Status);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should return last response in redirect chain</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldReturnLastResponseInRedirectChain()
        {
            Server.SetRedirect("/redirect/1.html", "/redirect/2.html");
            Server.SetRedirect("/redirect/2.html", "/redirect/3.html");
            Server.SetRedirect("/redirect/3.html", TestConstants.EmptyPage);

            var response = await Page.GoToAsync(TestConstants.ServerUrl + "/redirect/1.html");
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.Equal(TestConstants.EmptyPage, response.Url);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should not leak listeners during navigation</playwright-it>
        [Fact(Skip = "We don't need this test")]
        public void ShouldNotLeakListenersDuringNavigation() { }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should not leak listeners during bad navigation</playwright-it>
        [Fact(Skip = "We don't need this test")]
        public void ShouldNotLeakListenersDuringBadNavigation() { }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should not leak listeners during navigation of 11 pages</playwright-it>
        [Fact(Skip = "We don't need this test")]
        public void ShouldNotLeakListenersDuringNavigationOf11Pages() { }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should navigate to dataURL and not fire dataURL requests</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldNavigateToDataURLAndNotFireDataURLRequests()
        {
            var requests = new List<IRequest>();
            Page.Request += (sender, e) => requests.Add(e.Request);

            string dataUrl = "data:text/html,<div>yo</div>";
            var response = await Page.GoToAsync(dataUrl);
            Assert.Null(response);
            Assert.Empty(requests);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should navigate to URL with hash and fire requests without hash</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldNavigateToURLWithHashAndFireRequestsWithoutHash()
        {
            var requests = new List<IRequest>();
            Page.Request += (sender, e) => requests.Add(e.Request);

            var response = await Page.GoToAsync(TestConstants.EmptyPage + "#hash");
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.Equal(TestConstants.EmptyPage, response.Url);
            Assert.Single(requests);
            Assert.Equal(TestConstants.EmptyPage, requests[0].Url);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should work with self requesting page</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldWorkWithSelfRequestingPage()
        {
            var response = await Page.GoToAsync(TestConstants.ServerUrl + "/self-request.html");
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.Contains("self-request.html", response.Url);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should fail when navigating and show the url at the error message</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldFailWhenNavigatingAndShowTheUrlAtTheErrorMessage()
        {
            const string url = TestConstants.HttpsPrefix + "/redirect/1.html";
            var exception = await Assert.ThrowsAnyAsync<NavigationException>(async () => await Page.GoToAsync(url));
            Assert.Contains(url, exception.Message);
            Assert.Contains(url, exception.Url);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should send referer</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldSendReferer()
        {
            string referer1 = null;
            string referer2 = null;

            await TaskUtils.WhenAll(
                Server.WaitForRequest("/grid.html", r => referer1 = r.Headers["Referer"]),
                Server.WaitForRequest("/digits/1.png", r => referer2 = r.Headers["Referer"]),
                Page.GoToAsync(TestConstants.ServerUrl + "/grid.html", new GoToOptions
                {
                    Referer = "http://google.com/"
                })
            );

            Assert.Equal("http://google.com/", referer1);
            // Make sure subresources do not inherit referer.
            Assert.Equal(TestConstants.ServerUrl + "/grid.html", referer2);
            Assert.Equal(TestConstants.ServerUrl + "/grid.html", Page.Url);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should reject referer option when setExtraHTTPHeaders provides referer</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldRejectRefererOptionWhenSetExtraHTTPHeadersProvidesReferer()
        {
            await Page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
            {
                ["referer"] = "http://microsoft.com/"
            });

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await Page.GoToAsync(TestConstants.ServerUrl + "/grid.html", new GoToOptions
                {
                    Referer = "http://google.com/"
                }));

            Assert.Equal("\"referer\" is already specified as extra HTTP header", exception.Message);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should override referrer-policy</playwright-it>
        [Fact(Timeout = PlaywrightSharp.Playwright.DefaultTimeout)]
        public async Task ShouldOverrideReferrerPolicy()
        {
            Server.Subscribe("/grid.html", context =>
            {
                context.Response.Headers["Referrer-Policy"] = "no-referrer";
            });

            string referer1 = null;
            string referer2 = null;

            var reqTask1 = Server.WaitForRequest("/grid.html", r => referer1 = r.Headers["Referer"]);
            var reqTask2 = Server.WaitForRequest("/digits/1.png", r => referer2 = r.Headers["Referer"]);
            await TaskUtils.WhenAll(
                reqTask1,
                reqTask2,
                Page.GoToAsync(TestConstants.ServerUrl + "/grid.html", new GoToOptions
                {
                    Referer = "http://microsoft.com/"
                })
            );

            Assert.Equal("http://microsoft.com/", referer1);
            // Make sure subresources do not inherit referer.
            Assert.Null(referer2);
            Assert.Equal(TestConstants.ServerUrl + "/grid.html", Page.Url);
        }

        ///<playwright-file>navigation.spec.js</playwright-file>
        ///<playwright-describe>Page.goto</playwright-describe>
        ///<playwright-it>should fail when canceled by another navigation</playwright-it>
        [SkipBrowserAndPlatformFact(skipFirefox: true)]
        public async Task ShouldFailWhenCanceledByAnotherNavigation()
        {
            Server.SetRoute("/one-style.css", context => Task.Delay(10_000));
            var request = Server.WaitForRequest("/one-style.css");
            var failed = Page.GoToAsync(TestConstants.ServerUrl + "/one-style.html", TestConstants.IsFirefox ? LifecycleEvent.Networkidle : LifecycleEvent.Load);
            await request;
            await Page.GoToAsync(TestConstants.EmptyPage);

            var exception = await Assert.ThrowsAnyAsync<PlaywrightSharpException>(async () => await failed);

            Assert.Equal($"Navigation to {TestConstants.ServerUrl}/one-style.html was canceled by another one", exception.Message);
        }
    }
}
