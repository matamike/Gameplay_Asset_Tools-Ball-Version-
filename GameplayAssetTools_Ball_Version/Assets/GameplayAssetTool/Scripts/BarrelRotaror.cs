using UnityEngine;
using System.Collections;

public class BarrelRotaror : MonoBehaviour {
	//rotator script for projectors.
	void Update () {
		this.gameObject.transform.Rotate (Vector3.up * 100f * Time.deltaTime);
	}
}
