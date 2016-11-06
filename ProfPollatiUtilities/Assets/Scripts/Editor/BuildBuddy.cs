#if UNITY_EDITOR
using UnityEditor;
using System.Diagnostics;
using UnityEngine;
using System.IO.Compression;
using System.IO;
using Ionic.Zip;

public class BuildBuddy : MonoBehaviour {
	[MenuItem("Pollati Utilities/Build Buddy")]
	public static void BuildGame () {
		BuildTarget[] targets = {
			BuildTarget.StandaloneLinuxUniversal,
			BuildTarget.StandaloneOSXIntel64,
			BuildTarget.StandaloneWindows,
			BuildTarget.StandaloneWindows64
		};

		string[] targetPrefix = {"-Linux","-Mac","-Win_x86","-Win_x86-64"};

		string baseDir = "Builds" + Path.DirectorySeparatorChar;
		string projectName = PlayerSettings.productName;
		string buildPath;
		string buildName;

		for(var i=0;i<targets.Length;i++) {
			// Figure the name of the folder to build to
			buildPath = baseDir + projectName + targetPrefix [i];

			// Figure the extension for the executable
			if (targets[i]==BuildTarget.StandaloneOSXIntel) {
				buildName = projectName + ".app";
			} else {
				buildName = projectName + ".exe";
			}

			// Build
			UnityEngine.Debug.Log ("Building"+targetPrefix[i]);
			BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath + Path.DirectorySeparatorChar + buildName, targets[i], BuildOptions.None);

			// Zip
			UnityEngine.Debug.Log ("Zipping"+targetPrefix[i]);
			using (ZipFile zip = new ZipFile()) {
				zip.AddDirectory (buildPath + Path.DirectorySeparatorChar);
				zip.Save(buildPath + ".zip");
			}

			// Done
			UnityEngine.Debug.Log ("Building"+targetPrefix[i]+ " Complete!");
		}

		// Show a dialog so that the user can also open the folder with the builds in it if they want
		if (EditorUtility.DisplayDialog ("Builds complete!", "Your builds were created for Pollati's class!", "Show Build Folder", "Okay")) {
			EditorUtility.RevealInFinder (baseDir);
		}
	}
}
#endif