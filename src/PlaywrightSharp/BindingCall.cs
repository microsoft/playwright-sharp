using System;
using PlaywrightSharp.Transport;
using PlaywrightSharp.Transport.Channel;
using PlaywrightSharp.Transport.Protocol;

namespace PlaywrightSharp
{
    internal class BindingCall : IChannelOwner
    {
        private readonly ConnectionScope _scope;
        private readonly BindingCallChannel _channel;

        public BindingCall(ConnectionScope scope, string guid, BindingCallInitializer initializer)
        {
            _scope = scope;
            _channel = new BindingCallChannel(guid, scope);
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        ConnectionScope IChannelOwner.Scope => _scope;

        /// <inheritdoc/>
        Channel IChannelOwner.Channel => _channel;
    }
}
