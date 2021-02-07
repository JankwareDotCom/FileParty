using System;
using FileParty.Core.EventArgs;

namespace FileParty.Core.Interfaces
{
    public interface IConfiguredFileParty
    {
        /// <summary>
        /// The default module type
        /// </summary>
        Type DefaultModuleType { get; set; }
    }
}