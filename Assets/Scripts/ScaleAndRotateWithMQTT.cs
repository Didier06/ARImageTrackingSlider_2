
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;

public class ScaleAndRotateWithMQTT : MonoBehaviour
{
    public float scaleMinValue = 0.5f;
    public float scaleMaxValue = 3f;
    public float rotMinValue = 0f;
    public float rotMaxValue = 360f;

    private Slider scaleSlider;
    private Slider rotateSlider;
    private MqttClient client;

    private string mqttBroker = "mqtt.univ-cotedazur.fr";
    private int mqttPort = 8443;
    private string mqttTopic = "FABLAB_21_22/unity/test/in";
    private string username = "fablab2122";
    private string password = "2122";

    void Start()
    {
        // Sliders
        scaleSlider = GameObject.Find("ScaleSlider").GetComponent<Slider>();
        rotateSlider = GameObject.Find("RotateSlider").GetComponent<Slider>();

        scaleSlider.minValue = scaleMinValue;
        scaleSlider.maxValue = scaleMaxValue;
        rotateSlider.minValue = rotMinValue;
        rotateSlider.maxValue = rotMaxValue;

        scaleSlider.onValueChanged.AddListener(ScaleSliderUpdate);
        rotateSlider.onValueChanged.AddListener(RotateSliderUpdate);

        // MQTT Setup
        client = new MqttClient(mqttBroker, mqttPort, true, null, null, MqttSslProtocols.TLSv1_2);
        Debug.Log("Connexion MQTT réussie");

        client.MqttMsgPublishReceived += OnMqttMessageReceived;
        string clientId = Guid.NewGuid().ToString();
        client.Connect(clientId, username, password);
        client.Publish("FABLAB_21_22/unity/test/out", Encoding.UTF8.GetBytes("Bienvenue de la part de Unity !"), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        client.Subscribe(new string[] { mqttTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
    }

    void ScaleSliderUpdate(float value)
    {
        transform.localScale = Vector3.one * value;
    }

    void RotateSliderUpdate(float value)
    {
        transform.localEulerAngles = new Vector3(0, value, 0);
    }

    void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string json = Encoding.UTF8.GetString(e.Message);
        Debug.Log("Message MQTT reçu : " + Encoding.UTF8.GetString(e.Message));
        try
        {
            var command = JsonConvert.DeserializeObject<ScaleRotateCommand>(json);
            if (command != null)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (command.scale >= scaleMinValue && command.scale <= scaleMaxValue)
                    {
                        scaleSlider.value = command.scale;
                    }

                    if (command.rot >= rotMinValue && command.rot <= rotMaxValue)
                    {
                        rotateSlider.value = command.rot;
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Erreur parsing MQTT: " + ex.Message);
        }
    }

    [Serializable]
    public class ScaleRotateCommand
    {
        public float scale;
        public float rot;
    }
}
