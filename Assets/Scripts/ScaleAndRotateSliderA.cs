using UnityEngine;
using UnityEngine.UI;

public class ScaleAndRotateSliderA : MonoBehaviour
{
    [Header("Sliders (assign automatically by name)")]
    private Slider scaleSlider;
    private Slider rotateSlider;

    [Header("Slider Settings")]
    public float scaleMinValue = 0.6f;
    public float scaleMaxValue = 6f;
    public float rotMinValue = 0f;
    public float rotMaxValue = 360f;

    [Header("Target Object (to transform)")]
    public Transform targetObject;

    void Start()
    {
        // Safety: Ensure target is set
        if (targetObject == null)
        {
            Debug.LogWarning("Target object not assigned! Using this.transform by default.");
            targetObject = this.transform;
        }

        // Find sliders by name (ensure they exist in the scene)
        scaleSlider = GameObject.Find("ScaleSlider")?.GetComponent<Slider>();
        rotateSlider = GameObject.Find("RotateSlider")?.GetComponent<Slider>();

        if (scaleSlider == null || rotateSlider == null)
        {
            Debug.LogError("Sliders not found! Make sure 'ScaleSlider' and 'RotateSlider' exist in the scene.");
            return;
        }

        // Configure scale slider
        scaleSlider.minValue = scaleMinValue;
        scaleSlider.maxValue = scaleMaxValue;
        scaleSlider.value = (scaleMinValue + scaleMaxValue) / 2f;
        scaleSlider.onValueChanged.AddListener(OnScaleChanged);

        // Configure rotation slider
        rotateSlider.minValue = rotMinValue;
        rotateSlider.maxValue = rotMaxValue;
        rotateSlider.value = rotMinValue;
        rotateSlider.onValueChanged.AddListener(OnRotationChanged);
    }

    void OnScaleChanged(float value)
    {
        if (targetObject != null)
            targetObject.localScale = Vector3.one * value;
    }

    void OnRotationChanged(float value)
    {
        if (targetObject != null)
        {
            Vector3 currentRotation = targetObject.localEulerAngles;
            targetObject.localEulerAngles = new Vector3(currentRotation.x, value, currentRotation.z);
        }
    }
}

