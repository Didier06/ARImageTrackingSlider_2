using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TestSimple : MonoBehaviour
{
    public ARTrackedImageManager imageManager;

    void Start()
    {
        Debug.Log("TEST 1 - Start");
        Debug.Log("TEST 2 - Start");
        Debug.Log("TEST 3 - Start");
        
        if (imageManager == null)
        {
            Debug.LogError("TEST: imageManager NULL");
        }
        else
        {
            Debug.Log("TEST: imageManager OK");
            imageManager.trackablesChanged.AddListener(OnImageChanged);
            Debug.Log("TEST: Listener OK");
        }
    }

    void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        Debug.Log("TEST: IMAGE DETECTEE ! Count: " + args.added.Count);
    }
}
