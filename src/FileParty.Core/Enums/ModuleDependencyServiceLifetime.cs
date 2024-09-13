namespace FileParty.Core.Enums
{
    public enum ModuleDependencyServiceLifetime
    {
        /// <summary>
        ///     single instance of the service will be created
        /// </summary>
        Singleton = 0,

        /// <summary>
        ///     new instance of the service will be created for each scope
        /// </summary>
        Scoped = 1,

        /// <summary>
        ///     new instance of the service will be created every time it is requested
        /// </summary>
        Transient = 2
    }
}