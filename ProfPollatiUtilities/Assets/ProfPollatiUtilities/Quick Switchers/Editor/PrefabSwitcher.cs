#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

public class PrefabSwitcher : EditorWindow {
	private string baseDir= Directory.GetCurrentDirectory().ToString();

	[MenuItem("Pollati Utilities/Prefab Switcher",false,21)]
	public static void ShowWindow() {
		PrefabSwitcher prefabSwitcherWindow = (PrefabSwitcher)EditorWindow.GetWindow(typeof(PrefabSwitcher));
		prefabSwitcherWindow.titleContent = new GUIContent(" Prefab Switcher", EditorGUIUtility.FindTexture( "Prefab Icon" ));
	}

	private Vector2 scrollArea;
	GUILayoutOption[] medIcons_GUILayoutOption = new GUILayoutOption[] { GUILayout.Width(50.0f), GUILayout.Height(50.0f) };

	void OnGUI() {
		Texture2D icon;
		// Find the default icon for a Prefab
		Texture defIcon = EditorGUIUtility.FindTexture("Prefab Icon");

		// Start main scroll area
		scrollArea = EditorGUILayout.BeginScrollView(scrollArea);

		if(AssetDatabase.IsValidFolder("Assets/Prefabs")) {
			// Get all the Scenes in the Asset/Scenes folder
			string[] guids2 = AssetDatabase.FindAssets("t:prefab",new[] {"Assets/Prefabs"});

			foreach(string guid2 in guids2){
				/// @todo Sort by subfolders, for each subfolder, create a Foldout and add the scenes in there
				string fullpath = AssetDatabase.GUIDToAssetPath(guid2);
				if (GUI.Button(EditorGUILayout.BeginHorizontal("Button"), GUIContent.none)) {
					AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(fullpath));
				}
					icon = UnityEditor.AssetPreview.GetAssetPreview(AssetDatabase.LoadAssetAtPath<Object>(fullpath));
					GUILayout.Label((icon==null) ? defIcon : icon, medIcons_GUILayoutOption);
					GUILayout.Label(Path.GetFileNameWithoutExtension(fullpath));
				EditorGUILayout.EndHorizontal();
			}
		}

		EditorGUILayout.EndScrollView();
	}
}
#endif