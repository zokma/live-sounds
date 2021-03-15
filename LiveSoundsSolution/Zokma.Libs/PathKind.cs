using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zokma.Libs
{
    /// <summary>
    /// Kind of path.
    /// </summary>
    public enum PathKind
    {
        /// <summary>
        /// Application Root path.
        /// </summary>
        ApplicationRoot,

        /// <summary>
        /// Application roaming data path.
        /// </summary>
        ApplicationData,

        /// <summary>
        /// Application local data path.
        /// </summary>
        LocalApplicationData,

        /// <summary>
        /// User specific data path.
        /// </summary>
        Personal,
    }
}
