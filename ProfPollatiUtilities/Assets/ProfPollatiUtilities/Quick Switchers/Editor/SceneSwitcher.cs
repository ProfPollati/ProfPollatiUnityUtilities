#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

public class SceneSwitcher : EditorWindow {
	private string baseDir= Directory.GetCurrentDirectory().ToString();

	private bool optionLoadAdditive = false;

	[MenuItem("Pollati Utilities/Scene Switcher",false,22)]
	public static void ShowWindow() {
		SceneSwitcher sceneSwitcherWindow = (SceneSwitcher)EditorWindow.GetWindow(typeof(SceneSwitcher));
		sceneSwitcherWindow.titleContent = new GUIContent(" Scene Switcher", EditorGUIUtility.FindTexture( "BuildSettings.Editor" ));
	}

	private Vector2 scrollArea;
	GUILayoutOption[] smallIcons_GUILayoutOption = new GUILayoutOption[] { GUILayout.Width(25.0f), GUILayout.Height(25.0f) };

	void OnGUI() {
		// Find the default icons for the build settings, which will be like the scene icon
		Texture icon = EditorGUIUtility.FindTexture( "BuildSettings.Editor");
		//optionLoadAdditive = EditorGUILayout.Toggle("Load Additive?", optionLoadAdditive);

		// Start main scroll area
		scrollArea = EditorGUILayout.BeginScrollView(scrollArea);

		if(AssetDatabase.IsValidFolder("Assets/Scenes")) {
			// Get all the Scenes in the Asset/Scenes folder
			string[] guids2 = AssetDatabase.FindAssets("t:scene",new[] {"Assets/Scenes"});
			foreach(string guid2 in guids2){
				/// @todo Sort by subfolders, for each subfolder, create a Foldout and add the scenes in there
				string fullpath = AssetDatabase.GUIDToAssetPath(guid2);
				if (GUI.Button(EditorGUILayout.BeginHorizontal("Button"), GUIContent.none)) {
					if(SceneManager.GetActiveScene().isDirty) {
						EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
					}
					if(optionLoadAdditive) {
						EditorSceneManager.OpenScene(fullpath,OpenSceneMode.Additive);
					} else {
						EditorSceneManager.OpenScene(fullpath,OpenSceneMode.Single);
					}
				}
				GUILayout.Label(icon, smallIcons_GUILayoutOption);
				GUILayout.Label(Path.GetFileNameWithoutExtension(fullpath));
				EditorGUILayout.EndHorizontal();
			}
		}

		EditorGUILayout.EndScrollView();
	}
}
#endif