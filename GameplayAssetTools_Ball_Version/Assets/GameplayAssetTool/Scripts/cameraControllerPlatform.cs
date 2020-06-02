using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Camera))]

public class cameraControllerPlatform : MonoBehaviour {
	private GameObject anchor; //point of interest
	private GameObject player; //player target
	private GameObject snpsManager; //project ui

	public Ray ray;
	public RaycastHit hitInfo;
	private LineRenderer linRend;

	private bool needsRepos;

	private int timesPressedKey=3;//init camera position
	private int distanceSteps=3; //step counter switch to reset (max steps 3)

	private float dynamicDistanceFromPlayerStepping=6f; //switch between 2,4,6 meters distance modes
	private float fixedDistanceFromPlayer=6f; //fixed distance position
	private float followSpeedAmplifier=1f; //balance boost amplifier.

	//preferences over inspector
	[Tooltip("Switch Mouse X Axis Rotation On/Off")]
	public bool isMouseXRotating=true;
	[Tooltip("Switch Mouse Y Axis Rotation On/Off")]
	public bool isMouseYRotating=true;
	[Tooltip("When (true) press V to switch between camera modes. When(false) use the scroll wheel to Zoom In/Out.")]
	public bool isFixedDistanceFromPlayer=true;
	[Tooltip("Used only when the property (isFixedDistanceFromPlayer) is (false) to specify max (Zoom Out) distance.")]
	public float freeCamThresholdMax=6f; //init sample position 
	[Tooltip("Enables/Disables Camera Reposition on Focus Target Loss feature.")]
	public bool focusOnTargetLoss = true;
	[Tooltip("Set the X Axis mouse speed.")]
	public float cameraMouseXSpeed=1f;
	[Tooltip("Set the Y Axis mouse speed.")]
	public float cameraMouseYSpeed=1f;


	void Awake(){
		//OPTIONAL
		Application.targetFrameRate = 60; //try framerate target 60
		Application.runInBackground = true; 
	}

	void Start () {
		needsRepos = false;
		player = GameObject.FindGameObjectWithTag("Player"); //player
		anchor = GameObject.FindGameObjectWithTag("Anchor"); //anchor
		linRend = this.GetComponent<LineRenderer> ();
	}

	void Update () {
		
		if (GameObject.FindGameObjectWithTag ("Projector") != null) { //OPTIONAL - USE ONLY IF HAVING PAUSE MENU OR ANOTHER GUI TYPE OF POP UP.
			snpsManager = GameObject.FindGameObjectWithTag ("Projector");
		} 
		else {
			snpsManager=null;
		}

		//distance limiter validation check.
		if ((fixedDistanceFromPlayer > 6f || fixedDistanceFromPlayer < 1f) && isFixedDistanceFromPlayer==true) { 
			if(fixedDistanceFromPlayer>6f)
				fixedDistanceFromPlayer=6f;
			if(fixedDistanceFromPlayer<1f)
				fixedDistanceFromPlayer=1f;
		}

		if ((transform.position.y < player.transform.position.y) && needsRepos==false) {
			transform.position=new Vector3(transform.position.x,(Mathf.Lerp (transform.position.y, player.transform.position.y, 1f)),transform.position.z);
		}
		anchor.transform.position =new Vector3(player.transform.position.x,player.transform.position.y+(2f-(2f/fixedDistanceFromPlayer)),player.transform.position.z); //anchor pos
		this.transform.LookAt (new Vector3(anchor.transform.position.x*followSpeedAmplifier,anchor.transform.position.y,anchor.transform.position.z*followSpeedAmplifier)); //depthTest node looks anchor.

		if (Mathf.Abs (player.GetComponent<Rigidbody> ().velocity.x) > Mathf.Abs (player.GetComponent<Rigidbody> ().velocity.z)) {
			followSpeedAmplifier = Mathf.Abs(player.GetComponent<Rigidbody> ().velocity.x);
		} 
		else {
			followSpeedAmplifier = Mathf.Abs(player.GetComponent<Rigidbody> ().velocity.z);
		}

		//prepare parameters for fixed Distance Mode /Reset
		if (isFixedDistanceFromPlayer == true) {
			freeCamThresholdMax=6f;
		}

		//Camera Distance Switch Mode
		if(Input.GetKeyDown(KeyCode.V) && snpsManager==null && isFixedDistanceFromPlayer==true){ //Camera distance mode switch with fixed distance mode
			timesPressedKey+=1; //increment
			if(timesPressedKey>=1 && timesPressedKey<=distanceSteps){//switch
				dynamicDistanceFromPlayerStepping+=2f;
				fixedDistanceFromPlayer=dynamicDistanceFromPlayerStepping;
			}
			if(timesPressedKey>distanceSteps){//reset
				timesPressedKey=1;
				dynamicDistanceFromPlayerStepping=2f;
				fixedDistanceFromPlayer=dynamicDistanceFromPlayerStepping;
			}
		}

		//Camera distance switch with free distance mode (zoom in)
		if (Input.GetAxis ("Mouse ScrollWheel")>0f && snpsManager == null && isFixedDistanceFromPlayer == false) {
			if (fixedDistanceFromPlayer > 2) {
				dynamicDistanceFromPlayerStepping=fixedDistanceFromPlayer; //updates in case of manual change.
				dynamicDistanceFromPlayerStepping-=1f;
				fixedDistanceFromPlayer=dynamicDistanceFromPlayerStepping;
			}
		}

		//Camera distance switch with free distance mode (zoom out)
		if (Input.GetAxis("Mouse ScrollWheel")<0f && snpsManager == null && isFixedDistanceFromPlayer == false) { 
			if(fixedDistanceFromPlayer<freeCamThresholdMax){
				dynamicDistanceFromPlayerStepping+=1f;
				fixedDistanceFromPlayer=dynamicDistanceFromPlayerStepping;
			}
		}

		//Camera distance switch with free distance mode (idle)
		if (Input.GetAxis("Mouse ScrollWheel")==0f && snpsManager == null && isFixedDistanceFromPlayer == false) { 
			this.transform.position=transform.position; // maintain position on Idle
		}


		//Mouse Handling 
		if (Input.mousePresent) { // if mouse exists
			float mouseXAxis=Input.GetAxis("Mouse X"); //Mouse X position axis
			float mouseYAxis=Input.GetAxis("Mouse Y"); //Mouse Y position axis

			//idle mouse position X or Y Axis
			if(mouseXAxis==0 || mouseYAxis==0){
				this.transform.eulerAngles=new Vector3(this.transform.eulerAngles.x,this.transform.eulerAngles.y,this.transform.eulerAngles.z);
			}

            //rotate left arround
            if (Input.GetAxis("Mouse X") > 0f && snpsManager == null && isMouseXRotating == true)
            {
                if (Mathf.Abs(mouseXAxis) > Mathf.Abs(mouseYAxis))
                {
                    transform.RotateAround(anchor.transform.position, Vector3.up, 4f + (cameraMouseXSpeed * fixedDistanceFromPlayer * Time.deltaTime));
                }
            }

            //rotate right arround 
            if (Input.GetAxis("Mouse X") < 0f && snpsManager == null && isMouseXRotating == true)
            {
                if (Mathf.Abs(mouseXAxis) > Mathf.Abs(mouseYAxis))
                {
                    transform.RotateAround(anchor.transform.position, Vector3.down, 4f + (cameraMouseXSpeed * fixedDistanceFromPlayer * Time.deltaTime));
                }
            }

            //rotate front to back
            if (Input.GetAxis("Mouse Y") > 0f && snpsManager == null && isMouseYRotating == true)
            {
                if ((this.transform.position.y < (anchor.transform.position.y + (fixedDistanceFromPlayer / 2f))) && (Mathf.Abs(mouseYAxis) > Mathf.Abs(mouseXAxis)))
                {
                    transform.Translate(new Vector3(0f, cameraMouseYSpeed * mouseYAxis * fixedDistanceFromPlayer * Time.deltaTime, 0f), Space.World);
                }
                transform.LookAt(anchor.transform.position); //always look at anchor object
            }

            //rotate back to front
            if (Input.GetAxis("Mouse Y") < 0f && snpsManager == null && isMouseYRotating == true)
            {
                if ((this.transform.position.y > (anchor.transform.position.y - (fixedDistanceFromPlayer / 4.5f))) && (Mathf.Abs(mouseYAxis) > Mathf.Abs(mouseXAxis)))
                {
                    transform.Translate(new Vector3(0f, cameraMouseYSpeed * mouseYAxis * fixedDistanceFromPlayer * Time.deltaTime, 0f), Space.World);
                }
                transform.LookAt(anchor.transform.position); //always look at anchor object
            }
        }
    }

	void LateUpdate(){
		//anchor sync with player 
		if (Mathf.Abs(anchor.transform.position.y)>0f) {
			anchor.transform.position = new Vector3 (player.transform.position.x, player.transform.position.y +(2f-(2f/fixedDistanceFromPlayer)), player.transform.position.z); //jump normalized position
			transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z); //maintain position
			transform.LookAt (anchor.transform.position);
		}

		//camera follow (Behind Player)
		if (Vector3.Distance (transform.position, anchor.transform.position)>fixedDistanceFromPlayer && needsRepos==false) { //distance check from anchor
			if (Vector3.Distance (transform.position, player.transform.position) > fixedDistanceFromPlayer){
				float speedDistance = Math.Abs (Vector3.Distance (transform.position, player.transform.position) - fixedDistanceFromPlayer);
				transform.Translate(Vector3.forward*(speedDistance*followSpeedAmplifier)*Time.deltaTime);
			}
		}

		//camera follow (Front of Player)
		if (Vector3.Distance (transform.position, anchor.transform.position)<fixedDistanceFromPlayer && needsRepos==false) { //distance check from anchor
			if(Vector3.Distance(transform.position,player.transform.position) <= fixedDistanceFromPlayer){
				float speedDistance = Math.Abs (Vector3.Distance (transform.position, player.transform.position) - fixedDistanceFromPlayer);
				transform.Translate(Vector3.back*(speedDistance*followSpeedAmplifier)*Time.deltaTime);
			}
		}
	}

	void FixedUpdate(){
		//set line renderer ready to use when the feature is active.
		if(focusOnTargetLoss==true){
			linRend.transform.TransformDirection(Vector3.forward); //line renderer component aims forward the ray.
			linRend.SetPosition (0, new Vector3(this.transform.position.x,this.transform.position.y-0.2f,this.transform.position.z));//origin
			linRend.SetPosition (1, anchor.transform.position);//target
		}
		//line cast
		if (Physics.Linecast (new Vector3(this.transform.position.x,this.transform.position.y-0.2f,this.transform.position.z), player.transform.position, out hitInfo) && focusOnTargetLoss==true) {
			if(hitInfo.collider.gameObject.tag=="Barrier" || hitInfo.collider.gameObject.tag=="Terrain"){
				needsRepos=true;
			}
			else{
				needsRepos=false;
			}
		}

		//line cast case scenario (Focus On Camera Target Loss /PathFinding Target)
		try{
			if (needsRepos == true) {
				if(hitInfo.distance>=0.2f && hitInfo.collider.tag=="Barrier"){ //Barrier Scenario reposition
					transform.position=Vector3.MoveTowards(transform.position,player.transform.position,(fixedDistanceFromPlayer/2f)*0.2f);
					transform.eulerAngles=new Vector3(hitInfo.normal.x,Vector3.Angle(this.transform.eulerAngles,new Vector3(hitInfo.normal.x,anchor.transform.eulerAngles.y,hitInfo.normal.z)),hitInfo.normal.z);
					if(player.transform.position.y>hitInfo.collider.gameObject.transform.position.y)
						needsRepos=false;
				}
				if(hitInfo.distance>=0.2f && hitInfo.collider.tag=="Terrain"){ //Terrain Scenario reposition
					transform.position= this.transform.position;
					transform.eulerAngles=new Vector3(0f,Vector3.Angle(this.transform.eulerAngles,new Vector3(0f,anchor.transform.eulerAngles.y,0f)),0f);
					if(player.transform.position.y>hitInfo.collider.gameObject.transform.position.y)
						needsRepos=false;
				}
				if(Vector3.Distance(this.transform.position,player.transform.position)>fixedDistanceFromPlayer && needsRepos==true)
					needsRepos=false;
			}
		}
		catch(NullReferenceException e){ //Runtime Error scenario in case of fast motion or unexpected angle orientations
			e.Data.Clear ();
			needsRepos=false;
		}
	}
}
