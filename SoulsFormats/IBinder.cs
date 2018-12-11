using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A read-only collection of files in a bnd or bxf format.
    /// </summary>
    public interface IBinder
    {
        /// <summary>
        /// Files in this binder.
        /// </summary>
        IReadOnlyList<IBinderFile> Files { get; }
    }

    /// <summary>
    /// A read-only file in a bnd or bxf format.
    /// </summary>
    public interface IBinderFile
    {
        /// <summary>
        /// The ID of this file.
        /// </summary>
        int ID { get; }

        /// <summary>
        /// The name of this file, typically a network path.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The raw data of this file.
        /// </summary>
        byte[] Bytes { get; }
    }
}
