using System;
using System.Collections.Generic;
using FileParty.Core.Models;

namespace FileParty.Core.Interfaces
{
    public delegate void WriteProgressHandler(object sender, WriteProgressInfo info);

    public interface IWriteProgressSubscriptionManager
    {
        /// <summary>
        ///     Subscribes to all write progress events
        /// </summary>
        /// <param name="handler"></param>
        /// <returns>Handler Id</returns>
        Guid SubscribeToAll(WriteProgressHandler handler);

        /// <summary>
        ///     Subscribes to write progress events for a given requestId.
        /// </summary>
        /// <param name="requestId">Id from <see cref="FilePartyWriteRequest" /></param>
        /// <param name="handler"></param>
        /// <returns>Handler Id</returns>
        Guid SubscribeToRequest(Guid requestId, WriteProgressHandler handler);

        /// <summary>
        ///     UnSubscribes handler from all write progress events
        /// </summary>
        /// <param name="handlerId"></param>
        void UnsubscribeFromAll(Guid handlerId);

        /// <summary>
        ///     Unsubscribes all handlers for request
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="handlerId"></param>
        void UnsubscribeFromRequest(Guid requestId, Guid handlerId);

        List<WriteProgressHandler> GetHandlers();

        Dictionary<Guid, List<Guid>> GetRequestHandlerIds();

        List<WriteProgressHandler> GetRequestHandlers(Guid requestId);
    }
}