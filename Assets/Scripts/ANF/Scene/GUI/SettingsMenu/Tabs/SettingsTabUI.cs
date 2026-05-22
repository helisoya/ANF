using ANF.Locals;
using ANF.Persistent;
using ANF.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a visual tab in the settings menu
    /// </summary>
    public abstract class SettingsTabUI : MonoBehaviour
    {
        [SerializeField] protected Locals.LocalizedText tabNameText;
        [SerializeField] protected RectTransform root;

        protected SettingsMenuUI menu;
        protected ANFManager manager;

        /// <summary>
        /// Gets the tab's label
        /// </summary>
        /// <returns>The label's key</returns>
        public abstract string GetLabelKey();

        /// <summary>
        /// Initialize the component
        /// </summary>
        /// <param name="menu">The settings menu</param>
        /// <param name="manager">The ANF Manager</param>
        /// <param name="tab">The linked settings tab</param>
        public void Initialize(SettingsMenuUI menu, ANFManager manager)
        {
            this.menu = menu;
            this.manager = manager;

            tabNameText.SetNewKey(GetLabelKey());
            PopulateTab();
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        /// <summary>
        /// Populates the tab
        /// </summary>
        public abstract void PopulateTab();

        /// <summary>
        /// Redraws the tab's content
        /// </summary>
        public abstract void RedrawLocalizedElements();
    }

}
