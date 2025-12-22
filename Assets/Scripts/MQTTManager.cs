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

    private float connectionCheckTimer = 0f;
    private float connectionCheckInterval = 5f; // Vérifier toutes les 5 secondes

    private void Start()
    {
        // VÉRIFICATION CRITIQUE : Le Dispatcher doit exister pour que MQTT puisse parler à Unity
        if (!UnityMainThreadDispatcher.Exists())
        {
            Debug.Log("Création automatique du UnityMainThreadDispatcher...");
            GameObject dispatcherObj = new GameObject("MainThreadDispatcher");
            dispatcherObj.AddComponent<UnityMainThreadDispatcher>();
        }

        AttemptConnection();
    }

    private void Update()
    {
        // Système de reconnexion automatique
        connectionCheckTimer += Time.deltaTime;
        if (connectionCheckTimer >= connectionCheckInterval)
        {
            connectionCheckTimer = 0f;
            if (client == null || !client.IsConnected)
            {
                Debug.LogWarning("MQTT déconnecté. Tentative de reconnexion...");
                AttemptConnection();
            }
        }

        // HEARTBEAT LOG
        heartbeatTimer += Time.deltaTime;
        if (heartbeatTimer >= 5.0f)
        {
            heartbeatTimer = 0f;
            bool isConnected = (client != null && client.IsConnected);
            Debug.Log($"[MQTT HEARTBEAT] Connected? {isConnected} | Time: {Time.time}");
        }
    }
    private float heartbeatTimer = 0f;

    private void AttemptConnection()
    {
        try
        {
            if (client != null && client.IsConnected) return;

            // Création du client si nécessaire (ou recréation si l'ancien est corrompu)
            if (client == null)
            {
                client = new MqttClient(mqttBroker, mqttPort, true, null, null, MqttSslProtocols.TLSv1_2);
                client.MqttMsgPublishReceived += OnMqttMessageReceived;
            }

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId, username, password);
            
            if (client.IsConnected)
            {
                Debug.Log("MQTT connecté avec succès !");
                client.Publish("FABLAB_21_22/unity/test/out", Encoding.UTF8.GetBytes("Reconnexion Unity OK"), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
                client.Subscribe(new string[] { mqttTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Échec de la connexion MQTT : " + e.Message);
        }
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
                Debug.Log($"[MQTT DEBUG] Parsed -> Scale:{command.scale}, Rot:{command.rot}, Temp:{command.temperature}");

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    // 1. PILOTAGE DES SLIDERS
                    var sSlider = GameObject.Find("ScaleSlider")?.GetComponent<Slider>();
                    var rSlider = GameObject.Find("RotateSlider")?.GetComponent<Slider>();

                    if (sSlider != null) 
                    {
                        sSlider.value = command.scale;
                        sSlider.onValueChanged.Invoke(command.scale);
                    }
                    
                    if (rSlider != null) 
                    {
                        rSlider.value = command.rot;
                        rSlider.onValueChanged.Invoke(command.rot);
                    }

                    // Application immédiate de la température
                    ApplyTemperatureToScene(command.temperature);
                });
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Erreur parsing MQTT : " + ex.Message);
        }
    }

    // Méthode réutilisable pour tourner l'aiguille
    private void ApplyTemperatureToScene(float temperature)
    {
        var allTransforms = FindObjectsOfType<Transform>();
        foreach (var t in allTransforms)
        {
            if (t.name.IndexOf("Pointer", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                float angle = -temperature * 180f / 30f;
                
                // Rotation Y forcée (validée)
                t.localEulerAngles = new Vector3(0f, angle, 0f);
                
                Debug.Log($"[MQTT DIRECT] Rotation appliquée : {angle} sur {t.name}");
            }
        }
    }

    // Appelé par DynamicTrackedImageHandler quand un objet apparait
    public void ForceUpdateScene()
    {
        if (lastCommand != null)
        {
            Debug.Log("[MQTT] ForceUpdateScene demandé (Réapparition objet)");
            // On le fait dans le thread principal pour être sûr
            UnityMainThreadDispatcher.Instance().Enqueue(() => 
            {
               ApplyTemperatureToScene(lastCommand.temperature);
            });
        }
    }

    public void RegisterNewSlider(SliderController controller)
    {
        if (lastCommand != null)
        {
            controller.ApplyRemoteCommand(lastCommand);
            // Force re-trigger pour appliquer la température directe
            try {
                OnMqttMessageReceived(null, new MqttMsgPublishEventArgs(mqttTopic, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lastCommand)), false, 0, false));
            } catch {}
        }
    }


    public void UpdateLastCommandFromLocal(float scale, float rot)
    {
        lastCommand = new ScaleRotateCommand { scale = scale, rot = rot };
    }

    [Serializable]
    public class ScaleRotateCommand
    {
        [JsonProperty("scale")]
        public float scale;

        [JsonProperty("rot")]
        public float rot;

        [JsonProperty("temperature")]
        public float temperature;
    }

    //public class ReceiveData
    //{
    // public float temperature;
    //}
}
