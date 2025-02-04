using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MessageItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI timestampText;
    [SerializeField] private Image backgroundPanel;

    [Header("Style")]
    [SerializeField] private Color evenMessageColor = new Color(0.95f, 0.95f, 0.95f);
    [SerializeField] private Color oddMessageColor = new Color(1f, 1f, 1f);

    public void SetMessageData(string text, string timestamp, bool isEvenMessage = true)
    {
        messageText.text = text;
        timestampText.text = timestamp;

        // Set background color
        if (backgroundPanel != null)
        {
            backgroundPanel.color = isEvenMessage ? evenMessageColor : oddMessageColor;
        }
    }
} 