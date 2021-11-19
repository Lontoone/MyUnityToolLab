using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class WebcamControl : MonoBehaviour
{
    public RawImage rawImage;
    public AspectRatioFitter fitter;
    public bool useFrontCamera = false;
    public UnityEvent onWebCamOpened;

    private WebCamTexture webCamTexture;

    private void Start()
    {        
        Screen.orientation = ScreenOrientation.Portrait;
        StartCoroutine(OpenCamera());        
    }
    public IEnumerator OpenCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            string frontCamName = null;
            var webCamDevices = WebCamTexture.devices;
            foreach (var camDevice in webCamDevices)
            {
                if (useFrontCamera && camDevice.isFrontFacing)
                {
                    frontCamName = camDevice.name;
                    break;
                }
                else
                {
                    frontCamName = camDevice.name;
                    break;
                }
            }

            webCamTexture = new WebCamTexture(frontCamName, Screen.width, Screen.height, 60);
            webCamTexture.Play();

            rawImage.texture = webCamTexture;

            onWebCamOpened?.Invoke();
        }
    }

    private void Update()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            float ratio = (float)webCamTexture.width / (float)webCamTexture.height;
            fitter.aspectRatio = ratio;

            float scaleY = webCamTexture.videoVerticallyMirrored ? -1f : 1;
            rawImage.rectTransform.localScale = new Vector3(1, scaleY, 1);

            int orient = -webCamTexture.videoRotationAngle;
            rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
        }
    }
}
