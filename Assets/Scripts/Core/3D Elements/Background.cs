using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    [Header("Infos")]
    [SerializeField] public string backgroundName;
    [SerializeField] private Transform markersRoot;
    [SerializeField] private SkyData skyData;
    [SerializeField] private Vector3 directionalLightRotation = new Vector3(50, -30, 0);
    [SerializeField] private Transform rotatablesRoot;
    private Dictionary<string, Transform> markers;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, Quaternion.Euler(directionalLightRotation) * transform.forward * 5);
    }


    /// <summary>
    /// Initialize the background
    /// </summary>
    /// <param name="initializeSkyData">Should the skybox be changed ?</param>
    public void Init(bool initializeSkyData = true)
    {
        RegisterMarkers();
        if (initializeSkyData) LightingManager.instance.ChangeData(skyData);
        ApplyLightRotation();
    }

    /// <summary>
    /// Applies the light rotation of this background
    /// </summary>
    public void ApplyLightRotation()
    {
        LightingManager.instance.SetDirectionalLightRotation(directionalLightRotation);
    }

    /// <summary>
    /// Returns the background's name
    /// </summary>
    /// <returns>The background's name</returns>
    public string GetBackgroundName()
    {
        return backgroundName;
    }

    /// <summary>
	/// Checks if the marker exists
	/// </summary>
	/// <param name="marker">The marker's name</param>
	/// <returns>True if the marker exists</returns>
    public bool MarkerExists(string marker)
    {
        return markers.ContainsKey(marker);
    }

    /// <summary>
    /// Registers the background's makers
    /// </summary>
    private void RegisterMarkers()
    {
        markers = new Dictionary<string, Transform>();

        foreach (Transform child in markersRoot)
        {
            markers[child.name] = child;
        }
    }

    /// <summary>
    /// Finds a marker's position
    /// </summary>
    /// <param name="marker">The marker's name</param>
    /// <returns>The marker's position</returns>
    public Vector3 GetMarkerPosition(string marker)
    {
        if (markers.ContainsKey(marker))
        {
            return markers[marker].position;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Finds a marker's rotation
    /// </summary>
    /// <param name="marker">The marker's name</param>
    /// <returns>The marker's rotation</returns>
    public float GetMarkerRotation(string marker)
    {
        if (markers.ContainsKey(marker))
        {
            return markers[marker].eulerAngles.y;
        }
        return 0;
    }

    /// <summary>
    /// Unregisters the background's interactables
    /// </summary>
    public void UnregisterInteractables()
    {
        InteractableObject[] interactables = GetComponentsInChildren<InteractableObject>();
        foreach (InteractableObject interactable in interactables)
        {
            interactable.Unregister();
        }
    }

    /// <summary>
    /// Rotates the rotatable items
    /// </summary>
    public void RotateItems()
    {
        if (!rotatablesRoot) return;

        foreach (Transform child in rotatablesRoot)
        {
            child.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }
    }
}
