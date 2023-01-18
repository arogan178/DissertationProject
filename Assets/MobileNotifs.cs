using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Notifications.Android;

public class MobileNotifs : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Notification Channel setup
        var channel = new AndroidNotificationChannel()
        {
            Id = "1",
            Name = "Notifications Channel",
            Importance = Importance.Default,
            Description = "Affective State Triggered",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        
        //Sending of notification
        var notification = new AndroidNotification();
        notification.Title = "In Flow";
        notification.Text = "You're in flow, keep going";
        notification.FireTime = System.DateTime.Now.AddSeconds(10);

        AndroidNotificationCenter.SendNotification(notification, "1");
    }

    // Update is called once per frame
    void Update() { }
}
