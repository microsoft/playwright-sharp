using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlaywrightSharp.Firefox.Messaging;
using PlaywrightSharp.Firefox.Protocol;
using PlaywrightSharp.Firefox.Protocol.Target;
using PlaywrightSharp.Helpers;
using PlaywrightSharp.Messaging;

namespace PlaywrightSharp.Firefox
{
    internal class FirefoxConnection
    {
        internal const int BrowserCloseMessageId = -9999;

        private readonly AsyncDictionaryHelper<string, FirefoxSession> _asyncSessions;
        private readonly IConnectionTransport _transport;
        private readonly ConcurrentDictionary<string, FirefoxSession> _sessions = new ConcurrentDictionary<string, FirefoxSession>();
        private readonly ConcurrentDictionary<int, MessageTask<IFirefoxResponse>> _callbacks = new ConcurrentDictionary<int, MessageTask<IFirefoxResponse>>();
        private int _lastId;

        public FirefoxConnection(IConnectionTransport transport)
        {
            _transport = transport;
            _transport.MessageReceived += Transport_MessageReceived;
            _transport.Closed += Transport_Closed;

            _asyncSessions = new AsyncDictionaryHelper<string, FirefoxSession>(_sessions, "Session {0} not found");
        }

        public event EventHandler<IFirefoxEvent> MessageReceived;

        public event EventHandler<TransportClosedEventArgs> Disconnected;

        internal bool IsClosed { get; set; }

        public void Dispose()
        {
        }

        internal int GetMessageId() => Interlocked.Increment(ref _lastId);

        internal async Task<TFirefoxResponse> SendAsync<TFirefoxResponse>(IFirefoxRequest<TFirefoxResponse> request)
            where TFirefoxResponse : IFirefoxResponse
        {
            int id = GetMessageId();
            MessageTask<IFirefoxResponse> callback = new MessageTask<IFirefoxResponse>
            {
                TaskWrapper = new TaskCompletionSource<IFirefoxResponse>(),
                Method = request.Command,
            };
            _callbacks[id] = callback;

            try
            {
                await RawSendAsync(new ConnectionRequest
                {
                    Id = id,
                    Method = request.Command,
                    Params = request,
                }).ConfigureAwait(false);
            }

            // We need to silence exceptions on async void events.
#pragma warning disable CA1031 // Do not catch general exception types.
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types.
            {
                if (_callbacks.TryRemove(id, out _))
                {
                    callback.TaskWrapper.TrySetException(new MessageException(ex.Message, ex));
                }
            }

            var result = await callback.TaskWrapper.Task.ConfigureAwait(false);
            return (TFirefoxResponse)result;
        }

        internal Task RawSendAsync(ConnectionRequest request) => _transport.SendAsync(request.ToJson());

        internal void Close(string closeReason)
        {
            if (!IsClosed)
            {
                _transport.Close(closeReason);
            }
        }

        internal FirefoxSession GetSession(string sessionId) => _sessions.GetValueOrDefault(sessionId);

        private void Transport_Closed(object sender, TransportClosedEventArgs e) => Disconnected?.Invoke(this, e);

        private void Transport_MessageReceived(object sender, MessageReceivedEventArgs e) => ProcessMessage(e);

        private void ProcessMessage(MessageReceivedEventArgs e)
        {
            try
            {
                string response = e.Message;
                ConnectionResponse obj = null;

                try
                {
                    obj = JsonSerializer.Deserialize<ConnectionResponse>(response, JsonHelper.DefaultJsonSerializerOptions);
                }
                catch (JsonException ex)
                {
                    // _logger.LogError(ex, "Failed to deserialize response", response);
                    System.Diagnostics.Debug.WriteLine($"{ex}: Failed to deserialize response {response}");
                    return;
                }

                // _logger.LogTrace("◀ Receive {Message}", response);
                ProcessIncomingMessage(obj);
            }

            // We need to silence exceptions on async void events.
#pragma warning disable CA1031 // Do not catch general exception types.
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types.
            {
                string message = $"Connection failed to process {e.Message}. {ex.Message}. {ex.StackTrace}";

                // _logger.LogError(ex, message);
                System.Diagnostics.Debug.WriteLine(message);
                Close(message);
            }
        }

        private void ProcessIncomingMessage(ConnectionResponse obj)
        {
            if (obj.Id == BrowserCloseMessageId)
            {
                return;
            }

            var param = FirefoxProtocolTypes.ParseEvent(obj.Method, obj.Params.Value.GetRawText());
            if (param is TargetAttachedToTargetFirefoxEvent targetAttachedToTarget)
            {
                string sessionId = targetAttachedToTarget.SessionId;
                var session = new FirefoxSession(this, targetAttachedToTarget.TargetInfo.Type, sessionId);
                _asyncSessions.AddItem(sessionId, session);
            }
            else if (param is TargetDetachedFromTargetFirefoxEvent targetDetachedFromTarget)
            {
                string sessionId = targetDetachedFromTarget.SessionId;
                if (_sessions.TryRemove(sessionId, out var session))
                {
                    session.OnClosed(targetDetachedFromTarget.InternalName);
                }
            }

            if (obj.SessionId != null)
            {
                GetSession(obj.SessionId)?.OnMessage(obj);
            }
            else if (obj.Id.HasValue && _callbacks.TryRemove(obj.Id.Value, out var callback))
            {
                if (obj.Error != null)
                {
                    callback.TaskWrapper.TrySetException(new MessageException(callback, obj.Error));
                }
                else
                {
                    callback.TaskWrapper.TrySetResult(FirefoxProtocolTypes.ParseResponse(obj.Method, obj.Result.Value.GetRawText()));
                }
            }
            else
            {
                MessageReceived?.Invoke(this, param);
            }
        }
    }
}
