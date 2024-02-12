//-----------------------------------------------------------------------
// <copyright file="GvrEditorEmulator.cs" company="Google Inc.">
// Copyright 2017 Google Inc. All rights reserved.
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

using Gvr.Internal;
using UnityEngine;

/// <summary>Provides mouse-controlled head tracking emulation in the Unity editor.</summary>
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrEditorEmulator")]
public class GvrEditorEmulator : MonoBehaviour
{
    // GvrEditorEmulator should only be compiled in the Editor.
    //
    // Otherwise, it will override the camera pose every frame on device which causes the
    // following behaviour:
    //
    // The rendered camera pose will still be correct because the VR.InputTracking pose
    // gets applied after LateUpdate has occured. However, any functionality that
    // queries the camera pose during Update or LateUpdate after GvrEditorEmulator has been
    // updated will get the wrong value applied by GvrEditorEmulator intsead.
#if UNITY_EDITOR
    private const string AXIS_MOUSE_X = "Mouse X";
    private const string AXIS_MOUSE_Y = "Mouse Y";

    // Simulated neck model.  Vector from the neck pivot point to the point between the eyes.
    private static readonly Vector3 NECK_OFFSET = new Vector3(0, 0.075f, 0.08f);

    private static GvrEditorEmulator instance;
    private static bool instanceSearchedFor = false;

    // Allocate an initial capacity; this will be resized if needed.
    private static Camera[] allCameras = new Camera[32];

    // Use mouse to emulate head in the editor.
    // These variables must be static so that head pose is maintained between scene changes,
    // as it is on device.
    private static float mouseX = 0;
    private static float mouseY = 0;
    private static float mouseZ = 0;

    /// <summary>Gets the instance for this singleton class.</summary>
    /// <value>The instance for this singleton class.</value>
    public static GvrEditorEmulator Instance
    {
        get
        {
            if (instance == null && !instanceSearchedFor)
            {
                instance = FindAnyObjectByType<GvrEditorEmulator>();
                instanceSearchedFor = true;
            }

            return instance;
        }
    }

    /// <summary>Gets the emulated head position.</summary>
    /// <value>The emulated head position.</value>
    public Vector3 HeadPosition { get; private set; }

    /// <summary>Gets the emulated head rotation.</summary>
    /// <value>The emulated head rotation.</value>
    public Quaternion HeadRotation { get; private set; }

    /// <summary>Recenters the emulated headset.</summary>
    public void Recenter()
    {
        mouseX = mouseZ = 0;  // Do not reset pitch, which is how it works on the phone.
        UpdateHeadPositionAndRotation();
        ApplyHeadOrientationToVRCameras();
    }

    /// <summary>Single-frame updates for this module.</summary>
    /// <remarks>Should be called in one MonoBehavior's `Update` method.</remarks>
    public void UpdateEditorEmulation()
    {
        if (InstantPreview.IsActive)
        {
            return;
        }

        bool rolled = false;
        if (CanChangeYawPitch())
        {
            Cursor.lockState = CursorLockMode.Locked;
#if ENABLE_INPUT_SYSTEM
            mouseX += UnityEngine.InputSystem.Mouse.current.delta.x.ReadValue() * 5 / 20.0f;
#else
            mouseX += Input.GetAxis(AXIS_MOUSE_X) * 5;
#endif
            if (mouseX <= -180)
            {
                mouseX += 360;
            }
            else if (mouseX > 180)
            {
                mouseX -= 360;
            }

#if ENABLE_INPUT_SYSTEM
            mouseY += UnityEngine.InputSystem.Mouse.current.delta.y.ReadValue() * -2.4f / 20.0f;
#else
            mouseY -= Input.GetAxis(AXIS_MOUSE_Y) * 2.4f;
#endif
            mouseY = Mathf.Clamp(mouseY, -85, 85);
        }
        else if (CanChangeRoll())
        {
            Cursor.lockState = CursorLockMode.Locked;
            rolled = true;
#if ENABLE_INPUT_SYSTEM
            mouseZ += UnityEngine.InputSystem.Mouse.current.delta.x.ReadValue() * 0.5f;
#else
            mouseZ += Input.GetAxis(AXIS_MOUSE_X) * 5;
#endif
            mouseZ = Mathf.Clamp(mouseZ, -85, 85);
        }
		else
		{
            Cursor.lockState = CursorLockMode.None;
        }

        if (!rolled)
        {
            // People don't usually leave their heads tilted to one side for long.
            mouseZ = Mathf.Lerp(mouseZ, 0, Time.deltaTime / (Time.deltaTime + 0.1f));
        }

        UpdateHeadPositionAndRotation();
        ApplyHeadOrientationToVRCameras();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogError("More than one active GvrEditorEmulator instance was found in your scene.\n" +
                           "Ensure that there is only one active GvrEditorEmulator.");
            this.enabled = false;
            return;
        }
    }

    private void Start()
    {
        UpdateAllCameras();
    }

    private void Update()
    {
        UpdateEditorEmulation();
    }

    private bool CanChangeYawPitch()
    {
#if ENABLE_INPUT_SYSTEM
        return UnityEngine.InputSystem.Keyboard.current.leftAltKey.isPressed || 
               UnityEngine.InputSystem.Keyboard.current.rightAltKey.isPressed;
#else
        return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
#endif
    }

    private bool CanChangeRoll()
    {
#if ENABLE_INPUT_SYSTEM
        return UnityEngine.InputSystem.Keyboard.current.leftCtrlKey.isPressed ||
               UnityEngine.InputSystem.Keyboard.current.rightCtrlKey.isPressed;
#else
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#endif
    }

    private void UpdateHeadPositionAndRotation()
    {
        HeadRotation = Quaternion.Euler(mouseY, mouseX, mouseZ);
        HeadPosition = (HeadRotation * NECK_OFFSET) - (NECK_OFFSET.y * Vector3.up);
    }

    private void ApplyHeadOrientationToVRCameras()
    {
        UpdateAllCameras();

        // Update all VR cameras using Head position and rotation information.
        for (int i = 0; i < Camera.allCamerasCount; ++i)
        {
            Camera cam = allCameras[i];

            // Check if the Camera is a valid VR Camera, and if so update it to track head motion.
            if (cam && cam.enabled && cam.stereoTargetEye != StereoTargetEyeMask.None)
            {
                cam.transform.localPosition = HeadPosition;
                cam.transform.localRotation = HeadRotation;
            }
        }
    }

    // Avoids per-frame allocations. Allocates only when allCameras array is resized.
    private void UpdateAllCameras()
    {
        // Get all Cameras in the scene using persistent data structures.
        if (Camera.allCamerasCount > allCameras.Length)
        {
            int newAllCamerasSize = Camera.allCamerasCount;
            while (Camera.allCamerasCount > newAllCamerasSize)
            {
                newAllCamerasSize *= 2;
            }

            allCameras = new Camera[newAllCamerasSize];
        }

        // The GetAllCameras method doesn't allocate memory (Camera.allCameras does).
        Camera.GetAllCameras(allCameras);
    }

#endif  // UNITY_EDITOR
}
