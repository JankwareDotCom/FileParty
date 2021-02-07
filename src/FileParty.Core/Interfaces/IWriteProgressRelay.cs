using FileParty.Core.EventArgs;

namespace FileParty.Core.Interfaces
{
    public interface IWriteProgressRelay
    {
        void RelayWriteProgressEvent(object sender, WriteProgressEventArgs writeProgressEventArgs);
    }
}