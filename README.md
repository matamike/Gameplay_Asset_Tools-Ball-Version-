# Gameplay Asset Tools
Gameplay Asset Tools is a basic collections of tools created to help out developers and designers to allow them certain functionalities.
This version contains a package of : Player controller , Camera controller and the Snapshot Manager/Viewer. The player controller is a ball oriented type of player and comes with the basic rolling functionality which camera direction based, as well as able to jump when needed. Jump variation exist in two different versions: idle and forward direction. Idle will hop on Y-Axis while  forward will follow the relative direction of the ball and create a ballistic type of movement jump with a more explosive force.


This triple gameplay tool is designed to assist either programmers or artists for performing their work in any kind of environment. This asset pack contains a player controller with a good variety of controls,
a Snapshot manager tool which captures in game content while playtesting for any purpose( examine content after playtesting or save it as a wallpaper and a Third Person (TP) camera which contains a variety of options allowing both
programmers and artists to adjust their work in their designated scene . These 3 assets come as a bundle ,but each one of them can work independently.

This triple bundle of gameplay tools has been mostly tested on 5.6.5f1 version of Unity. 
Earlier versions had been initially developed in Unity 5.0.0f4. The asset has also been tested and works through 2019.2 version.
It is strongly recommended to use it from version 5.6.5f1 and later.


Instructions: 
	How to import the package (Only Applicable if the tool kit comes in a custom package form / reuse case in multiple projects):
		Step 1: While in Unity Editor go to Assets/Import Package/Custom Package...(If the project has been exported as a package) or
			 While in Unity Editor go to File/Open Project and press "Open to find the location of the project in your computer. Note:If the          project is not exported as a package ignore Step 2.
 
		Step 2: Find the Asset Package which contains the files associated and import it. 
		
	Setup: 
		Test the Assets:
		Step 1: The following package contains an already prepared demo scene to test the asset's functionality and features.Navigate       
    through the scene folder within the asset folder "GameplayAssetTool" and load "SampleScene".
		(Important Note: Before loading demo scene or any other scene which includes these assets please check the project directory after 
    importing the package for a ".txt" file with the name "lastDirectorySaveTarget".)
			 
    If that file exists please remove any text ,save the file and exit afterwards.Do not attempt playtest with the snapshotViewer 
    content active if this isn't resolved properly.
		If the file doesn't exist you don't need to take further action ,it will automatically be created on your first playtest.)  
			
		Step 2: Navigate through Prefabs folder within the "GameplayAssetTool" folder and read the ".txt" file for each asset to help you 
    understand how to properly use it and all of their features.
		(Note 1: Each of these assets is independent to each other.)
		(Note 2: If you wish to use "snapshotViewer" as a standalone asset you need to have an active camera to your desired scene.)

		Step 3: The folder "PlayerPrefabs" contains a player controller with a ball. 		
		
Important Note: Read documentation texts in each of the specified folders containing the assets for further understanding and proper use(Note: Main Assets are located within the package's folder "Prefabs").
