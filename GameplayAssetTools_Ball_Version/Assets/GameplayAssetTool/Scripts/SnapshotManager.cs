using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

public class SnapshotManager : MonoBehaviour {
	private GameObject projector; //Main UI Panel
	private GameObject projectorText; //Main UI Panel Label
	private GameObject imgSequencer; //Seconday UI Panel 
	private GameObject inputSeq; //Instruction Info UI
	private GameObject renderCam; //Camera where snapshots rendered

	private int numOfCaptures=0; //last number increment of snapshots(Used in Capture Frame Case)
	private int snapshotCounter=0;//pointer of starting snap (Used to browse Left/Right the Image Canvas)
	private int maxSnapNum; //last incremented file found (with or without correction of order)
	private int tempMaxSnapNum; //used to renew the data found(Images) in folder Larger or Smaller Sum.
	private int lastSnapFound; //last index of image file found 
	private int timesPressedEsc; //escape flag 
	private int imagesFoundinDirectory=0; //new sum of images found in directory 
	private int missingSnapNum=0; //new sum of images NOT found in directory
	private int canvasSnapCounter; //init count in temp sequencer of snapshots.
	private int tempSaveSnapNum=0; //capacity adjust for list<Tex2d>
	private string newPath; //temporal variable storing the new specified snapshot directory.(Note:Any Older Images in different directory will not be affected/different save files).
	private int[] imageSlotsArr; //pos holder for ordered images

	private string savedSnapshotData; //reader of sum of images
	private bool takeSnap=false; // take snaphot flag
	private Texture2D tempTex; //tex2d holder 
	private List<int> imageSlots = new List<int> (); // images found in directory holder
	AsyncOperation saveSnap;
	AsyncOperation leftBrowse;
	AsyncOperation rightBrowse;

	[Tooltip("Contains Temporal Screenshots taken from the (SnapshotViewer) tool.")]
	public List<Texture2D> tempSnapsHolder; //place holder for tempsnaps
	[Tooltip("Path used for storing the snapshots. (Note: Initialize if null or empty with the desired default path.)")]
	public string path; //path folder to project.
			
	void Start () {
		//check for the 1st run if file not exist
		if (!File.Exists ("lastDirectorySaveTarget.txt")){
			path = Path.GetFullPath (Directory.GetCurrentDirectory ());
			File.WriteAllText ("lastDirectorySaveTarget.txt", path);
		}
		else {//opens existing file to get last saved directory preference.
			path = File.ReadAllText ("lastDirectorySaveTarget.txt");

			//save folder path error check (null or blank path)
			if ((path == null || path == "") || !Directory.Exists(path)) {
				path = Path.GetFullPath (Directory.GetCurrentDirectory ());
				File.WriteAllText ("lastDirectorySaveTarget.txt", path);
			}
		}

		//Create file if not exist (Including different prefered paths.)
		if (!File.Exists (path+"/"+"screenshotNumber.txt")) {
			File.WriteAllText(path+"/"+"screenshotNumber.txt","0"); //creates the file for storing the number of snapshots in folder.
		}

		projector= GameObject.FindGameObjectWithTag("Projector");//snapshot holder.
		projector.SetActive (false);
		projectorText = GameObject.FindGameObjectWithTag ("Label");
		projectorText.SetActive (false);
	    imgSequencer = GameObject.FindGameObjectWithTag ("Sequencer");
		imgSequencer.SetActive (false);
		inputSeq = GameObject.FindGameObjectWithTag ("InputSeq");
		inputSeq.SetActive (false);
		timesPressedEsc = 0;
		renderCam = GameObject.FindGameObjectWithTag ("DepthTest");
		tempSnapsHolder.Capacity = tempSaveSnapNum + 1;
		canvasSnapCounter = 0;
		newPath = path;
		StartCoroutine(ValidationCheck ());//validate images in file.
	}

	void Update () {
		//screenshot capture
		if(Input.GetKeyDown(KeyCode.P) && projector.activeInHierarchy==false){
			savedSnapshotData =File.ReadAllText(path+"/"+"screenshotNumber.txt");
			maxSnapNum=int.Parse(savedSnapshotData);
			numOfCaptures=maxSnapNum+1;
			bool fileChecker=File.Exists(path+"/"+"ScreenshotUnity"+numOfCaptures+".png");
			if(fileChecker==false && numOfCaptures>maxSnapNum){
					takeSnap=true;
					if(takeSnap==true){
						StartCoroutine(TakeSnap()); //save temporal snaps in array
						tempTex=null; //dump active texture after each entry
					}
			}
			else{
				numOfCaptures++;
			}
		}

		// open image browser(sceenshot manager)
		if (Input.GetKeyDown(KeyCode.I)) {
			projector.SetActive(true);//activate viewing window
			projectorText.SetActive(true); //activate text
			imgSequencer.SetActive(true); //activate image sequencer
			//Create file if not exist (Including different prefered paths.)
			if (!File.Exists (path+"/"+"screenshotNumber.txt")) {
				File.WriteAllText(path+"/"+"screenshotNumber.txt","0"); //creates the file for storing the number of snapshots in folder.
			}
			StartCoroutine(RefreshImages());
			Time.timeScale = 0f;//slow motion while in snapshot menu.
		}

		//close snap menu
		if (Input.GetKeyDown(KeyCode.Escape)) {
			timesPressedEsc=+1;
			if(timesPressedEsc==1 && imgSequencer.GetComponent<RawImage>().color.a==1f){
				imgSequencer.GetComponent<RawImage>().color= new Color32(255,255,255,0);
				inputSeq.SetActive(false);
				timesPressedEsc=0;
				canvasSnapCounter=0;
				imgSequencer.GetComponent<RawImage>().texture=null;
			}
			if(timesPressedEsc==1 && imgSequencer.GetComponent<RawImage>().color.a==0){
				projector.SetActive(false);
				projectorText.SetActive(false);
				imgSequencer.GetComponent<RawImage>().canvasRenderer.Clear();
				imgSequencer.SetActive(false);
				timesPressedEsc=0;
				Time.timeScale = 1f; //return time scale back to normal.
			}
		}

		//move left the snap album
		if(Input.GetKeyDown(KeyCode.K) && projector.activeInHierarchy==true){
			leftBrowse = BrowseLeft ();
			leftBrowse = null;
		}

		//move right the snap album
		if(Input.GetKeyDown(KeyCode.L) && projector.activeInHierarchy==true){ 
			rightBrowse = BrowseRight ();
			rightBrowse = null;
		}

		//open temporal snapshot holder to view images.
		if (Input.GetKeyDown(KeyCode.F1) && projector.activeInHierarchy==true) {
			imgSequencer.GetComponent<RawImage>().color= new Color32(255,255,255,255); //activate transparrent layer to work in here
			inputSeq.SetActive(true); //activate inputSeq
		}

		// temporal image selector from current screenshot session collection (Left Orientation Browsing)
		if(Input.GetKeyDown(KeyCode.F3) && inputSeq.activeInHierarchy==true){
			canvasSnapCounter-=1;
			if(canvasSnapCounter<0){
				canvasSnapCounter=0;
			}
			if(canvasSnapCounter>=0 && tempSnapsHolder.Count!=0){
				imgSequencer.GetComponent<RawImage>().texture=tempSnapsHolder[canvasSnapCounter];
			}
		}

		// temporal image selector from current screenshot session collection (Right Orientation Browsing)
		if(Input.GetKeyDown(KeyCode.F4) && inputSeq.activeInHierarchy==true){
			canvasSnapCounter+=1;
			if(canvasSnapCounter>=(tempSnapsHolder.Capacity)){
				canvasSnapCounter=tempSnapsHolder.Capacity-1;
			}
			if(canvasSnapCounter<(tempSnapsHolder.Capacity) && tempSnapsHolder.Count!=0){
				imgSequencer.GetComponent<RawImage>().texture=tempSnapsHolder[canvasSnapCounter];
			}
		}

		//save temporal session active snapshot
		if (Input.GetKeyDown (KeyCode.X) && inputSeq.activeInHierarchy==true && tempSnapsHolder.Count>0) {
			saveSnap = SaveImage (); 
		}

		//delete active imageSequencer element on demand
		if (Input.GetKeyDown (KeyCode.Delete) && inputSeq.activeInHierarchy==true && tempSnapsHolder.Count>0) { 
			tempSnapsHolder.Remove(tempSnapsHolder[canvasSnapCounter]); //remove current screenshot from list.
			tempSaveSnapNum--; //decrement list count
			tempSnapsHolder.Capacity=tempSaveSnapNum; //apply new capacity
		}
	}

	//Validation of images in File
	IEnumerator ValidationCheck(){
		yield return new WaitForEndOfFrame ();
		//init validation check data onLoad
		savedSnapshotData =File.ReadAllText(path+"/"+"screenshotNumber.txt"); //read sum of images last found.
		maxSnapNum=int.Parse(savedSnapshotData); //parse to int.
		for(int i=1; i<=maxSnapNum; i++){
			if((File.Exists(path+"/"+"ScreenshotUnity"+i+".png"))){
				imageSlots.Insert(imagesFoundinDirectory,i);
				imagesFoundinDirectory+=1;
			}
		}
		missingSnapNum = maxSnapNum - imagesFoundinDirectory;//corrected sum number
		imageSlotsArr = imageSlots.ToArray ();
		
		//replace missing images slots with larger increment numbers	
		if (missingSnapNum!=0) {
			for (int j=0; j<imagesFoundinDirectory; j++) { //gets old number of snaps
				if(imageSlotsArr[j]!=j+1){
					if(File.Exists(path+"/"+"ScreenshotUnity"+imageSlotsArr[j]+".png")){
						byte[] arrDec=File.ReadAllBytes(path+"/"+"ScreenshotUnity"+imageSlotsArr[j]+".png");
						byte[] newArrDec=arrDec;
						File.WriteAllBytes(path+"/"+"ScreenshotUnity"+(j+1)+".png",newArrDec);
						File.Delete(path+"/"+"ScreenshotUnity"+imageSlotsArr[j]+".png");
					}
				}
			}
			savedSnapshotData=imagesFoundinDirectory.ToString();
			File.WriteAllText(path+"/"+"screenshotNumber.txt",savedSnapshotData);
		}
	}

	//Refresh Image Changes in File while playtesting
	IEnumerator RefreshImages(){
		yield return new WaitForEndOfFrame ();
		savedSnapshotData =File.ReadAllText(path+"/"+"screenshotNumber.txt");
		tempMaxSnapNum= int.Parse(savedSnapshotData);
		lastSnapFound = 0; //init state.
		int tmp=0; //check screenshot number of images
		int i=1; //loop counter
		do{ //verify the file of saved screenshots
			if(File.Exists(path+"/"+"ScreenshotUnity"+i+".png")){
				tmp+=1; //sum of screenshots
				lastSnapFound=i; //last screenshot number found based on file record
			}
			i+=1;
		}while(i<=tempMaxSnapNum);
		savedSnapshotData=lastSnapFound.ToString(); //save the new last snap found
		File.WriteAllText(path+"/"+"screenshotNumber.txt",savedSnapshotData); //overwrite data to file of last snapshot found\
	}

	//Capture Snap
	IEnumerator TakeSnap(){
		yield return new WaitForEndOfFrame();
		RenderTexture tempRend = new RenderTexture (Screen.width, Screen.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
		renderCam.GetComponent<Camera>().Render();
		tempTex = new Texture2D (tempRend.width,tempRend.height); //set screenshot placeholder to get data
		tempTex.ReadPixels(new Rect(0,0,tempTex.width,tempTex.height),0,0);
		tempTex.Apply();
		tempSnapsHolder.Add(tempTex);
		tempSaveSnapNum++;
		tempSnapsHolder.Capacity = tempSaveSnapNum;
		takeSnap=false;
		RenderTexture.active=null;
	}

	//Save Image to File
	AsyncOperation SaveImage(){
		string imageSumInFileTemp=File.ReadAllText(path+"/"+"screenshotNumber.txt");
		int temp=int.Parse(imageSumInFileTemp)+1;//increment image sum in File.
		byte[] tempImageData; //temp bytestream variable
		Texture2D imageToSave = new Texture2D(Screen.width,Screen.height); //temp Texture data to save active slot image.

		imageToSave=tempSnapsHolder[canvasSnapCounter]; //temp Save slot.
		tempImageData=imageToSave.EncodeToPNG(); //bytestream encoding.
		File.WriteAllBytes(path+"/"+"ScreenshotUnity"+temp+".png",tempImageData); //save image data
		File.WriteAllText(path+"/"+"screenshotNumber.txt",temp.ToString()); //save sum of images data
		tempSnapsHolder.Remove(imageToSave); //remove selected saved element from list
		tempSaveSnapNum--; //decrement list count
		tempSnapsHolder.Capacity=tempSaveSnapNum; //apply new capacity
		imageToSave=null; //dump temp tex2d
		tempImageData=null; //dump temp bytestream
		return saveSnap;
	}

	//Browse Photo Album Right to Left
	AsyncOperation BrowseLeft(){
		snapshotCounter-=1;
		if(snapshotCounter<1)
			snapshotCounter=1;
		if(File.Exists(path+"/"+"ScreenshotUnity"+snapshotCounter+".png")){
			byte[] arrPl=File.ReadAllBytes(path+"/"+"ScreenshotUnity"+snapshotCounter+".png");//read image as a bytestream
			Texture2D texPl= new Texture2D(200,200); //create 2DTexture
			if(texPl.LoadImage(arrPl)){ //load bytestream into 2DTexture
				projector.GetComponent<RawImage>().texture=texPl; //assign snapshot texture
				projectorText.GetComponent<Text>().text="Snap("+snapshotCounter+")";
			}
		}
		else{
			if(snapshotCounter>1)
				snapshotCounter-=1;
		}

		return leftBrowse;
	}

	//Browse Photo Album Left to Right
	AsyncOperation BrowseRight(){
		snapshotCounter+=1;
		if(snapshotCounter<1)
			snapshotCounter=1;
		if(snapshotCounter>lastSnapFound)
			snapshotCounter=lastSnapFound;
		if(File.Exists(path+"/"+"ScreenshotUnity"+snapshotCounter+".png")){
			byte[] arrDec=File.ReadAllBytes(path+"/"+"ScreenshotUnity"+snapshotCounter+".png");//read image as a bytestream
			Texture2D texDec= new Texture2D(200,200); //create 2DTexture
			if (texDec.LoadImage (arrDec)) { //load bytestream into 2DTexture
				projector.GetComponent<RawImage> ().texture = texDec; //assign snapshot texture
				projectorText.GetComponent<Text> ().text = "Snap(" + snapshotCounter + ")";
			}
		}
		else{
			if(snapshotCounter<lastSnapFound)
				snapshotCounter+=1;
		}

		return rightBrowse;
	}

	void SavePath(string path){
		if(!Directory.Exists(path)){
			path = Path.GetFullPath (Directory.GetCurrentDirectory ()); //default path directory.
		}
		if (!File.Exists ("lastDirectorySaveTarget.txt")) { //checks if file for directory saves exists.
			File.WriteAllText("lastDirectorySaveTarget.txt",path); //creates the file for storing the last chosen directory save path.

		}
		if (File.ReadAllText ("lastDirectorySaveTarget.txt")!=path) { //checks if path directory has changed 
			File.WriteAllText("lastDirectorySaveTarget.txt",path); //update directory path for save.
		}
	}

	//Select Folder 
	void  OnGUI(){
		if(projector.gameObject.activeSelf==true){
			if (GUILayout.Button ("Select Folder")) {
				newPath = EditorUtility.OpenFolderPanel (path,path,"");//open file browser dialog and set or keep current directory
				SavePath(newPath); //init save path.
				//update directory if changed
				if (path != newPath && (newPath != "" && newPath != null)) {
					path = newPath;
					if (!File.Exists (path+"/"+"screenshotNumber.txt")) {
						File.WriteAllText(path+"/"+"screenshotNumber.txt","0"); //creates the file for storing the number of snapshots in folder.
					}
					StartCoroutine(RefreshImages());//sync images to new preference.
					snapshotCounter=1; //reset image pointer loaded to starting directory image.
				}
				else { //keep last preference as directory for target save
					path = Path.GetFullPath (path); 
				}
			}
		}
	}

}