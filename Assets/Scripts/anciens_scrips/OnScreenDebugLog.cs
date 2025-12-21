using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Affiche les logs Unity sur l'écran Android en grand pour faciliter le débogage
/// </summary>
public class OnScreenDebugLog : MonoBehaviour
{
    public int fontSize = 24;
    public int maxLines = 30;
    
    private TextMeshProUGUI textDisplay;
    private string logText = "";
    private int lineCount = 0;

    void Awake()
    {
        // Créer un Canvas si nécessaire
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DebugCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Créer le GameObject pour le texte
        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(canvas.transform, false);

        // Ajouter le composant TextMeshProUGUI
        textDisplay = textObj.AddComponent<TextMeshProUGUI>();
        textDisplay.fontSize = fontSize;
        textDisplay.color = Color.white;
        textDisplay.alignment = TextAlignmentOptions.TopLeft;
        textDisplay.textWrappingMode = TextWrappingModes.Normal;

        // Configurer le RectTransform pour remplir l'écran
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = new Vector2(10, 10);
        rectTransform.offsetMax = new Vector2(-10, -10);

        // Ajouter un fond semi-transparent
        GameObject bgObj = new GameObject("DebugBackground");
        bgObj.transform.SetParent(canvas.transform, false);
        bgObj.transform.SetSiblingIndex(0); // Mettre en arrière-plan

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f); // Noir semi-transparent

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Ajouter une couleur selon le type de log
        string coloredLog = "";
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                coloredLog = $"<color=red>❌ {logString}</color>";
                break;
            case LogType.Warning:
                coloredLog = $"<color=yellow>⚠️ {logString}</color>";
                break;
            case LogType.Log:
                if (logString.Contains("✅"))
                    coloredLog = $"<color=green>{logString}</color>";
                else if (logString.Contains("❌"))
                    coloredLog = $"<color=red>{logString}</color>";
                else
                    coloredLog = $"<color=white>{logString}</color>";
                break;
        }

        // Ajouter la nouvelle ligne
        logText += coloredLog + "\n";
        lineCount++;

        // Limiter le nombre de lignes
        if (lineCount > maxLines)
        {
            int firstNewline = logText.IndexOf('\n');
            if (firstNewline >= 0)
            {
                logText = logText.Substring(firstNewline + 1);
                lineCount--;
            }
        }

        // Mettre à jour l'affichage
        if (textDisplay != null)
        {
            textDisplay.text = logText;
        }
    }
}
