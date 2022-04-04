using System;
using FileParty.Core.Interfaces;

namespace FileParty.Core
{
    public abstract class BaseFilePartyModule<TStorageProvider, TAsyncStorageProvider> : IFilePartyModule
        where TStorageProvider : class, IStorageProvider
        where TAsyncStorageProvider : class, IAsyncStorageProvider
    {
        /// <summary>
        ///     Modules must have an empty constructor.  Any dependencies should be registered
        ///     in the empty constructor using the RegisterModuleDependency Extension Methods.
        /// </summary>
        protected BaseFilePartyModule()
        {
            return;
        }

        public virtual Type GetStorageProviderType()
        {
            return typeof(TStorageProvider);
        }

        public virtual Type GetAsyncStorageProviderType()
        {
            return typeof(TAsyncStorageProvider);
        }
    }
}