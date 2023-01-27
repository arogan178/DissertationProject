using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;
using System.IO;

public class GameManagerScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
            Permission.RequestUserPermission(Permission.CoarseLocation);
    }
}
