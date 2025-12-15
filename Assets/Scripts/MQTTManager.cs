using System;
using System.Text;
using UnityEngine;
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
                    SliderController[] controllers = FindObjectsOfType<SliderController>();
                    foreach (var ctrl in controllers)
                    {
                        ctrl.ApplyRemoteCommand(command);// mise à jour des slider quand une commande mqtt est reçue
                    }

                    DynamicTrackedImageHandler handler = FindObjectOfType<DynamicTrackedImageHandler>();
                    // On passe null pour récupérer n'importe quel prefab actif (celui qu'on regarde actuellment)
                    GameObject instance = handler.GetSpawnedInstance(null);


                    //DynamicTrackedImageHandler handler = FindObjectOfType<DynamicTrackedImageHandler>();// quel est l(objet selectionné ?
                    //GameObject selected = handler.GetCurrentPrefab(); // on le récupère avec la méthode de DynamicTrackedIomageHandler

                    Transform pointerTransform = instance.transform.Find("contener_gauge_temp/MainBody/Pointer");// l'aiguille (pointer) existe-elle dans l'objet 3D , si oui c'est la jauge
                    //Transform pointerTransform = selected.GetComponentInChildren<Transform>(true).FirstOrDefault(t => t.name == "Pointer");
                    if (pointerTransform != null)
                    {
                        Debug.Log("element 'Pointer' existe !");
                        //pointerTransform.rotation = Quaternion.Euler(50f, 100f, -90f);

                        // Test : rotation absolue (remplace complètement la rotation)

                        pointerTransform.localEulerAngles = new Vector3(0f, -lastCommand.temperature * 180 / 30f, 0f);


                        // Affichage pour vérifier que la rotation est appliquée
                        Debug.Log("Rotation appliquée à Pointer : " + pointerTransform.localEulerAngles);
                        //pointerTransform.transform.Rotate(50f, 100f, 41f);
                    }
                    else
                    {
                        Debug.Log("L'élément 'Pointer' est introuvable.");

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
