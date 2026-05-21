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
    public class SettingsTabUI : MonoBehaviour
    {
        [SerializeField] private Locals.LocalizedText tabNameText;
        [SerializeField] private RectTransform root;

        private SettingsTab tab;
        private SettingsMenuUI menu;

        /// <summary>
        /// Initialize the component
        /// </summary>
        /// <param name="menu">The settings menu</param>
        /// <param name="manager">The ANF Manager</param>
        /// <param name="tab">The linked settings tab</param>
        public void Initialize(SettingsMenuUI menu, ANFManager manager, SettingsTab tab)
        {
            this.tab = tab;
            this.menu = menu;

            tabNameText.SetNewKey(tab.GetName());
            tab.PopulateTab(manager, menu, root);
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        /// <summary>
        /// Redraws the tab's content
        /// </summary>
        /// <param name="manager">The ANF Manager</param>
        public void RedrawLocalizedElements(ANFManager manager)
        {
            tab.RedrawLocalizedEntries(manager, menu, root);
        }
    }

}
