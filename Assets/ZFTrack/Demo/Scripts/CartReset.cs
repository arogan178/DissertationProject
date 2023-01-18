using UnityEngine;

namespace ZenFulcrum.Track {

[RequireComponent(typeof(TrackCart))]
public class CartReset : MonoBehaviour {
	private Track initialTrack;
	private Vector3 initialPosition;
	private Quaternion initialRotation;

	public void Start() {
		initialTrack = GetComponent<TrackCart>().CurrentTrack;
		initialPosition = transform.position;
		initialRotation = transform.rotation;
	}

	public void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			ResetCart();
		}
	}

	public void ResetCart() {
		var cart = GetComponent<TrackCart>();
		cart.CurrentTrack = initialTrack;
		transform.position = initialPosition;
		transform.rotation = initialRotation;
		var rb = GetComponent<Rigidbody>();
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
	}
}

}
