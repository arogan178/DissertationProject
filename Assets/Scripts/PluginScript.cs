using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class pluginScript : MonoBehaviour
{
    public int[] vibrationPattern = { 200, 100, 200, 100, 200 };
    private AndroidJavaObject m_CurrentActivity;

    void Start()
    {
        string msg = "Battery Level: " + (GetBatteryLevel() * 100) + "%";
        ShowToast(msg);

        SendNotification();
    }

    //method that calls our native plugin.
    public void ShowToast(string msg)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            // Retrieve the UnityPlayer class.
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass(
                "com.unity3d.player.UnityPlayer"
            );
            // Retrieve the UnityPlayerActivity object
            AndroidJavaObject unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>(
                "currentActivity"
            );
            // Retrieve the "Bridge" from our native plugin.
            // ! Notice we define the complete package name.
            AndroidJavaObject alert = new AndroidJavaObject("com.a178.mylibrary.Vibrate");
            // Setup the parameters we want to send to our native plugin.
            object[] parameters = new object[2];
            parameters[0] = unityActivity;
            parameters[1] = msg;
            // Call PrintString in bridge, with our parameters.
            alert.Call("PrintString", parameters);
        }
    }

    public float GetBatteryLevel()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass(
                "com.unity3d.player.UnityPlayer"
            );
            AndroidJavaObject unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>(
                "currentActivity"
            );
            AndroidJavaObject alert = new AndroidJavaObject("com.a178.mylibrary.Battery");
            object[] parameters = new object[1];
            parameters[0] = unityActivity;
            return alert.Call<float>("GetBatteryPct", parameters);
        }
        return -1f;
    }

    public void SendNotification()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass(
            "com.unity3d.player.UnityPlayerActivity"
        );
        m_CurrentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        int iconResourceId = Resources.Load<Sprite>("glasses").GetInstanceID();
        // Create the notification builder
        AndroidJavaObject notificationBuilder = new AndroidJavaObject(
            "android.app.Notification$Builder",
            m_CurrentActivity
        )
            .Call<AndroidJavaObject>("setContentTitle", "My Notification")
            .Call<AndroidJavaObject>("setContentText", "This is my notification text")
            .Call<AndroidJavaObject>("setSmallIcon", iconResourceId);

        // Set the vibration pattern
        long[] vibrationPattern = { 1000, 2000, 1000, 2000 };
        notificationBuilder.Call("setVibration", vibrationPattern);

        // Send the notification
        AndroidJavaObject notificationManager = m_CurrentActivity.Call<AndroidJavaObject>(
            "getSystemService",
            "notification"
        );
        notificationManager.Call("notify", 1, notificationBuilder.Call<AndroidJavaObject>("build"));
    }
}
