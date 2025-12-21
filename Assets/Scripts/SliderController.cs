
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public float scaleMinValue = 0.5f;
    public float scaleMaxValue = 3f;
    public float rotMinValue = 0f;
    public float rotMaxValue = 360f;

    private Slider scaleSlider;
    private Slider rotateSlider;

    private MQTTManager mqttManager;

    void Start()
    {
        scaleSlider = GameObject.Find("ScaleSlider").GetComponent<Slider>();
        rotateSlider = GameObject.Find("RotateSlider").GetComponent<Slider>();

        scaleSlider.minValue = scaleMinValue;
        scaleSlider.maxValue = scaleMaxValue;
        rotateSlider.minValue = rotMinValue;
        rotateSlider.maxValue = rotMaxValue;

        scaleSlider.onValueChanged.AddListener(ScaleSliderUpdate); //Quand le slider scaleSlider change de valeur, appelle la fonction ScaleSliderUpdate
        rotateSlider.onValueChanged.AddListener(RotateSliderUpdate);

        // Appliquer immédiatement les valeurs actuelles des sliders
        ScaleSliderUpdate(scaleSlider.value);
        RotateSliderUpdate(rotateSlider.value);

        mqttManager = FindObjectOfType<MQTTManager>();// Recherche le premier GameObject dans la scène qui contient un composant MQTTManager (game objet MQTT qui est ds la scene XR). Et le stocke dans la variable mqttManager.
        if (mqttManager != null)
        {
            mqttManager.RegisterNewSlider(this);
        }
    }

    public void ScaleSliderUpdate(float value)
    {
        transform.localScale = Vector3.one * value;

        // mettre à jour MQTTManager avec la nouvelle valeur locale
        if (mqttManager != null)
        {
            mqttManager.UpdateLastCommandFromLocal(value, rotateSlider.value);
        }
    }

    public void RotateSliderUpdate(float value)
    {
        transform.localEulerAngles = new Vector3(0, value, 0);

        // mettre à jour MQTTManager avec la nouvelle valeur locale
        if (mqttManager != null)
        {
            mqttManager.UpdateLastCommandFromLocal(scaleSlider.value, value);
        }
    }

    public void ApplyRemoteCommand(MQTTManager.ScaleRotateCommand command)
    {
        scaleSlider.value = command.scale;
        ScaleSliderUpdate(command.scale);

        rotateSlider.value = command.rot;
        RotateSliderUpdate(command.rot);
    }
}