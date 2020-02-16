using System;
using System.Threading.Tasks;
using PlaywrightSharp.Tests.Attributes;
using PlaywrightSharp.Tests.BaseTests;
using Xunit;
using Xunit.Abstractions;

namespace PlaywrightSharp.Tests.ElementHandle
{
    ///<playwright-file>elementhandle.spec.js</playwright-file>
    ///<playwright-describe>ElementHandle.scrollIntoViewIfNeeded</playwright-describe>
    public class ElementHandleScrollIntoViewIfNeededTests : PlaywrightSharpPageBaseTest
    {
        internal ElementHandleScrollIntoViewIfNeededTests(ITestOutputHelper output) : base(output)
        {
        }

        ///<playwright-file>elementhandle.spec.js</playwright-file>
        ///<playwright-describe>ElementHandle.scrollIntoViewIfNeeded</playwright-describe>
        ///<playwright-it>should work</playwright-it>
        [SkipBrowserAndPlatformFact(skipFirefox: true)]
        public async Task ShouldWork()
        {
            await Page.GoToAsync(TestConstants.ServerUrl + "/offscreenbuttons.html");
            for (var i = 0; i < 11; ++i)
            {
                var button = await Page.QuerySelectorAsync("#btn" + i);
                var before = await button.GetVisibleRatioAsync();
                Assert.Equal(10 - i, Math.Round(before * 10));
                await button.ScrollIntoViewIfNeededAsync();
                var after = await button.GetVisibleRatioAsync();
                Assert.Equal(10, Math.Round(after * 10));
                await Page.EvaluateAsync("() => window.scrollTo(0, 0)");
            }
        }
    }
}
