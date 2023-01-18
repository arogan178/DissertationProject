//-----------------------------------------------------------------------
// <copyright file="ObjectController.cs" company="Google Inc.">
// Copyright 2014 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

/// <summary>Controls interactable teleporting objects in the Demo scene.</summary>
[RequireComponent(typeof(Collider))]
public class ChooseGameScene : MonoBehaviour
{
    /// <summary>
    /// The material to use when this object is inactive (not being gazed at).
    /// </summary>
    public Material inactiveMaterial;

    /// <summary>The material to use when this object is active (gazed at).</summary>
    public Material gazedAtMaterial;

    private Vector3 startingPosition;
    private Renderer myRenderer;

    GameObject[] canvasElements;

    /// <summary>Sets this instance's GazedAt state.</summary>
    /// <param name="gazedAt">
    /// Value `true` if this object is being gazed at, `false` otherwise.
    /// </param>
    public void SetGazedAt(bool gazedAt)
    {
        if (inactiveMaterial != null && gazedAtMaterial != null)
        {
            myRenderer.material = gazedAt ? gazedAtMaterial : inactiveMaterial;
            return;
        }
    }

    /// <summary>Resets this instance and its siblings to their starting positions.</summary>
    public void Reset()
    {
        int sibIdx = transform.GetSiblingIndex();
        int numSibs = transform.parent.childCount;
        for (int i = 0; i < numSibs; i++)
        {
            GameObject sib = transform.parent.GetChild(i).gameObject;
            sib.transform.localPosition = startingPosition;
            sib.SetActive(i == sibIdx);
        }
    }

//    /// <summary>Calls the Recenter event.</summary>
//    public void Recenter()
//    {
//#if !UNITY_EDITOR
//            GvrCardboardHelpers.Recenter();
//#else
//        if (GvrEditorEmulator.Instance != null)
//        {
//            GvrEditorEmulator.Instance.Recenter();
//        }
//#endif  // !UNITY_EDITOR
//    }

    /// <summary>Teleport this instance randomly when triggered by a pointer click.</summary>
    /// <param name="eventData">The pointer click event which triggered this call.</param>
    public void LoadGame(BaseEventData eventData)
    {
        string tag = gameObject.tag;
        if (tag.Equals("Game1"))
        {
            SceneManager.LoadScene("TrackScene");
        }
        else if (tag.Equals("Game2"))
        {
            SceneManager.LoadScene("TrackScene");
        }
        else
        {
            SceneManager.LoadScene("TrackScene");
        }

        SetGazedAt(false);
    }

    private void Start()
    {
        startingPosition = transform.localPosition;
        myRenderer = GetComponent<Renderer>();
        SetGazedAt(false);

        canvasElements = GameObject.FindGameObjectsWithTag("UICanvas");

        if (canvasElements != null)
        {
            foreach (GameObject uiElement in canvasElements)
            {
                uiElement.SetActive(false);
            }
        }
    }
}

