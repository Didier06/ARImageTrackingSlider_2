using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public class MqttTest : MonoBehaviour
{
    private MqttClient client;

    // === À personnaliser selon votre configuration ===
    private string broker = "mqtt.univ-cotedazur.fr";
    private int port = 8443; // Connexion sécurisée (SSL)
    private string username = "fablab2122";
    private string password = "2122";
    private string topic = "FABLAB_21_22/unity/test/out";
    private string message = "Bienvenue sur MQTT depuis Unity !";

    void Start()
    {
        // Connexion sécurisée TLSv1.2 avec validation simple
        client = new MqttClient(
            broker, port, true,
            null, null,
            MqttSslProtocols.TLSv1_2,
            (sender, cert, chain, sslPolicyErrors) => true
        );

        string clientId = System.Guid.NewGuid().ToString();

        try
        {
            client.Connect(clientId, username, password);

            if (client.IsConnected)
            {
                Debug.Log("✅ Connecté au broker MQTT !");
                client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
                Debug.Log("📤 Message envoyé : " + message);
            }
            else
            {
                Debug.LogError("❌ Connexion MQTT échouée.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❗ Erreur de connexion MQTT : " + ex.Message);
        }
    }
}