using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Playwright.Protocol
{
    internal class ConnectionCallback
    {
        public TaskCompletionSource<JsonElement?> TaskCompletionSource { get; set; }

        public bool TreatErrorPropertyAsError { get; set; }
    }
}
