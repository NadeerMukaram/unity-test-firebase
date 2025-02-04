using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    public DatabaseReference databaseReference;
    private bool isInitialized = false;
    private const string DATABASE_URL = "https://crud-firebase-431b3-default-rtdb.firebaseio.com/";
    private FirebaseApp app;

    async void Start()
    {
        await InitializeFirebase();
    }

    private async Task InitializeFirebase()
    {
        try
        {
            // First, check and fix dependencies
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                return;
            }

            // Configure Firebase
            if (FirebaseApp.DefaultInstance == null)
            {
                var options = new AppOptions
                {
                    DatabaseUrl = new Uri(DATABASE_URL)
                };
                app = FirebaseApp.Create(options);
            }
            else
            {
                app = FirebaseApp.DefaultInstance;
                app.Options.DatabaseUrl = new Uri(DATABASE_URL);
            }

            // Initialize database
            databaseReference = FirebaseDatabase.GetInstance(app).RootReference;
            isInitialized = true;
            Debug.Log("Firebase initialized successfully!");

            // Add initial data
            await AddInitialData();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firebase initialization error: {ex.Message}");
        }
    }

    private async Task AddInitialData()
    {
        try
        {
            // Check if data exists
            var snapshot = await databaseReference.Child("messages").GetValueAsync();
            if (!snapshot.Exists)
            {
                // Create sample messages
                var sampleMessages = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        {"msgText", "Hello! Welcome to our chat app!"},
                        {"msgTimeStamp", DateTime.Now.ToString("MMMM d, yyyy 'at' h:mm:ss tt UTC+8")}
                    },
                    new Dictionary<string, object>
                    {
                        {"msgText", "This is a sample message"},
                        {"msgTimeStamp", DateTime.Now.AddMinutes(1).ToString("MMMM d, yyyy 'at' h:mm:ss tt UTC+8")}
                    },
                    new Dictionary<string, object>
                    {
                        {"msgText", "Feel free to add your own messages!"},
                        {"msgTimeStamp", DateTime.Now.AddMinutes(2).ToString("MMMM d, yyyy 'at' h:mm:ss tt UTC+8")}
                    }
                };

                // Add each message with an auto-generated key
                foreach (var message in sampleMessages)
                {
                    string messageKey = databaseReference.Child("messages").Push().Key;
                    await databaseReference.Child($"messages/{messageKey}").SetValueAsync(message);
                }
                
                Debug.Log("Sample messages added successfully!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error adding sample data: {e.Message}");
        }
    }

    // Helper method to check initialization
    private bool CheckInitialization()
    {
        if (!isInitialized)
        {
            Debug.LogError("Firebase is not initialized yet!");
            return false;
        }
        return true;
    }

    // Create operation
    public async Task CreateData(string path, object data)
    {
        if (!CheckInitialization()) return;
        try
        {
            string jsonData = JsonUtility.ToJson(data);
            await databaseReference.Child(path).SetRawJsonValueAsync(jsonData);
            Debug.Log($"Data created successfully at {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating data: {e.Message}");
        }
    }

    // Read operation
    public async Task<string> ReadData(string path)
    {
        if (!CheckInitialization()) return null;
        try
        {
            var snapshot = await databaseReference.Child(path).GetValueAsync();
            if (snapshot.Exists)
            {
                return snapshot.GetRawJsonValue();
            }
            else
            {
                Debug.Log($"No data exists at {path}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading data: {e.Message}");
            return null;
        }
    }

    // Update operation
    public async Task UpdateData(string path, Dictionary<string, object> updates)
    {
        if (!CheckInitialization()) return;
        try
        {
            await databaseReference.Child(path).UpdateChildrenAsync(updates);
            Debug.Log($"Data updated successfully at {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating data: {e.Message}");
        }
    }

    // Delete operation
    public async Task DeleteData(string path)
    {
        if (!CheckInitialization()) return;
        try
        {
            await databaseReference.Child(path).RemoveValueAsync();
            Debug.Log($"Data deleted successfully at {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting data: {e.Message}");
        }
    }

    // Listen for real-time updates
    public void ListenForDataChange(string path, Action<string> callback)
    {
        if (!CheckInitialization()) return;
        databaseReference.Child(path).ValueChanged += (object sender, ValueChangedEventArgs args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError($"Error listening to data: {args.DatabaseError.Message}");
                return;
            }

            if (args.Snapshot != null && args.Snapshot.Exists)
            {
                callback(args.Snapshot.GetRawJsonValue());
            }
        };
    }
}