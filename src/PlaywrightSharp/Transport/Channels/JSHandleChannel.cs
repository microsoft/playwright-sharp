using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using PlaywrightSharp.Helpers;

namespace PlaywrightSharp.Transport.Channels
{
    internal class JSHandleChannel : Channel<JSHandle>
    {
        public JSHandleChannel(string guid, Connection connection, JSHandle owner) : base(guid, connection, owner)
        {
        }

        internal event EventHandler<PreviewUpdatedEventArgs> PreviewUpdated;

        internal override void OnMessage(string method, JsonElement? serverParams)
        {
            switch (method)
            {
                case "previewUpdated":
                    PreviewUpdated?.Invoke(this, new PreviewUpdatedEventArgs { Preview = serverParams.Value.GetProperty("preview").ToString() });
                    break;
            }
        }

        internal Task<JsonElement?> EvaluateExpressionAsync(string script, bool isFunction, EvaluateArgument arg)
            => Connection.SendMessageToServerAsync<JsonElement?>(
                Guid,
                "evaluateExpression",
                new Dictionary<string, object>
                {
                    ["expression"] = script,
                    ["isFunction"] = isFunction,
                    ["arg"] = arg,
                });

        internal Task<JSHandleChannel> EvaluateExpressionHandleAsync(string script, bool isFunction, object arg)
            => Connection.SendMessageToServerAsync<JSHandleChannel>(
                Guid,
                "evaluateExpressionHandle",
                new Dictionary<string, object>
                {
                    ["expression"] = script,
                    ["isFunction"] = isFunction,
                    ["arg"] = arg,
                });

        internal Task<JsonElement> GetJsonValueAsync() => Connection.SendMessageToServerAsync<JsonElement>(Guid, "jsonValue", null);

        internal Task DisposeAsync() => Connection.SendMessageToServerAsync(Guid, "dispose", null);

        internal Task<JSHandleChannel> GetPropertyAsync(string propertyName)
            => Connection.SendMessageToServerAsync<JSHandleChannel>(
                Guid,
                "getProperty",
                new Dictionary<string, object>
                {
                    ["name"] = propertyName,
                });

        internal async Task<List<JSElementProperty>> GetPropertiesAsync()
            => (await Connection.SendMessageToServerAsync(Guid, "getPropertyList", null).ConfigureAwait(false))?
                .GetProperty("properties").ToObject<List<JSElementProperty>>(Connection.GetDefaultJsonSerializerOptions());

        internal class JSElementProperty
        {
            public string Name { get; set; }

            public JSHandleChannel Value { get; set; }
        }
    }
}
