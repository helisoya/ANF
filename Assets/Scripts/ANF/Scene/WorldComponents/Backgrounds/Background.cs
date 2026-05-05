using System.Collections.Generic;
using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
    /// Represents an individual background
    /// </summary>
    public class Background : MonoBehaviour
    {
        [Header("Infos")]
        [SerializeField] private Transform markersRoot;
        private Dictionary<string, Transform> markers;

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
        public Vector3 GetMarkerRotation(string marker)
        {
            if (markers.ContainsKey(marker))
            {
                return markers[marker].eulerAngles;
            }
            return Vector3.zero;
        }

        public void OnLoad()
        {
            RegisterMarkers();
        }


        public void OnUnLoad()
        {

        }
    }
}