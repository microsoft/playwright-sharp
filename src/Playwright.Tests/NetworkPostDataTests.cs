using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright.Tests.BaseTests;
using Xunit;
using Xunit.Abstractions;


namespace Microsoft.Playwright.Tests
{
    /// <playwright-file>network-post-data.spec.ts</playwright-file>
    [Collection(TestConstants.TestFixtureBrowserCollectionName)]
    public sealed class NetworkPostDataTests : PlaywrightSharpPageBaseTest
    {

        /// <inheritdoc/>
        public NetworkPostDataTests(ITestOutputHelper output) :
                base(output)
        {
        }

        /// <playwright-file>network-post-data.spec.ts</playwright-file>
        /// <playwright-it>should return correct postData buffer for utf-8 body</playwright-it>
        [Fact(Timeout = 30_000)]
        public async Task ShouldReturnCorrectPostdataBufferForUtf8Body()
        {
            await Page.GotoAsync(TestConstants.EmptyPage);
            string value = "baẞ";

            var task = Page.WaitForEventAsync(PageEvent.Request);
            var actualTask = Page.EvaluateAsync(@$"() => {{
                      const request = new Request('{TestConstants.ServerUrl + "/title.html"}', {{
                        method: 'POST',
                        body: JSON.stringify('{value}'),
                      }});
                      request.headers.set('content-type', 'application/json;charset=UTF-8');
                      return fetch(request);
                    }}");

            await Task.WhenAll(task, actualTask);

            string expectedJsonValue = JsonSerializer.Serialize(value, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });

            var request = task.Result;
            Assert.Equal(expectedJsonValue, request.PostData);
            Assert.Equal(value, request.PostDataJSON().RootElement.GetString());
        }

        /// <playwright-file>network-post-data.spec.ts</playwright-file>
        /// <playwright-it>should return post data w/o content-type</playwright-it>
        [Fact(Timeout = 30_000)]
        public async Task ShouldReturnPostDataWOContentType()
        {
            await Page.GotoAsync(TestConstants.EmptyPage);

            var task = Page.WaitForEventAsync(PageEvent.Request);
            var actualTask = Page.EvaluateAsync(@"(url) => {
                      const request = new Request(url, {
                        method: 'POST',
                        body: JSON.stringify({ value: 42 }),
                      });
                      request.headers.set('content-type', '');
                      return fetch(request);
                    }", TestConstants.ServerUrl + "/title.html");

            await Task.WhenAll(task, actualTask);

            var request = task.Result;
            Assert.Equal(42, request.PostDataJSON().RootElement.GetProperty("value").GetInt32());
        }

        /// <playwright-file>network-post-data.spec.ts</playwright-file>
        /// <playwright-it>should throw on invalid JSON in post data</playwright-it>
        [Fact(Timeout = 30_000)]
        public async Task ShouldThrowOnInvalidJSONInPostData()
        {
            await Page.GotoAsync(TestConstants.EmptyPage);

            var task = Page.WaitForEventAsync(PageEvent.Request);
            var actualTask = Page.EvaluateAsync(@"(url) => {
                      const request = new Request(url, {
                        method: 'POST',
                        body: '<not a json>',
                      });
                      return fetch(request);
                    }", TestConstants.ServerUrl + "/title.html");

            await Task.WhenAll(task, actualTask);

            var request = task.Result;
            Assert.ThrowsAny<JsonException>(() => request.PostDataJSON());
        }

        /// <playwright-file>network-post-data.spec.ts</playwright-file>
        /// <playwright-it>should return post data for PUT requests</playwright-it>
        [Fact(Timeout = 30_000)]
        public async Task ShouldReturnPostDataForPUTRequests()
        {
            await Page.GotoAsync(TestConstants.EmptyPage);

            var task = Page.WaitForEventAsync(PageEvent.Request);
            var actualTask = Page.EvaluateAsync(@"(url) => {
                      const request = new Request(url, {
                        method: 'PUT',
                        body: JSON.stringify({ value: 42 }),
                      });
                      return fetch(request);
                    }", TestConstants.ServerUrl + "/title.html");

            await Task.WhenAll(task, actualTask);

            var request = task.Result;
            Assert.Equal(42, request.PostDataJSON().RootElement.GetProperty("value").GetInt32());
        }
    }
}
