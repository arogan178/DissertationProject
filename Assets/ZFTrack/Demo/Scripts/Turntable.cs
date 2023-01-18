using UnityEngine;

namespace ZenFulcrum.Track {

[RequireComponent(typeof(Track))]
public class Turntable : MonoBehaviour {
	public Track[] endpoints;

	public float speed = 45;

	private int selected = -1;
	private bool moving = false;
	private Vector3 center;
	private Track track;
	private GameObject table;

	protected void Start() {
		track = GetComponent<Track>();
		center = track.Centeroid;

		table = new GameObject("Table");
		table.transform.position = center;
		table.transform.parent = transform.parent;
		transform.parent = table.transform;
		transform.rotation = Quaternion.identity;

		Switch();
	}

	protected void Update() {
		if (!moving) return;

		var target = endpoints[selected];

		var startRot = table.transform.rotation;
		table.transform.LookAt(target.TrackAbsoluteStart.position);
		var endRot = table.transform.rotation;

		table.transform.rotation = Quaternion.RotateTowards(startRot, endRot, speed * Time.deltaTime);

		if (table.transform.rotation == endRot) {
			moving = false;
			target.PrevTrack = track;
			track.NextTrack = target;
		}
	}

	public void Switch() {
		Switch((selected + 1) % endpoints.Length);
	}

	public void Switch(int index) {
		selected = index;
		moving = true;

		//no connections
		foreach (var endpoint in endpoints) {
			endpoint.PrevTrack = null;
		}
		track.NextTrack = null;
	}
}

}

