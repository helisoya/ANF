using ANF.GUI;
using ANF.Scene;
using ANF.Utils;
using Leguar.TotalJSON;
using System.Collections.Generic;
using UnityEngine;

namespace ANF.Persistent
{
    /// <summary>
    /// Represents a tab of the settings. 
    /// Ex : The screen resolution part of the settings
    /// </summary>
    public interface SettingsTab : Jsonable
    {
        /// <summary>
        /// Initialize the part
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Resets the part to the default settings
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Gets the visual name/key for the part
        /// </summary>
        /// <returns>The name/key</returns>
        public abstract string GetName();

        /// <summary>
        /// Populates a root with the tab's content
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        /// <param name="menu">The settings menu</param>
        /// <param name="root">The tab's root</param>
        public abstract void PopulateTab(ANFManager manager, SettingsMenuUI menu, RectTransform root);

        /// <summary>
        /// Redraws the localized entries in a case of a language change
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        /// <param name="menu">The settings menu</param>
        /// <param name="root">The tab's root</param>
        public abstract void RedrawLocalizedEntries(ANFManager manager, SettingsMenuUI menu, RectTransform root);

        /// <summary>
        /// Applies the settings to the relevant places
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        public abstract void ApplySettings(ANFManager manager);
    }
}

