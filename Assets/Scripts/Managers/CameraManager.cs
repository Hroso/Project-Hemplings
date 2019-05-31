using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GM 
{
	public class CameraManager : MonoBehaviour
	{
		public float moveSpeed = 0.01f;
		public Transform camTransform;
		public float minY;
		public float minX;
		public float maxX;

		void Update () 
		{
			float h = Input.GetAxis("Horizontal");
			Vector3 mp = Vector3.zero;
			mp.x = h * moveSpeed;
			Vector3 tp = camTransform.position + mp;
			tp.x = Mathf.Clamp(tp.x, minX, maxX + 0.01f);
			camTransform.position = tp;
			 

		}
	}
}

