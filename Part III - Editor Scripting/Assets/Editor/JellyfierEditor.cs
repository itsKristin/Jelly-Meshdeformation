using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class JellyfierEditor : EditorWindow 
{
	int editorMode;
	int setupType;

	string[] editorModes = new string[] {"Jellied Objects", "Unjellied Objects"};
	string[] setupTypes = new string[] {"Automatic", "Manually"};

	List<string> usedPrefabs = new List<string>();
	List<string> possiblePrefabs = new List<string>();

	JellyBody previewJelly;
	GameObject previewGameObject;
	Editor previewEditor;

	Vector2 usedPrefabsScroll;
	Vector2 possiblePrefabsScroll;	

	[MenuItem("itsKristin/Jellyfier")]
	public static void ShowWindow()
	{
		JellyfierEditor window = GetWindow<JellyfierEditor>("Jellyfier");
		window.position = new Rect(20f,80f,550f,500f);
	}

	void OnGUI() 
	{
		GUILayout.Label("itsKristin's Jellyfier", EditorStyles.boldLabel);
		GUILayout.Space(10);

		GUILayout.Label("Browse through the prefabs in your Project!", 
		EditorStyles.label);
		GUILayout.Label("Decide weather or not you want them to be jellyfied.", 
		EditorStyles.label);
		GUILayout.Space(15);


		int previousPadding = GUI.skin.window.padding.bottom;
		GUI.skin.window.padding.bottom = -20;

		Rect windowRect = GUILayoutUtility.GetRect(1f,17f);
		windowRect.x += 4f;
		windowRect.width -= 7f;

		editorMode = GUI.SelectionGrid(windowRect,editorMode,editorModes,2,"window");
		GUI.skin.window.padding.bottom = previousPadding;


		GetPrefabs();

		switch(editorMode)
		{
			case 0:
			if(usedPrefabs.Count == 0 && possiblePrefabs.Count == 0)
			{
				GUILayout.Label("Your project doesn't seem to have any suitable Prefabs yet!", 
				EditorStyles.boldLabel);
			}
			else if(usedPrefabs.Count == 0 && possiblePrefabs.Count != 0)
			{
				GUILayout.Label("There are no jellied prefabs yet.",
				EditorStyles.boldLabel);
			}
			else 
			{
				GUILayout.Label("Preview:", EditorStyles.boldLabel);

				if(previewGameObject != null)
				{
					if(previewGameObject.GetComponent<JellyBody>())
					{
						previewJelly = previewGameObject.GetComponent<JellyBody>();
					} else if (previewGameObject.GetComponentInChildren<JellyBody>())
					{
						previewJelly = previewGameObject.GetComponentInChildren<JellyBody>();
					}

					if(previewEditor == null)
					{
						previewEditor = Editor.CreateEditor(previewGameObject);
					}
					previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256f,256f),GUIStyle.none);
				}

				GUILayout.Label("Jelly Settings:",EditorStyles.boldLabel);
				using (new EditorGUI.DisabledScope(!previewGameObject))
				{
					GUILayout.Label("Jelly Consistency:", EditorStyles.boldLabel);
					GUILayout.Label("REMEBER: If you toggle automatic setup all personalized settings will be ignored!",
					EditorStyles.label);
					setupType = (JellyManagement.UseManualSettings) ? 0 : 1;
					setupType = GUILayout.SelectionGrid(setupType,setupTypes,2,"Toggle");
					JellyManagement.UseManualSettings = (setupType == 0) ? false : true; 

					using (new EditorGUI.DisabledScope(setupType == 0))
					{
						if(previewJelly)
						{
							GUILayout.Label("Physics Settings:", EditorStyles.boldLabel);
							previewJelly.ReactToGravity = EditorGUILayout.Toggle("React to Gravity", 
							previewJelly.ReactToGravity);
							previewJelly.AllowRotation = EditorGUILayout.Toggle("Allow Rotation", 
							previewJelly.AllowRotation);
							GUILayout.Space(10);
							previewJelly.Stiffness = EditorGUILayout.FloatField("Stiffness", 
							previewJelly.Stiffness);
							previewJelly.Attenuation = EditorGUILayout.FloatField("Attenuation", 
							previewJelly.Attenuation);
						}
					}
					
				}
	
				GUILayout.Space(10);
				GUILayout.Label("Jellyfied Prefabs:",EditorStyles.boldLabel);
				usedPrefabsScroll = GUILayout.BeginScrollView(usedPrefabsScroll);
				for(int i = 0; i < usedPrefabs.Count; i++)
				{
					GUILayout.BeginVertical();
					GUILayout.Label(PrefabName(usedPrefabs[i]), EditorStyles.miniBoldLabel);
					GUILayout.Label(usedPrefabs[i], GUILayout.Width(position.width/2));

					GUILayout.BeginHorizontal();

					if(GUILayout.Button("Select",GUILayout.Width(position.width/2-10)))
					{
						Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(usedPrefabs[i]);
						previewGameObject = (GameObject) AssetDatabase.LoadMainAssetAtPath(usedPrefabs[i]);
						previewEditor = Editor.CreateEditor(previewGameObject);
						previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256,256),
						GUIStyle.none);
					}
					if(GUILayout.Button("Unjellify", GUILayout.Width(position.width/2-10)))
					{
						Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(usedPrefabs[i]);
						previewGameObject = (GameObject) AssetDatabase.LoadMainAssetAtPath(usedPrefabs[i]);
						Unjellify(previewGameObject);
						GetPrefabs();
						Repaint();
						previewGameObject = null;							
					}
					
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
				}
				GUILayout.EndScrollView();
			}
			
			break;
			case 1:
			if(usedPrefabs.Count == 0 && possiblePrefabs.Count == 0)
			{
				GUILayout.Label("Your project doesn't seem to have any suitable Prefabs yet!", 
				EditorStyles.boldLabel);
			}
			else if(usedPrefabs.Count != 0 && possiblePrefabs.Count == 0)
			{
				GUILayout.Label("There are no suitable prefabs that aren't jellyfied.",
				EditorStyles.boldLabel);
			}
			else 
			{
				GUILayout.Label("Preview:", EditorStyles.boldLabel);

				if(previewGameObject != null)
				{
					if(previewEditor == null)
					{
						previewEditor = Editor.CreateEditor(previewGameObject);
					}
					previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256f,256f),
					GUIStyle.none);
				}


				GUILayout.Space(10);
				GUILayout.Label("Suitable Prefabs:",EditorStyles.boldLabel);
				usedPrefabsScroll = GUILayout.BeginScrollView(possiblePrefabsScroll);
				for(int i = 0; i < possiblePrefabs.Count; i++)
				{
					GUILayout.BeginVertical();
					GUILayout.Label(PrefabName(possiblePrefabs[i]), EditorStyles.miniBoldLabel);
					GUILayout.Label(possiblePrefabs[i], GUILayout.Width(position.width/2));

					GUILayout.BeginHorizontal();

					if(GUILayout.Button("Select",GUILayout.Width(position.width/2-10)))
					{
						Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(possiblePrefabs[i]);
						previewGameObject = (GameObject) AssetDatabase.LoadMainAssetAtPath(possiblePrefabs[i]);
						previewEditor = Editor.CreateEditor(previewGameObject);
						previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256,256),
						GUIStyle.none);
					}
					if(GUILayout.Button("Jellify", GUILayout.Width(position.width/2-10)))
					{
						Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(possiblePrefabs[i]);
						previewGameObject = (GameObject) AssetDatabase.LoadMainAssetAtPath(possiblePrefabs[i]);
						Jellify(previewGameObject);
						GetPrefabs();
						Repaint();
						previewGameObject = null;							
					}
					
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
				}
				GUILayout.EndScrollView();
			}
			break;
		}
	}

	void Unjellify(GameObject _object)
	{
		Component component = null;
		if(_object.GetComponent<JellyBody>()){
			component = _object.GetComponent<JellyBody>();

		} else if (_object.GetComponentInChildren<JellyBody>()){
			component = _object.GetComponentInChildren<JellyBody>();
		}

		if(component != null)
		{
			DestroyImmediate(component,true);
		}
	}

	void Jellify(GameObject _object)
	{
		if(_object.GetComponent<MeshFilter>()){
			_object.AddComponent<JellyBody>();
		} else if (_object.GetComponentInChildren<MeshFilter>())
		{
			MeshFilter component = _object.GetComponentInChildren<MeshFilter>();
			component.gameObject.AddComponent<JellyBody>();
		}
	}

	void GetPrefabs()
	{
		usedPrefabs.Clear();
		possiblePrefabs.Clear();

		string[] assetPaths = AssetDatabase.GetAllAssetPaths();
		List<string> prefabPaths = new List<string>();

		for(int i = 0; i < assetPaths.Length; i++)
		{
			if(assetPaths[i].Contains(".prefab"))
			{
				prefabPaths.Add(assetPaths[i]);
			}
		}

		if(prefabPaths.Count != 0)
		{
			for(int j = 0; j < prefabPaths.Count; j++)
			{
				Object obj = AssetDatabase.LoadMainAssetAtPath(prefabPaths[j]);
				GameObject go;

				try
				{
					go = (GameObject) obj;
					if(go.GetComponent<JellyBody>() || 
					go.GetComponentInChildren<JellyBody>())
					{
						if(!usedPrefabs.Contains(prefabPaths[j]))
						{
							usedPrefabs.Add(prefabPaths[j]);
						}
					}
					else if (go.GetComponent<MeshFilter>() || 
					go.GetComponentInChildren<MeshFilter>())
					{
						if(!possiblePrefabs.Contains(prefabPaths[j]))
						{
							possiblePrefabs.Add(prefabPaths[j]);
						}
					}
				} 
				catch
				{
					Debug.LogError("Prefab " + prefabPaths[j] + 
					" doesn't cast to GameObject!"); 
				}
			}
		}
	}

	string PrefabName(string path)
	{
		string name = path.Substring(path.LastIndexOf('/') + 1);
		return name.Substring(0,name.Length - 7);
	}
}
