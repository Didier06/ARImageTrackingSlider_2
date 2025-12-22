
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public float scaleMinValue = 0.5f;
    public float scaleMaxValue = 3f;
    public float rotMinValue = 0f;
    public float rotMaxValue = 360f;

    public enum Axis { X, Y, Z }
    public Axis pointerRotationAxis = Axis.X; // X est le bon axe pour le modèle Blender actuel

    private Slider scaleSlider;
    private Slider rotateSlider;

    private MQTTManager mqttManager;

    void Start()
    {
        // Recherche sécurisée des sliders (ils peuvent être absents)
        var scaleObj = GameObject.Find("ScaleSlider");
        if (scaleObj != null) 
        {
            scaleSlider = scaleObj.GetComponent<Slider>();
            if (scaleSlider != null)
            {
                scaleSlider.minValue = scaleMinValue;
                scaleSlider.maxValue = scaleMaxValue;
                scaleSlider.onValueChanged.AddListener(ScaleSliderUpdate);
                ScaleSliderUpdate(scaleSlider.value);
            }
        }

        var rotateObj = GameObject.Find("RotateSlider");
        if (rotateObj != null)
        {
            rotateSlider = rotateObj.GetComponent<Slider>();
            if (rotateSlider != null)
            {
                rotateSlider.minValue = rotMinValue;
                rotateSlider.maxValue = rotMaxValue;
                rotateSlider.onValueChanged.AddListener(RotateSliderUpdate);
                RotateSliderUpdate(rotateSlider.value);
            }
        }

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

    // Modifié : Plus de gestion locale de la température/pointer ici.
    // C'est géré directement par MQTTManager pour éviter les conflits.

    public void ApplyRemoteCommand(MQTTManager.ScaleRotateCommand command)
    {
        Debug.Log($"[SliderController] Reçu commande Scale/Rot");

        // 1. Scale et Rotation (Classique, via les sliders)
        if (scaleSlider != null)
        {
            scaleSlider.value = command.scale;
            ScaleSliderUpdate(command.scale);
        }
        else
        {
             transform.localScale = Vector3.one * command.scale;
        }

        if (rotateSlider != null)
        {
            rotateSlider.value = command.rot;
            RotateSliderUpdate(command.rot);
        }
        else
        {
             transform.localEulerAngles = new Vector3(0, command.rot, 0);
        }
    }
}