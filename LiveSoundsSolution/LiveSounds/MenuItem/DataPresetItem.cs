using LiveSounds.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSounds.MenuItem
{
    /// <summary>
    /// DataPreset menu item.
    /// </summary>
    internal class DataPresetItem
    {
        /// <summary>
        /// DataPreset.
        /// </summary>
        internal DataPreset DataPreset { get; private set; }

        /// <summary>
        /// Creates menu item.
        /// </summary>
        /// <param name="dataPreset">DataPreset.</param>
        public DataPresetItem(DataPreset dataPreset)
        {
            this.DataPreset = dataPreset;
        }

        /// <summary>
        /// Returns display string.
        /// </summary>
        /// <returns>Display string of the menu.</returns>
        public override string ToString()
        {
            if (this.DataPreset == null)
            {
                return LocalizedInfo.MenuItemNotFoundForDataPreset;
            }

            var items = this.DataPreset.AudioItems;

            return String.Format(LocalizedInfo.MenuItemNameForDataPreset, this.DataPreset.Name,
                ((items != null) ? items.Length : 0));
        }
    }
}
