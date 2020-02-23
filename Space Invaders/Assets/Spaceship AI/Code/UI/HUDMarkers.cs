using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HUDMarkers : Singleton<HUDMarkers> {

    // Current selected target offscreen indicator
    public GameObject OffscreenIndicatorPrefab;
    // For the marker pool
    public GameObject NonselectedIndicatorPrefab;

    private GameObject offscreenIndicator;
    private Image currTargetOffscreenMarker;

    public Transform Target
    {
        get { return currentTarget; }
        set { currentTarget = value; }
    }
    private Transform currentTarget;
    private Image currentTargetMarker;

    private Image[] markerPool;
    private int markerPoolSize = 30;
    
    private float hScreenWidth, hScreenHeight;
    private Dictionary<int, GameObject> markerObjectMap;

    private void Start()
    {
        markerObjectMap = new Dictionary<int, GameObject>();
        hScreenHeight = Screen.height / 2;
        hScreenWidth = Screen.width / 2;
        currentTargetMarker = GetComponentInChildren<Image>();

        markerPool = new Image[markerPoolSize];

        // Initialize marker pool
        for (int i = 0; i < markerPoolSize; i++)
        {
            GameObject indicator = GameObject.Instantiate(NonselectedIndicatorPrefab, this.transform);
            markerPool[i] = indicator.GetComponent<Image>();
            markerPool[i].enabled = false;

            // Initialize the marker - gameobject map
            markerObjectMap.Add(i, null);
        }

        offscreenIndicator = GameObject.Instantiate(OffscreenIndicatorPrefab, transform);
        currTargetOffscreenMarker = offscreenIndicator.GetComponent<Image>();
        offscreenIndicator.SetActive(false);
    }
    
    void Update () {
        // Handle current target on and off-screen markers
		if(currentTarget != null && currentTarget.gameObject.activeInHierarchy)
        {
            DisplayMarker(currentTarget);            
        }
        else
        {
            currentTargetMarker.enabled = false;
            offscreenIndicator.SetActive(false);
        }

        GameObject[] objectsInRange = GameObject.FindGameObjectsWithTag("Ship");

        // Pass all objects
        for (int i = 0; i < objectsInRange.Length; i++)
        {
            GameObject obj = objectsInRange[i];
            // Check if obj is already attached to a marker
            bool alreadyUsed = false;
            foreach(var markerObj in markerObjectMap.Values)
                if(obj == markerObj)
                {
                    alreadyUsed = true;
                    break;
                }

            if (alreadyUsed)
                continue;

            if (IsObjectOnScreen(obj.transform) && obj.transform != currentTarget)
            {
                // Find first available HUD marker
                for (int j = 0; j < markerPoolSize; j++)
                {
                    if (markerObjectMap[j] == null)
                    {
                        // Assign marker to onscreen object
                        markerPool[j].enabled = true;
                        markerObjectMap[j] = obj;

                        markerPool[j].GetComponent<NonSelectedHUDMarker>().markerTarget = obj;
                        markerPool[j].rectTransform.localPosition = GetScreenPosOfObject(obj.transform);
                        break;
                    }
                }
            }
        }

        // Pass all markers, turn off unused ones
        for (int j = 0; j < markerPoolSize; j++)
        {
            if (markerObjectMap[j] != null)
            {
                GameObject obj = markerObjectMap[j];
                if (!IsObjectOnScreen(obj.transform) || obj.transform == currentTarget)
                {
                    // Turn off marker
                    markerPool[j].enabled = false;
                    markerObjectMap[j] = null;
                }
                else
                {
                    // Update marker position
                    markerPool[j].rectTransform.localPosition = GetScreenPosOfObject(obj.transform);
                }
            }
            else
            {
                // Turn off marker
                markerPool[j].enabled = false;
            }
        }


    }

    private void DisplayMarker(Transform target)
    {
        float x = Camera.main.WorldToScreenPoint(target.position).x - hScreenWidth;
        float y = Camera.main.WorldToScreenPoint(target.position).y - hScreenHeight;
        float z = Camera.main.WorldToScreenPoint(target.position).z;

        // Check if Target is off-screen            
        if (x < -hScreenWidth || x > hScreenWidth || y < -hScreenHeight || y > hScreenHeight)
        {
            // Target is off screen
            currentTargetMarker.enabled = false;

            if (!offscreenIndicator.activeInHierarchy)
            {
                offscreenIndicator.SetActive(true);
            }
            else
            {
                if(z>0)
                    currTargetOffscreenMarker.rectTransform.localPosition = new Vector3(
                        Mathf.Clamp(x, -hScreenWidth, hScreenWidth),
                        Mathf.Clamp(y, -hScreenHeight, hScreenHeight), 0f);
                else
                    currTargetOffscreenMarker.rectTransform.localPosition = new Vector3(
                        Mathf.Clamp(x, hScreenWidth, -hScreenWidth),
                        Mathf.Clamp(y, hScreenHeight, -hScreenHeight), 0f);
            }

        }
        else
        {
            if (z > 0)
            {
                // Target is on screen
                offscreenIndicator.SetActive(false);

                currentTargetMarker.enabled = true;
                currentTargetMarker.rectTransform.localPosition = new Vector3(x, y, 0f);
            }
        }
    }

    private bool IsObjectOnScreen(Transform obj)
    {
        float x = Camera.main.WorldToScreenPoint(obj.position).x;
        float y = Camera.main.WorldToScreenPoint(obj.position).y;
        float z = Camera.main.WorldToScreenPoint(obj.position).z;

        // Check if Target is off-screen            
        if (x < 0 || x > Screen.width || y < 0 || y > Screen.height)
        {
            return false;
        }
        else if (z > 0) // Target is in front of the camera
        {
            return true;
        }
        else // Target is behind the camera
        {
            return false;
        }

    }

    private Vector3 GetScreenPosOfObject(Transform target)
    {
        float x = Camera.main.WorldToScreenPoint(target.position).x - hScreenWidth;
        float y = Camera.main.WorldToScreenPoint(target.position).y - hScreenHeight;

        return new Vector3(
            Mathf.Clamp(x, -hScreenWidth, hScreenWidth),
            Mathf.Clamp(y, -hScreenHeight, hScreenHeight), 0f);
    }

    public void ClearTarget()
    {
        currentTarget = null;
        currentTargetMarker.enabled = false;
    }
}
