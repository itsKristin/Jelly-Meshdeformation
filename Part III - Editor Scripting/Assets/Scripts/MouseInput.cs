using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInput : MonoBehaviour {

	[Header("Input Settings:")]
	public float pressureForce;

	protected RaycastHit hitInfo;
	protected Ray ray;

	protected Vector3 inputPoint;
	protected JellyBody jellyBody;
	protected GameObject jelly;

	public static MouseInput instance;

	private void Start(){
		instance = this;
	}

	private void Update(){
		CheckForMouseInput();	
	}

	private void CheckForMouseInput(){
		if(Input.GetMouseButton(0)){
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray,out hitInfo)){
				jellyBody = hitInfo.collider.gameObject.GetComponent<JellyBody>();
				if(jellyBody != null){
					inputPoint = hitInfo.point;
					inputPoint += hitInfo.normal * 0.1f;
					jellyBody.AddPointForce(pressureForce, inputPoint);
				}
			}
		}
	}
}
