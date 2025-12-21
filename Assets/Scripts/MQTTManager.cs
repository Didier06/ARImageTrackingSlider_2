using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;

public class MQTTManager : MonoBehaviour
{
    private MqttClient client;
    private string mqttBroker = "mqtt.univ-cotedazur.fr";
    private int mqttPort = 8443;
    private string mqttTopic = "FABLAB_21_22/unity/test/in";
    private string username = "fablab2122";
    private string password = "2122";

    private ScaleRotateCommand lastCommand = null;
    //private ReceiveData receiveData= null;

    private void Start()
    {
        // VÉRIFICATION CRITIQUE : Le Dispatcher doit exister pour que MQTT puisse parler à Unity
        if (!UnityMainThreadDispatcher.Exists())
        {
            Debug.Log("Création automatique du UnityMainThreadDispatcher...");
            GameObject dispatcherObj = new GameObject("MainThreadDispatcher");
            dispatcherObj.AddComponent<UnityMainThreadDispatcher>();
        }

        client = new MqttClient(mqttBroker, mqttPort, true, null, null, MqttSslProtocols.TLSv1_2);
        client.MqttMsgPublishReceived += OnMqttMessageReceived;
        string clientId = Guid.NewGuid().ToString();
        client.Connect(clientId, username, password);
        client.Publish("FABLAB_21_22/unity/test/out", Encoding.UTF8.GetBytes("Bienvenue de la part de Unity !"), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        client.Subscribe(new string[] { mqttTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

        Debug.Log("MQTT connecté.");
    }

    private void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string json = Encoding.UTF8.GetString(e.Message);
        Debug.Log("MQTT reçu : " + json);

        try
        {
            var command = JsonConvert.DeserializeObject<ScaleRotateCommand>(json);
            if (command != null)
            {
                lastCommand = command;

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    // 1. PILOTAGE DES SLIDERS (Comme le script est sur un enfant, c'est le moyen le plus sûr)
                    var sSlider = GameObject.Find("ScaleSlider")?.GetComponent<Slider>();
                    var rSlider = GameObject.Find("RotateSlider")?.GetComponent<Slider>();

                    if (sSlider != null) 
                    {
                        sSlider.value = command.scale;
                        sSlider.onValueChanged.Invoke(command.scale); // DÉCLENCHE SliderController sur l'enfant
                    }
                    
                    if (rSlider != null) 
                    {
                        rSlider.value = command.rot;
                        rSlider.onValueChanged.Invoke(command.rot); // DÉCLENCHE SliderController sur l'enfant
                    }

                    // 2. TEMPERATURE (Cas particulier : on doit le faire manuellement car pas de slider associé)
                    DynamicTrackedImageHandler handler = FindObjectOfType<DynamicTrackedImageHandler>();
                    if (handler != null)
                    {
                        GameObject instance = handler.GetSpawnedInstance(null);
                        if (instance != null)
                        {
                            // Recherche robuste du pointeur, où qu'il soit dans la hiérarchie
                            Transform pointerTransform = null;
                            var allTransforms = instance.GetComponentsInChildren<Transform>(true);
                            foreach(var t in allTransforms) { if(t.name == "Pointer") { pointerTransform = t; break; } }

                            if (pointerTransform != null)
                            {
                                pointerTransform.localEulerAngles = new Vector3(0f, -lastCommand.temperature * 180 / 30f, 0f);
                                Debug.Log($"[MQTT] Temperature appliquée : {lastCommand.temperature}");
                            }
                        }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Erreur parsing MQTT : " + ex.Message);
        }
    }

    public void RegisterNewSlider(SliderController controller)
    {
        if (lastCommand != null)
        {
            controller.ApplyRemoteCommand(lastCommand);// mise à jour des sliders   > dans SliderController.js
        }
    }


    public void UpdateLastCommandFromLocal(float scale, float rot)
    {
        lastCommand = new ScaleRotateCommand { scale = scale, rot = rot };
    }


    [Serializable]
    public class ScaleRotateCommand
    {
        public float scale;
        public float rot;
        public float temperature; //  ajout de la clé temperature
    }

    //public class ReceiveData
    //{
    // public float temperature;
    //}
}
