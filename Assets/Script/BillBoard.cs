using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour 
{
	public Vector3 rotationOffset;

	void Update () 
    {
		Vector3 p = Camera.main.transform.position;
		p.y = transform.position.y;
		transform.LookAt (p);
		transform.rotation *= Quaternion.Euler(rotationOffset);
	}
}