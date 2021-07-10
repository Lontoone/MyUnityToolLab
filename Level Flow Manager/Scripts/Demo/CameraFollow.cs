using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    public GameObject Player;
    Camera camera;
    private void Awake()
    {
        camera = GetComponent<Camera>();
        SceneManager.sceneLoaded += CloseAllSubCamera;
    }
    public void OnDestroy()
    {
        SceneManager.sceneLoaded -= CloseAllSubCamera;
    }
    public void Update()
    {
        transform.position = new Vector3(Player.transform.position.x,
                                        Player.transform.position.y,
                                        -10);
    }

    private void CloseAllSubCamera(Scene _scene, LoadSceneMode loadSceneMode)
    {
        foreach (Camera _camera in FindObjectsOfType<Camera>())
        {
            Debug.Log(_camera.gameObject.name);
            if (_camera != camera)
            {
                _camera.enabled = false;
            }
        }
    }
}
