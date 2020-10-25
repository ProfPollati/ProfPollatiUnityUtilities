#if UNITY_EDITOR
// You'll need to include compiler conditions to only compile this if this is going through the Unity Editor, otherwise, you will not be able to compile a Build!
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Diagnostics;

public class LaunchDuplicator : EditorWindow {
	private static Process newUnity = null;

	private string unityLocation = ""; // Set in OnEnable
	private string baseDir = "";       // Set in OnEnable
	private string destDir = "";       // Set in OnEnable

	private string output = "";

	private bool showDetails = false;
	private bool deleteBeforeCopy = false;

	private Vector2 scrollArea;
	private Vector2 scrollConfigArea;
	private Vector2 scrollOutputArea;

	GUILayoutOption[] tinyIcons_GUILayoutOption = new GUILayoutOption[] { GUILayout.Width(20.0f), GUILayout.Height(20.0f) };

	// Add menu item named "Duplicate Launcher" to a new menu
	[MenuItem("Pollati Utilities/Duplicate Launcher")]
	public static void ShowWindow() {
		//Show existing window instance. If one doesn't exist, make one.
		LaunchDuplicator launchDuplicatorWindow = (LaunchDuplicator)EditorWindow.GetWindow(typeof(LaunchDuplicator));
		launchDuplicatorWindow.titleContent = new GUIContent(" Duplicate Launcher", EditorGUIUtility.FindTexture("d_preAudioLoopOff"));
	}

	private void OnEnable() {
		baseDir = Directory.GetCurrentDirectory().ToString();
		destDir = Directory.GetParent(Directory.GetCurrentDirectory()).ToString() + Path.DirectorySeparatorChar + PlayerSettings.productName + "_COPY";
		unityLocation = EditorApplication.applicationPath + (Application.platform == RuntimePlatform.OSXEditor ? Path.DirectorySeparatorChar + "Contents/MacOS/Unity" : "");
	}

	void OnGUI() {
		// Start main scroll area
		scrollArea = EditorGUILayout.BeginScrollView(scrollArea);

		// Description
		GUILayout.Label ("Simplifies testing multiplayer networking. Click the button to save the project (if not playing), duplicate it, and launch a new instance of Unity with the copied project.", EditorStyles.wordWrappedLabel);
		EditorGUILayout.Space();

		// Delete option
		deleteBeforeCopy = EditorGUILayout.ToggleLeft("Delete existing copy before launching",deleteBeforeCopy);

			// If the duplicate Unity is running, allow us to terminate it
			if(newUnity!=null && newUnity.HasExited==false) {
				if(GUILayout.Button("Terminate")) { TerminateUnity(); }
			} else {
				// Otherwise, if the editor is playing the game, duplicate and launch
				if(Application.isPlaying) {
					if(GUILayout.Button("Duplicate and Launch")) { DuplicateAndLaunch(); }
				// But if the editor is not playing the game, we should save the scene and assets
				} else {
					if(GUILayout.Button("Save, Duplicate, and Launch")) { DuplicateAndLaunch(); }
				}
			}

			EditorGUILayout.Space();
			GUILayout.Label("Update Particulars:", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
				// Button to update all assets instead of making a complete copy
				EditorGUILayout.BeginVertical();
					if(GUILayout.Button("All Items",GUILayout.MinHeight(109.0f))) { UpdateItems(); }
				EditorGUILayout.EndVertical();

				// Buttons to update only certain folders instead of making a complete copy
				EditorGUILayout.BeginVertical();
					if (GUI.Button(EditorGUILayout.BeginHorizontal("Button"), GUIContent.none)) { UpdateFolder("Scripts/"); }
						GUILayout.Label(EditorGUIUtility.FindTexture("cs Script Icon"),tinyIcons_GUILayoutOption);
						GUILayout.Label("Scripts");
					EditorGUILayout.EndHorizontal();
					
					if (GUI.Button(EditorGUILayout.BeginHorizontal("Button"), GUIContent.none)) { UpdateFolder("Scenes/"); }
						GUILayout.Label(EditorGUIUtility.FindTexture("BuildSettings.Editor"),tinyIcons_GUILayoutOption);
						GUILayout.Label("Scenes");
					EditorGUILayout.EndHorizontal();

					if (GUI.Button(EditorGUILayout.BeginHorizontal("Button"), GUIContent.none)) {  UpdateFolder("Prefabs/"); }
						GUILayout.Label(EditorGUIUtility.FindTexture("Prefab Icon"),tinyIcons_GUILayoutOption);
						GUILayout.Label("Prefabs");
					EditorGUILayout.EndHorizontal();

					if (GUI.Button(EditorGUILayout.BeginHorizontal("Button"), GUIContent.none)) {  UpdateFolder("Scriptable Objects/"); }
						GUILayout.Label(EditorGUIUtility.IconContent("ScriptableObject Icon"),tinyIcons_GUILayoutOption);
						GUILayout.Label("Scriptable Objects");
					EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			// Output
			showDetails = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showDetails, "Configs", true);
			if(showDetails) {
				scrollConfigArea = EditorGUILayout.BeginScrollView(scrollConfigArea,GUILayout.MinHeight(175));
					// Display the current project folder
					GUILayout.Label ("Project Base:", EditorStyles.boldLabel);
					GUILayout.TextField (baseDir, EditorStyles.wordWrappedLabel);

					// Show where the duplicate project folder will be
					GUILayout.Label ("Destination Directory: ", EditorStyles.boldLabel);
					GUILayout.TextField (destDir, EditorStyles.wordWrappedLabel);

					// Show the location of Unity executable, allow it to be edited in case the user wants to
					GUILayout.Label ("Unity Application Location: ", EditorStyles.boldLabel);
					unityLocation = GUILayout.TextField (unityLocation, EditorStyles.textField);
				EditorGUILayout.EndScrollView();
			}

			EditorGUILayout.Space();

			// Show output so we can see what is going on, or wrong.
			GUILayout.Label ("Output ", EditorStyles.boldLabel);

			// Scrollable output window
			scrollOutputArea = EditorGUILayout.BeginScrollView(scrollOutputArea, GUILayout.MinHeight(75));
				EditorGUILayout.TextArea(output, EditorStyles.textArea);
			EditorGUILayout.EndScrollView();

		// End main scroll area
		EditorGUILayout.EndScrollView();

		EditorGUILayout.Space();
	}

	void UpdateFolder(string which) {
		string baseDirComplete = baseDir + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + which;
		string destDirComplete = destDir + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + which;

		if(deleteBeforeCopy) {
			FileUtil.DeleteFileOrDirectory(destDirComplete);
		}
		DirectoryCopy(baseDirComplete, destDirComplete,true,true);
	}

	void UpdateItems() {
		UpdateFolder("Scripts/");
		UpdateFolder("Scenes/");
		UpdateFolder("Prefabs/");
		UpdateFolder("Scriptable Objects/");
	}

	/// <summary>
	/// Handles saving, deleting, duplicating, and launching
	/// </summary>
	void DuplicateAndLaunch() {
		output = "Attempting launch duplicate...";

		// Only save the scene if not running and dirty, otherwise we can run into interesting issues...
		if (EditorApplication.isPlaying == false) {
			output += "\n" + "Saving project and assets...";

			// Save Scene(s)
			bool needToSave = false;
			int scenes = SceneManager.sceneCount;
			if (scenes > 0) {
				for (int n = 0; n < SceneManager.sceneCount; ++n) {
					Scene scene = SceneManager.GetSceneAt (n);
					if (scene.isDirty) {
						needToSave = true;
						break;
					}
				}

				if (needToSave) {
					if (EditorSceneManager.SaveOpenScenes ()) {
						output += "\n" + "*** ERROR: Saving scenes failed. ***";
						return;
					} else {
						output += "\n" + "Saved " + scenes +" scenes" + ((scenes>1) ? "s" : "");
					}
				}
			}

			// Save Project
			AssetDatabase.SaveAssets();
			output += "\n" + "Saved assets";
		}

		// Try to copy the project folder, if not lets list why
		try{
			if(deleteBeforeCopy && Directory.Exists(destDir)) {
				output += "\n" + "Deleting old copy...";
				FileUtil.DeleteFileOrDirectory(destDir);
			}

			if(Directory.Exists(destDir)) {
				output += "\n" + "Replacing old copy...";
				//FileUtil.ReplaceDirectory(baseDir, destDir);
				DirectoryCopy(baseDir,destDir,true,true);
			} else {
				output += "\n" + "Copying project...";
				//FileUtil.CopyFileOrDirectory(baseDir,destDir);
				DirectoryCopy(baseDir,destDir,true,false);
			}
		} catch (IOException ex) {
			if (ex.ToString ().IndexOf ("editor\\FileUtilBindings.gen.cs:75") < 1) {
				output += "\n" + "*** FAILED: " + ex.ToString () + "***";
				return;
			} else {
				output += "\n" + "I know Unity says it failed, but it is " + Directory.Exists (destDir).ToString () + " that the dest dir exists!";
			}
		}

		if(File.Exists(unityLocation)) {
			output += "\n" + "Launching copy...";
			// Launch another Unity
			newUnity = new Process();
			newUnity.StartInfo.FileName = unityLocation;
			newUnity.StartInfo.Arguments = "-projectPath \"" + destDir + "\"";
			newUnity.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

			if(!newUnity.Start()) {
				output += "\n" + "*** FAILED: Could not start Unity instance. ***";
			} else {
				output += "\n" + "New instance of Unity running!";
			}
		} else {
			output += "\n" + "*** FAILED: Unity Location \"" + unityLocation + "\" does not exist! ***";
		}
	}

	/// <summary>
	/// Terminates the duplicate Unity instance.
	/// </summary>
	void TerminateUnity() {
		newUnity.Kill();
		output = "";
	}

	/// <summary>
	/// Directories a copy, since the default Unity FileUtil doesn't want to work.
	/// </summary>
	/// <see cref="https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx"/>
	/// <param name="sourceDirName">Source dir name.</param>
	/// <param name="destDirName">Destination dir name.</param>
	/// <param name="copySubDirs">If set to <c>true</c> copy sub directories.</param>
	/// <param name="overwriteExisting">If set to <c>true</c> overwrite existing files.</param>
	private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwriteExisting) {
		// Get the subdirectories for the specified directory.
		DirectoryInfo dir = new DirectoryInfo(sourceDirName);

		if (!dir.Exists) {
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}

		DirectoryInfo[] dirs = dir.GetDirectories();

		// Ignore the temp folder
		if(destDirName.IndexOf("\\Temp")<1 && destDirName.IndexOf("\\.vs")<1) {
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName)) {
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files) {
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, overwriteExisting);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs) {
				foreach (DirectoryInfo subdir in dirs) {
					string temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, copySubDirs, overwriteExisting);
				}
			}
		}
	}
}
#endif
