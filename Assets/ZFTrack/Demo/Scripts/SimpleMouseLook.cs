using UnityEngine;

namespace ZenFulcrum.Track {

/// <summary>
/// Simple no-nonsense mouse look script for use in demos. It's not very refined, but it's simple.
///
/// (The 5.x and newer controllers are pretty heavy to include for a demo.)
/// </summary>
public class SimpleMouseLook : MonoBehaviour {

	public float lookSpeed = 2.5f;

	private float lookPitch, lookYaw;

	public void Awake() {
	}

	public void Update() {
		var lookDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * lookSpeed;

		lookYaw = (lookYaw + lookDelta.x) % 360f;

		lookPitch += -lookDelta.y;
		lookPitch = Mathf.Clamp(lookPitch, -90, 90);

		transform.localRotation = Quaternion.Euler(lookPitch, lookYaw, 0);
	}

}

}
