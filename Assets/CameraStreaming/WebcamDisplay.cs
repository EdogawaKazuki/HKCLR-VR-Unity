using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WebcamDisplay : MonoBehaviour
{
    public RawImage rawImage;
    public Material material;
    public TMP_Dropdown cameraDropdown;

    private WebCamTexture webcamTexture;
    private List<WebCamDevice> devices;
    [SerializeField]
    private int initCameraIndex = 0;
    void Start()
    {
        // Find the RawImage and Dropdown components
        // rawImage = GetComponent<RawImage>();
        // cameraDropdown = GetComponentInChildren<TMP_Dropdown>();

        // Initialize the devices list
        devices = new List<WebCamDevice>(WebCamTexture.devices);

        // Populate the dropdown with camera names
        PopulateDropdown();

        // Set the dropdown listener
        cameraDropdown.onValueChanged.AddListener(delegate {
            SelectCamera(cameraDropdown.value);
        });

        // Start the first camera by default
        if (devices.Count > 1)
        {
            SelectCamera(initCameraIndex);
        }
    }

    void PopulateDropdown()
    {
        List<string> options = new List<string>();
        foreach (var device in devices)
        {
            options.Add(device.name);
        }

        cameraDropdown.ClearOptions();
        cameraDropdown.AddOptions(options);
    }

    void SelectCamera(int index)
    {
        // Stop the current webcam if it's running
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
        // Initialize the new WebCamTexture with the selected device
        webcamTexture = new WebCamTexture(devices[index].name);
        Debug.Log(webcamTexture);
        Debug.Log(rawImage);

        // Assign the WebCamTexture to the RawImage's texture
        if(rawImage != null)
        {
            rawImage.texture = webcamTexture;
        }
        if (material != null)
        {
            material.mainTexture = webcamTexture;
        }

        // Start the webcam feed
        webcamTexture.Play();
    }

    void OnDestroy()
    {
        // Stop the webcam feed when the script is destroyed
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
    }
}