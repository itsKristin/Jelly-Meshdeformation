using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyBody : MonoBehaviour {

	float stiffness;
	float attenuation;

	Vector3[] initialVerts;
	Vector3[] displacedVerts;
	Vector3[] vertVelocities;
	int[] tris;

	Mesh mesh;
	Rigidbody rigidBody;

	float volume;

	public Mesh JellyMesh 
	{
		get{ return mesh;} 
		set{ mesh = value;}
	}

	public float Stiffness
	{
		get{ return stiffness;} 
		set{ stiffness = value;}
	}


	public float Attenuation 
	{
		get{ return attenuation;} 
		set{ attenuation = value;}
	}

	public float UnitformScale 
	{
		get {return 1f;}
	}

	public Vector3[] InitialVerts 
	{
		get{ return initialVerts;} 
		set{ initialVerts = value;}
	}

	public Vector3[] DisplacedVerts 
	{
		get { return displacedVerts;} 
		set{ displacedVerts = value;}
	}

	public Vector3[] VertVelocities 
	{
		get { return vertVelocities;} 
		set{ vertVelocities = value;}
	}

	void Start()
	{
		mesh = GetComponent<MeshFilter>().mesh;
		rigidBody = GetComponent<Rigidbody>();

		initialVerts = mesh.vertices;
		displacedVerts = new Vector3[initialVerts.Length];
		for(int i = 0; i < displacedVerts.Length; i++)
		{
			displacedVerts[i] = initialVerts[i];
		}
		vertVelocities = new Vector3[initialVerts.Length];
		
		tris = mesh.triangles;

		volume = AssumedVolume();
		stiffness = volume;
		attenuation = Dampening();
		rigidBody.mass = volume * 10f;

		JellyManagement.Instance.AddJelly(gameObject);
	}


	float AssumedVolume()
	{
		float _volume = 0;
		float scaledVolume = 0;

		for(int i = 0; i < tris.Length; i+= 3)
		{
			Vector3 a = initialVerts[tris[i]];
			Vector3 b = initialVerts[tris[i+1]];
			Vector3 c = initialVerts[tris[i+2]];
			
			_volume += (Vector3.Dot(a, Vector3.Cross(b,c))/6.0f);
			scaledVolume = _volume + (_volume * ProcentualScaleFactor());
		}
		return Mathf.Abs(scaledVolume);
	}

	float ProcentualScaleFactor()
	{
		Vector3 _localScale = transform.localScale;

		float sFx = (_localScale.x > 1.0f) ? _localScale.x - 1.0f : _localScale.x;
		float sFy = (_localScale.y > 1.0f) ? _localScale.y - 1.0f : _localScale.y;
		float sFz = (_localScale.z > 1.0f) ? _localScale.z - 1.0f : _localScale.z;

		return ((sFx + sFy + sFz)/3.0f);
	}

	float Dampening()
	{
		return ((((1f/volume) * volume) * (1f-(ProcentualScaleFactor()/10f))) * 2);
	}

	public void AddPointForce(float _force, Vector3 _pressurePoint)
	{
		Vector3[] pressurePoints = new Vector3[1];
		pressurePoints[0] = _pressurePoint;
		JellyManagement.Instance.AddForceToVerts(pressurePoints,_force,gameObject,this);
	}

	void OnCollisionStay(Collision other) 
	{
		if(other.contacts.Length > 0)
		{
			Vector3[] contactPoints = new Vector3[other.contacts.Length];
			for(int i = 0; i < other.contacts.Length; i++)
			{
				Vector3 currentContactpoint = other.contacts[i].point;
				currentContactpoint += other.contacts[i].normal * mesh.bounds.max.x;
				contactPoints[i] = currentContactpoint;
			}

			JellyManagement.Instance.AddForceToVerts(contactPoints,rigidBody.mass,
			gameObject,this);
		}	
	}
}
