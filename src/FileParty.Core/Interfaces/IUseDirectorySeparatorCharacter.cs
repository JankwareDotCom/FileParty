namespace FileParty.Core.Interfaces
{
    public interface IUseDirectorySeparatorCharacter
    {
        /// <summary>
        ///     Directory Separator Character
        ///     In some instances, using Path.DirectorySeparatorCharacter will do the trick,
        ///     but in others, it should be explicitly defined.
        /// </summary>
        char DirectorySeparatorCharacter { get; }
    }
}