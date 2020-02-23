using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 

/// <summary>
/// This class handles clicks on UI markers (ships) and sets them as selected
/// targets.
/// </summary>
public class NonSelectedHUDMarker : MonoBehaviour, IPointerClickHandler
{
    public GameObject markerTarget;

    public void OnPointerClick(PointerEventData eventData)
    {
        HUDMarkers.Instance.Target = markerTarget.transform;
    }
}
