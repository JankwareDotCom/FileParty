using System.Linq;
using FileParty.Core.EventArgs;
using FileParty.Core.Interfaces;

namespace FileParty.Core.WriteProgress
{
    public class WriteProgressRelay : IWriteProgressRelay
    {
        private readonly IWriteProgressSubscriptionManager _subscriptionManager;
        
        public WriteProgressRelay(IWriteProgressSubscriptionManager subscriptionManager)
        {
            _subscriptionManager = subscriptionManager;
        }
        
        public void RelayWriteProgressEvent(object sender, WriteProgressEventArgs writeProgressEventArgs)
        {
            var requestHandlers = _subscriptionManager.GetRequestHandlers(writeProgressEventArgs.WriteRequestId);

            if (requestHandlers.Any())
            {
                requestHandlers.ForEach(x=>x.Invoke(sender, writeProgressEventArgs.WriteProgressInfo));
                
                if (writeProgressEventArgs.WriteProgressInfo.PercentComplete >= 100)
                {
                    _subscriptionManager.GetRequestHandlerIds()[writeProgressEventArgs.WriteRequestId]
                        .ForEach(x => _subscriptionManager.UnsubscribeFromRequest(writeProgressEventArgs.WriteRequestId, x));
                }
            }
            
            _subscriptionManager.GetHandlers().ForEach(x => x.Invoke(sender, writeProgressEventArgs.WriteProgressInfo));
        }
    }
}