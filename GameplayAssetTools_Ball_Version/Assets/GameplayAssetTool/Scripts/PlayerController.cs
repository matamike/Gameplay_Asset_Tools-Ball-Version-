using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour {
	public float speed=1f; //default speed value

	private Rigidbody playerRB; //player rigidbody component
	private GameObject playerCam; //active camera object reference
	private bool airborne=false; //flag for airborne state

	void Start(){
		playerRB = this.GetComponent<Rigidbody> (); //init rigidbody component of player
		playerCam = GameObject.FindGameObjectWithTag("DepthTest"); //init active camera object reference

	}


	void FixedUpdate(){
		//FORWARD MOVEMENT
		if (Input.GetKey (KeyCode.W) && (Mathf.Abs(playerRB.velocity.x)<=speed && Mathf.Abs(playerRB.velocity.z)<=speed)) {
			Vector3 direction = playerCam.transform.TransformDirection (playerCam.transform.position);
			playerRB.AddForce(new Vector3(direction.x*-1f,direction.y,direction.z*-1f)*Time.deltaTime);

			//Projectile Jump Forward
			if(Input.GetKeyDown(KeyCode.Space)){
				if(airborne==false)
					playerRB.velocity=new Vector3(direction.x*-1f,direction.y+(3f*Mathf.Sin(Mathf.PI/4f)*180f),direction.z*-1f)*Time.deltaTime;
			}
		}

		//BACKWARD MOVEMENT
		if (Input.GetKey (KeyCode.S) && (Mathf.Abs(playerRB.velocity.x)<=speed && Mathf.Abs(playerRB.velocity.z)<=speed)) {
			Vector3 direction = playerCam.transform.TransformDirection (playerCam.transform.position);
			playerRB.AddForce(new Vector3(direction.x,direction.y,direction.z)*Time.deltaTime);
		}

		//LEFT MOVEMENT
		if (Input.GetKey (KeyCode.A) && (Mathf.Abs(playerRB.velocity.x)<=speed && Mathf.Abs(playerRB.velocity.z)<=speed)) {
			Vector3 direction =  Vector3.MoveTowards(playerCam.transform.right*-1f*speed,new Vector3(playerCam.transform.right.x*-1f*speed,0f,0f),Time.deltaTime);
			if (direction.x != 0f) {
				playerRB.velocity += (direction  * Time.deltaTime);
			} 
		}

		//RIGHT MOVEMENT
		if (Input.GetKey (KeyCode.D) && (Mathf.Abs(playerRB.velocity.x)<=speed && Mathf.Abs(playerRB.velocity.z)<=speed)) {
			Vector3 direction =  Vector3.MoveTowards(playerCam.transform.right*speed,new Vector3(playerCam.transform.right.x*speed,0f,0f),Time.deltaTime);
			if (direction.x != 0f) {
				playerRB.velocity += (direction * Time.deltaTime);
			}
		}

		//JUMP
		if (Input.GetKeyDown (KeyCode.Space) && !Input.GetKey(KeyCode.W)) {
			Vector3 direction = new Vector3(0f,1f,0f)  * speed;
			if (airborne==false)
				playerRB.velocity = new Vector3(playerRB.velocity.x,direction.y,playerRB.velocity.z);
		}
	}

	void OnCollisionStay(Collision col){
		airborne = false;
	}

	void OnCollisionExit(Collision col){
		airborne = true;
	}
}
