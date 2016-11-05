# Professor Pollati's Unity Utilities
This repo contains the Unity Editor scripts that can be used in my classes to help make some task as simple as a menu choice. These editor scripts should work on both Mac and Windows.

## Releases
In the [Releases](https://github.com/ProfPollati/ProfPollatiUnityUtilities/releases), you will find the latest ProfPollatiUtilities.unitypackage which will contains all the files you need. Just go in the menu **Assets** -> **Import Package** -> **Custom Package** and select the unity package.

## The Scripts

### Build Buddy
This basic script will produce the Linux, Mac, and Windows builds and zip them for you. That way, you have the required releases, named properly for the GitHub Releases required for submitting in your project.

Once it completes, it will pop open a requester where you can hit _Okay_ or _Show Build Folder_ to have the Build folder show up in Explorer on Windows or in the Finder for Mac.

#### Notes
In order to easily produce a ZIP file with Unity 5.4 or lower, the library [Ionic.Zip.Unity](https://github.com/r2d2rigo/dotnetzip-for-unity) from https://github.com/r2d2rigo/dotnetzip-for-unity in needed in the Assets/Plugin/ folder.

### Launch Duplicator
When trying to test out your project for the Multiplayer assignments, you either have to make a build of the game and launch that build and launch the editor, or you can make a duplicate of your project and launch that in another instance of Unity. Launching another instance makes it a little faster to changing things around, but it can take more time to copy the files, _unless_ you use this script.

This will add a window with controls for you to Save, Duplicate and Launch. If you are running the game, it will not save the scenes or project since it may lead to issue with project.
