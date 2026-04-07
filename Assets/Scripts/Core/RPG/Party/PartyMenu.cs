using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the party menu
/// </summary>
public class PartyMenu : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GameObject root;

    [Header("Tabs")]
    [SerializeField] private GameObject defaultPage;
    [SerializeField] private PartyMenuStatusTab statusTab;
    [SerializeField] private PartyMenuEquipTab equipTab;
    [SerializeField] private PartyMenuItemsTab itemsTab;



    public bool open
    {
        get { return root.activeInHierarchy; }
    }

    /// <summary>
    /// Opens the party menu
    /// </summary>
    public void Open()
    {
        root.SetActive(true);
        OpenDefault();
    }

    /// <summary>
    /// Closes the party menu
    /// </summary>
    public void Close()
    {
        root.SetActive(false);
    }

    /// <summary>
    /// Opens the default page
    /// </summary>
    public void OpenDefault()
    {
        defaultPage.SetActive(true);
        statusTab.Close();
        equipTab.Close();
        itemsTab.Close();
    }

    /// <summary>
    /// Opens the status page
    /// </summary>
    public void OpenStatus()
    {
        defaultPage.SetActive(false);
        statusTab.Open();
    }

    /// <summary>
    /// Opens the equip page
    /// </summary>
    public void OpenEquip()
    {
        defaultPage.SetActive(false);
        equipTab.Open();
    }

    /// <summary>
    /// Opens the items page
    /// </summary>
    public void OpenItems()
    {
        defaultPage.SetActive(false);
        itemsTab.Open();
    }
}
