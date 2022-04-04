using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FileParty.Core.Interfaces;

namespace FileParty.Core.WriteProgress
{
    public class WriteProgressSubscriptionManager : IWriteProgressSubscriptionManager
    {
        private readonly ConcurrentDictionary<Guid, WriteProgressHandler> _allHandlers = new ConcurrentDictionary<Guid, WriteProgressHandler>();

        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, WriteProgressHandler>> _requestHandlers =
            new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, WriteProgressHandler>>();

        public Guid SubscribeToAll(WriteProgressHandler handler)
        {
            var id = Guid.NewGuid();
            _allHandlers[id] = handler;
            return id;
        }

        public Guid SubscribeToRequest(Guid requestId, WriteProgressHandler handler)
        {
            var handlers = _requestHandlers.GetOrAdd(requestId, new ConcurrentDictionary<Guid, WriteProgressHandler>());
            var id = Guid.NewGuid();
            handlers[id] = handler;
            return id;
        }

        public void UnsubscribeFromAll(Guid handlerId)
        {
            _allHandlers.TryRemove(handlerId, out _);
        }

        public void UnsubscribeFromRequest(Guid requestId, Guid handlerId)
        {
            if (!_requestHandlers.ContainsKey(requestId)) return;

            if (!_requestHandlers[requestId].ContainsKey(handlerId)) return;

            _requestHandlers[requestId].TryRemove(handlerId, out _);

            if (_requestHandlers[requestId].Any()) return;

            _requestHandlers.TryRemove(requestId, out _);
        }

        public List<WriteProgressHandler> GetHandlers()
        {
            return _allHandlers.Values.ToList();
        }

        public Dictionary<Guid, List<Guid>> GetRequestHandlerIds()
        {
            return _requestHandlers
                .ToDictionary(k => k.Key, v => v.Value.Keys.ToList());
        }

        public List<WriteProgressHandler> GetRequestHandlers(Guid requestId)
        {
            return !_requestHandlers.ContainsKey(requestId)
                ? new List<WriteProgressHandler>()
                : _requestHandlers[requestId].Values.ToList();
        }
    }
}