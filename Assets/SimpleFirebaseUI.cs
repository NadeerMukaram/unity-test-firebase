using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SimpleFirebaseUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button submitButton;

    [Header("Message Display")]
    [SerializeField] private Transform messageContainer; // Parent object for messages
    [SerializeField] private MessageItem messagePrefab;  // Reference to the MessageItem prefab
    [SerializeField] private ScrollRect scrollRect;      // Reference to scroll view
    
    private FirebaseManager firebaseManager;
    private List<MessageItem> messageItems = new List<MessageItem>();

    async void Start()
    {
        firebaseManager = FindObjectOfType<FirebaseManager>();
        if (firebaseManager == null)
        {
            Debug.LogError("FirebaseManager not found in scene!");
            return;
        }

        submitButton.onClick.AddListener(SubmitMessage);
        
        // Wait for Firebase to initialize
        await System.Threading.Tasks.Task.Delay(2000);
        
        // Start listening for real-time updates
        firebaseManager.ListenForDataChange("messages", OnMessageUpdate);
        
        // Load initial messages
        await LoadMessages();
    }

    private async Task LoadMessages()
    {
        string jsonData = await firebaseManager.ReadData("messages");
        if (jsonData != null)
        {
            UpdateDisplayText(jsonData);
        }
    }

    private async void SubmitMessage()
    {
        if (string.IsNullOrEmpty(messageInput.text)) return;

        try
        {
            // Create a message object matching your document structure
            var message = new Dictionary<string, object>
            {
                {"msgText", messageInput.text},
                {"msgTimeStamp", DateTime.Now.ToString("MMMM d, yyyy 'at' h:mm:ss tt UTC+8")}
            };

            // Use Firebase's Push() to generate a unique key
            string messageKey = firebaseManager.databaseReference.Child("messages").Push().Key;
            
            // Add the message to the database
            await firebaseManager.databaseReference.Child("messages").Child(messageKey).SetValueAsync(message);
            
            Debug.Log($"Message added successfully with key: {messageKey}");
            
            // Clear input field
            messageInput.text = "";
        }
        catch (Exception e)
        {
            Debug.LogError($"Error adding message: {e.Message}");
        }
    }

    private void UpdateDisplayText(string jsonData)
    {
        try
        {
            // Clear existing messages
            foreach (var item in messageItems)
            {
                Destroy(item.gameObject);
            }
            messageItems.Clear();

            // Parse JSON using Newtonsoft.Json
            JObject messages = JObject.Parse(jsonData);
            
            foreach (var message in messages)
            {
                var messageData = message.Value;
                string text = messageData["msgText"].ToString();
                string time = messageData["msgTimeStamp"].ToString();

                // Create new message instance
                MessageItem newMessage = Instantiate(messagePrefab, messageContainer);
                newMessage.SetMessageData(text, time);
                messageItems.Add(newMessage);
            }

            // Scroll to bottom
            Canvas.ForceUpdateCanvases();
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing JSON: {e.Message}\nJSON data: {jsonData}");
        }
    }

    private void OnMessageUpdate(string jsonData)
    {
        if (!string.IsNullOrEmpty(jsonData))
        {
            UpdateDisplayText(jsonData);
        }
    }

    private string FormatJson(string json)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int indentLevel = 0;
        bool inQuotes = false;

        foreach (char c in json)
        {
            switch (c)
            {
                case '{':
                case '[':
                    sb.Append(c);
                    sb.AppendLine();
                    indentLevel++;
                    sb.Append(new string(' ', indentLevel * 2));
                    break;
                case '}':
                case ']':
                    sb.AppendLine();
                    indentLevel--;
                    sb.Append(new string(' ', indentLevel * 2));
                    sb.Append(c);
                    break;
                case ',':
                    sb.Append(c);
                    if (!inQuotes)
                    {
                        sb.AppendLine();
                        sb.Append(new string(' ', indentLevel * 2));
                    }
                    break;
                case '"':
                    sb.Append(c);
                    inQuotes = !inQuotes;
                    break;
                case ':':
                    sb.Append(c);
                    if (!inQuotes)
                        sb.Append(" ");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }

    [Serializable]
    private class MessageData
    {
        public string msgText;
        public string msgTimeStamp;
    }
} 