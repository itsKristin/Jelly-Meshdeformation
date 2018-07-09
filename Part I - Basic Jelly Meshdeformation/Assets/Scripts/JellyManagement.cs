using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyManagement : MonoBehaviour {

	//Instance
	static JellyManagement instance;

	//MouseInput Settings:
	static bool useStandartMouseInput;
	static bool allowToPickUp;
	static float clickPressure;
	static float forceOffset;

	//Physics Settings:
	static bool reactToGravity;
	static bool allowRotation;

	//Consistency Settings:
	static bool useManualSettings;
	static float stiffness = 10f;
	static float attenuation = 5.5f;

	//Book Keeping
	[HideInInspector] 
	public List<GameObject> jelliedBodies = new List<GameObject>();

	//Getters & Setters
	public static JellyManagement Instance {get{ return instance;} set{ instance = value;}}

	public static bool UseStandartMouseInput {get{ return useStandartMouseInput;} set{ useStandartMouseInput = value;}}
	public static bool AllowToPickUp {get{ return allowToPickUp;} set{ allowToPickUp = value;}}
	public static float ClickPressure {get{ return clickPressure;} set{ clickPressure = value;}}
	public static float ForceOffset {get{ return forceOffset;} set{ forceOffset = value;}}

	public static bool ReactToGravity {get{ return reactToGravity;} set{ reactToGravity = value;}}
	public static bool AllowRotation {get { return allowRotation;} set{ allowRotation = value;}}

	public static bool UseManualSettings {get { return useManualSettings;} set{ useManualSettings = value;}}
	public static float Stiffness{get{ return stiffness;} set{ stiffness = value;}}
	public static float Attenuation {get{ return attenuation;} set{ attenuation = value;}}

	void Awake()
	{
		instance = this;
		DontDestroyOnLoad(this);

		jelliedBodies.Clear();
	}

	public void AddJelly(GameObject _gameObject)
	{
		if(!jelliedBodies.Contains(_gameObject)){
			jelliedBodies.Add(_gameObject);
		}
	}

	public void AddForceToVerts(Vector3[] _contactPoints, float _force, GameObject _jelliedObject, JellyBody _jellyBody)
	{
		Vector3 currentPoint;

		for(int i = 0; i < _contactPoints.Length; i++)
		{
			currentPoint = _jelliedObject.transform.InverseTransformPoint(_contactPoints[i]);
			for(int j = 0; j < _jellyBody.DisplacedVerts.Length; j++)
			{

				Vector3 pointToVert = (_jellyBody.DisplacedVerts[j] - currentPoint) * _jellyBody.UnitformScale;
				if(!useManualSettings)
				{
					float attenuatedForce = (_force/((_jellyBody.Attenuation)/2f)) / (1f + pointToVert.sqrMagnitude);
					float velocity = attenuatedForce * Time.deltaTime;
					_jellyBody.VertVelocities[j] += pointToVert.normalized * velocity;
				} 
				else 
				{
					float attenuatedForce = (_force/((attenuation)/2f)) / (1f + pointToVert.sqrMagnitude);
					float velocity = attenuatedForce * Time.deltaTime;
					_jellyBody.VertVelocities[j] += pointToVert.normalized * velocity;
				}
			}
		}
	}

	void Update() 
	{
		for(int i = 0; i < jelliedBodies.Count; i++)
		{
			JellyBody currentJellybody = jelliedBodies[i].GetComponent<JellyBody>();
			UpdateVerts(currentJellybody);
		}	
	}

	void UpdateVerts(JellyBody _jellyBody)
	{
		for(int i = 0; i < _jellyBody.DisplacedVerts.Length; i++)
		{
			Vector3 velocity = _jellyBody.VertVelocities[i];
			Vector3 displacement = _jellyBody.DisplacedVerts[i] - _jellyBody.InitialVerts[i];
			float springforce = (useManualSettings) ? _jellyBody.Stiffness : stiffness;
			float dampening = (useManualSettings) ? _jellyBody.Attenuation : attenuation;
			displacement *= _jellyBody.UnitformScale;
			velocity -= displacement * springforce * Time.deltaTime;
			velocity *= _jellyBody.UnitformScale - dampening * Time.deltaTime;
			_jellyBody.VertVelocities[i] = velocity;
			_jellyBody.DisplacedVerts[i] += velocity * (Time.deltaTime/_jellyBody.UnitformScale);
		}

		_jellyBody.JellyMesh.vertices = _jellyBody.DisplacedVerts;
		_jellyBody.JellyMesh.RecalculateNormals();
	}
}
