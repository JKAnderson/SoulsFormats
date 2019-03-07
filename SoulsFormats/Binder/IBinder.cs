using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A BND or BXF container of generic files.
    /// </summary>
    public interface IBinder
    {
        /// <summary>
        /// A code indicating which file data is present.
        /// </summary>
        Binder.Format Format { get; set; }

        /// <summary>
        /// Files in this binder.
        /// </summary>
        List<BinderFile> Files { get; set; }
    }
}
