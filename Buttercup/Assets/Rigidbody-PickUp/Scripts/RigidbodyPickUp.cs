using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Character/Rigidbody Pick-Up")]
public class RigidbodyPickUp : MonoBehaviour
{
    public string pickupButton = "Fire1"; //The name of the Pick Up button you are going to use.
    public bool togglePickUp = false; //Make picking up objects toggable.
    public float distance = 3f; //How far the object is being held away from you
    public float maxDistanceHeld = 4f; //How far the object can be held. If it goes past, it releases.
    public float maxDistanceGrab = 10f; //The maximum distance an object can be grabbed.
    public GameObject playerCam;//Camera of player

    private bool objectIsToggled = false; //Check if the object is held //PRIVATE
    private float togg_time = 0.5f; //A short timer for when the object be allowed to press again //PRIVATE
    private float timeHeld = 0.5f; // PRIVATE
    private Ray playerAim; //Vector3 of main camera's direction //PRIVATE
    public static GameObject objectHeld; // Object being held currently //PRIVATE
    public static bool isObjectHeld; // is the object being held? //PRIVATE
    private bool objectCan; // Can the object the player is looking at be held? //PRIVATE
    private float intTimeHeld; // PRIVATE

    private GameObject[] pickableObjs; /*Objects that PlayerGun can pick up  //PRIVATE
    !LEAVE BLANK, TAG OBJECTS WITH "Pickable" TO ADD OBJECTS TO THIS LIST!*/
    public Shader pickableShader;
    public bool autoSetTransparentShader = true;

    public physicsSub physicsMenu = new physicsSub(); //Brings the Physic settings into the Inspector
    public crosshairSystem crosshairsSystem = new crosshairSystem(); //Bring the Crosshair System into the Inspector
    public audioSoundsSub audioSystem = new audioSoundsSub(); //Brings the audio menu into the Inspector
    public objectAlphaSub objectHoldingOpacity = new objectAlphaSub(); //Bring the Object Alpha system into the Inspector
    public throwingSystemMenu throwingSystem = new throwingSystemMenu(); //Bring the Throwing System into the Inspector
    public rotationSystemSub rotationSystem = new rotationSystemSub(); //Bring the Rotation System into the Inspector
    public objectZoomSub zoomSystem = new objectZoomSub(); //Brings the Object Zoom System into the Inspector
    public objectFreezing objectFreeze = new objectFreezing();

    void Start()
    {
        //Set bools, objects, and floats to proper defaults.
        ResetPickUp(false);
        timeHeld = 0.5f;
        intTimeHeld = timeHeld;
        zoomSystem.intDistance = distance;
        zoomSystem.maxZoom = maxDistanceHeld - 0.7f;
        throwingSystem.defaultThrowTime = throwingSystem.throwTime;

        //Certain files are not selected in the inspector, will default to ones included if the systems are enabled. 
        //If the systems are not enabled on debug start, they will not load so keep that in mind.
        if(crosshairsSystem.enabled)
        {
            crosshairsSystem.crosshairTextures = new Texture2D[3];
            if(crosshairsSystem.crosshairTextures[0] == null)
            {
                crosshairsSystem.crosshairTextures[0] = Resources.Load<Texture2D>("Crosshair/crosshair128");
            }
            if(crosshairsSystem.crosshairTextures[1] == null)
            {
                crosshairsSystem.crosshairTextures[1] = Resources.Load<Texture2D>("Crosshair/crosshair_able");
            }
            if (crosshairsSystem.crosshairTextures[2] == null)
            {
                crosshairsSystem.crosshairTextures[2] = Resources.Load<Texture2D>("Crosshair/crosshair_grab");
            }
        }
        if(audioSystem.enabled)
        {
            if(audioSystem.pickedUpAudio == null)
            {
                audioSystem.pickedUpAudio = Resources.Load<AudioClip>("Audio/Rigid_PickUp");
            }
            if(audioSystem.objectHeldAudio == null)
            {
                audioSystem.objectHeldAudio = Resources.Load<AudioClip>("Audio/Rigid_Held");
            }
            if(audioSystem.throwAudio == null)
            {
                audioSystem.throwAudio = Resources.Load<AudioClip>("Audio/Rigid_Dropped");
            }
        }

        UpdatePickableShaders();
    }

    public void UpdatePickableShaders()
    {
        if (autoSetTransparentShader)
        {
            pickableObjs = GameObject.FindGameObjectsWithTag("Pickable");
            foreach (GameObject pickable in pickableObjs)
            {
                pickable.GetComponent<Renderer>().material.shader = pickableShader;
            }
        }
    }

    void LateUpdate()
    {
        if (isObjectHeld && physicsMenu.placeObjectBack)
        {
            if (Vector3.Distance(objectHeld.GetComponent<Placeback>().pos, objectHeld.transform.position) < physicsMenu.placeDistance)
            {
                physicsMenu.canPlaceBack = true;
            }
            else if (Vector3.Distance(objectHeld.GetComponent<Placeback>().pos, objectHeld.transform.position) > physicsMenu.placeDistance)
            {
                physicsMenu.canPlaceBack = false;
            }
        }

        //Check to see if the object held is deleted, if so, make it false.
        if (objectHeld == null && isObjectHeld)
        {
            isObjectHeld = false;
            if (togglePickUp)
            {
                togg_time = 0.5f;
                objectIsToggled = false;
            }
            if (zoomSystem.enabled)
            {
                distance = zoomSystem.intDistance;
            }
        }

        if ((physicsMenu.objectRotated && !isObjectHeld && !Input.GetButton(rotationSystem.rotateButton) && rotationSystem.enabled) &&
                                                            (physicsMenu.objectDirection == physicsSub.objectRotation.FaceForward ||
                                                             physicsMenu.objectDirection == physicsSub.objectRotation.TurnOnY ||
                                                             physicsMenu.objectDirection == physicsSub.objectRotation.None))
        {
            physicsMenu.objectRotated = false;

            /* FOR ADVANCED USERS:
                 * If you wish to add your own FPS controller, modify the 2 if statements to your own character class to disable them when rotating objects.
                 */
           // if (this.TryGetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>(out UnityStandardAssets.Characters.FirstPerson.FirstPersonController fps))
     //       {
    //            fps.enabled = true;
   //         }
   //     //    else if (this.TryGetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>(out UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController rfps))
  //         {
  //              rfps.enabled = true;
   //         }
        }
    }

    void Update()
    {
        if (rotationSystem.enabled)
        {
            if (Input.GetButton(rotationSystem.rotateButton) && isObjectHeld)
            {
                physicsMenu.objectRotated = true;
                objectHeld.GetComponent<Rigidbody>().freezeRotation = true;
                objectHeld.GetComponent<Rigidbody>().velocity = Vector3.zero;

                /* FOR ADVANCED USERS:
                 * If you wish to add your own FPS controller, modify the 2 if statements to your own character class to disable them when rotating objects.
                 */
       //         if (this.TryGetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>(out UnityStandardAssets.Characters.FirstPerson.FirstPersonController fps))
     //           {
       //             fps.enabled = !fps.enabled;
      //          }
      //          else if (this.TryGetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>(out UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController rfps))
     //           {
     //               rfps.enabled = !rfps.enabled;
    //            }

                switch (rotationSystem.lockRotationTo)
                {
                    case rotationSystemSub.lockingRotation.X:
                        objectHeld.transform.RotateAroundLocal(playerCam.transform.up, -Mathf.Deg2Rad * (rotationSystem.xRotationSpeed * Input.GetAxis("Mouse X")));
                        break;
                    case rotationSystemSub.lockingRotation.Y:
                        objectHeld.transform.RotateAroundLocal(playerCam.transform.right, Mathf.Deg2Rad * (rotationSystem.yRotationSpeed * Input.GetAxis("Mouse Y")));
                        break;
                    case rotationSystemSub.lockingRotation.None:
                        objectHeld.transform.RotateAroundLocal(playerCam.transform.up, -Mathf.Deg2Rad * (rotationSystem.xRotationSpeed * Input.GetAxis("Mouse X")));
                        objectHeld.transform.RotateAroundLocal(playerCam.transform.right, Mathf.Deg2Rad * (rotationSystem.yRotationSpeed * Input.GetAxis("Mouse Y")));
                        break;
                }
            }
            else if (!Input.GetButton(rotationSystem.rotateButton) && isObjectHeld)
            {
                objectHeld.GetComponent<Rigidbody>().freezeRotation = false;

                /* FOR ADVANCED USERS:
                 * If you wish to add your own FPS controller, modify the 2 if statements to your own character class to disable them when rotating objects.
                 */
              //  if (this.TryGetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>(out UnityStandardAssets.Characters.FirstPerson.FirstPersonController fps))
          //      {
         //           fps.enabled = true;
        //        }
        //        else if (this.TryGetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>(out UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController rfps))
        //        {
       //             rfps.enabled = true;
       //         }
            }
        }
    }
    void FixedUpdate()
    {
        //Crosshair Raycasting
        Ray playerAim = playerCam.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));
        RaycastHit hit;
        //Debug.DrawRay(playerAim.origin, playerAim.direction, Color.red);

        if (Physics.Raycast(playerAim, out hit, maxDistanceGrab - 1.5f))
        {
            objectCan = hit.transform.tag == "Pickable";
        }
        else
        {
            objectCan = false;
        }

        if (isObjectHeld && objectIsToggled)
        {
            holdObject();
            togg_time = togg_time - Time.deltaTime;
        }

        if (throwingSystem.enabled && objectHeld != null)
        {
            if (Input.GetButton(throwingSystem.throwButton) && !throwingSystem.throwing)
            {
                ThrowObject();
            }
        }

        if(throwingSystem.throwing)
        {
            throwingSystem.throwTime -= Time.deltaTime;
            if(throwingSystem.throwTime < 0)
            {
                throwingSystem.throwing = false;
                throwingSystem.throwTime = throwingSystem.defaultThrowTime;
                throwingSystem.obj = null;
            }
        }

        //Button toggles for Raycasting
        if (togglePickUp)
        {
            //Button toggles for Raycasting
            if (Input.GetButtonDown(pickupButton) && !throwingSystem.throwing && !isObjectHeld && !objectIsToggled && togg_time > 0.49f)
            {
                /*If no object is held, try to pick up an object.
                  works if you hold down the button as well.*/
                tryPickObject();
            }

            if (Input.GetButtonDown(pickupButton) && isObjectHeld && objectIsToggled && togg_time < 0)
            {
                if (physicsMenu.placeObjectBack && Vector3.Distance(objectHeld.GetComponent<Placeback>().pos, objectHeld.transform.position) < physicsMenu.placeDistance)
                {
                    objectHeld.transform.position = objectHeld.GetComponent<Placeback>().pos;
                    objectHeld.transform.rotation = objectHeld.GetComponent<Placeback>().rot;
                    ResetPickUp(true);
                    togg_time = 0.5f;
                }
                else
                {
                    ResetPickUp(true);
                    togg_time = 0.5f;
                }
            }
        }
        else if (!togglePickUp)
        {
            //Button toggles for Raycasting
            if (Input.GetButton(pickupButton) && !throwingSystem.throwing)
            {
                    if (!isObjectHeld)
                    /*If no object is held, try to pick up an object.
                      works if you hold down the button as well.*/
                    {
                        tryPickObject();
                    }
                    else if (isObjectHeld)
                    {
                        holdObject();
                    }
            }
            else if (!Input.GetButton(pickupButton) && isObjectHeld)
            {
                if (physicsMenu.placeObjectBack && Vector3.Distance(objectHeld.GetComponent<Placeback>().pos, objectHeld.transform.position) < physicsMenu.placeDistance)
                {
                    objectHeld.transform.position = objectHeld.GetComponent<Placeback>().pos;
                    objectHeld.transform.rotation = objectHeld.GetComponent<Placeback>().rot;
                    ResetPickUp(true);
                }
                ResetPickUp(true);
            }

        }

        //Object Rotation System
        if (objectFreeze.enabled)
        {
            if (Input.GetButtonDown(objectFreeze.freezeButton) && isObjectHeld)
            {
                objectFreeze.objectFrozen = true;
            }

            if (objectFreeze.objectFrozen && isObjectHeld)
            {
                objectHeld.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                ResetPickUp(false);
            }
            else if (!objectFreeze.objectFrozen && isObjectHeld)
            {
                objectHeld.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
        }



        //Object Alpha System
        if (isObjectHeld && objectHoldingOpacity.enabled)
        {
            Color alpha = objectHeld.GetComponent<Renderer>().material.color;
            alpha.a = objectHoldingOpacity.transparency;
            objectHeld.GetComponent<Renderer>().material.color = alpha;
            objectHoldingOpacity.alphaSet = true;
        }
    }

    //Will try to pick up the rigidbody in the 'pickableObjs' array.
    private void tryPickObject()
    {
        Ray playerAim = playerCam.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Physics.Raycast(playerAim, out hit);//Outputs the Raycast

        if (hit.collider.gameObject != null)
        {
            if (hit.collider.gameObject.tag == "Pickable" && Vector3.Distance(hit.collider.gameObject.transform.position, playerCam.transform.position) <= maxDistanceGrab)
            {
                isObjectHeld = true; //If object is successfully held, turn on bool
                objectHeld = hit.collider.gameObject; //Makes the object that got hit by the raycast go into the gun's objectHeld
                objectHeld.GetComponent<Rigidbody>().useGravity = false; //Disable gravity to fix a bug
                objectHeld.GetComponent<Rigidbody>().velocity = Vector3.zero;
                if (audioSystem.enabled)
                {
                    GetComponent<AudioSource>().PlayOneShot(audioSystem.pickedUpAudio);
                    audioSystem.letGoFired = false;
                }
                if (togglePickUp)
                {
                    objectIsToggled = true;
                }
                if (objectFreeze.enabled)
                {
                    objectFreeze.objectFrozen = false;
                }
                if (physicsMenu.placeObjectBack)
                {
                    if (objectHeld.GetComponent("Placeback") == null)
                    {
                        objectHeld.AddComponent<Placeback>();
                    }
                }
            }
        }
    }

    private void holdObject()
    {
        Ray playerAim = playerCam.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        /*Finds the next position for the object held to move to, depending on the Camera's position
        ,direction, and distance the object is held between you two.*/
        Vector3 nextPos = playerCam.transform.position + playerAim.direction * distance;
        //Takes the current position of the object held
        Vector3 currPos = objectHeld.transform.position;
        timeHeld = timeHeld - 0.1f * Time.deltaTime;

        if (audioSystem.enabled)
        {
            GetComponent<AudioSource>().PlayOneShot(audioSystem.objectHeldAudio);
        }

        /*Checking the distance between the player and the object held.
         * If the distance exceeds the 'maxDistanceHeld', it will let the object go. This also
         * stops a bug that forces objects through walls if you move back too far with an object held
         * maxDistanceGrab is how far you are able to grab an object, if it exceeds the amount, it won't do anything
         */
        if (Vector3.Distance(objectHeld.transform.position, playerCam.transform.position) > maxDistanceGrab || throwingSystem.throwing)
        {
            ResetPickUp(true);
        }

        //If an object is held, apply the object's placement.
        else if (isObjectHeld)
        {
            if (Vector3.Distance(objectHeld.transform.position, playerCam.transform.position) > maxDistanceHeld && timeHeld < 0)
            {
                ResetPickUp(true);
            }
            else
            {
                objectHeld.GetComponent<Rigidbody>().velocity = (nextPos - currPos) * 10;
                if(physicsMenu.keepRotation)
                {
                    physicsMenu.intRotation = objectHeld.transform.rotation;
                }
                if (!physicsMenu.objectRotated)
                {
                    switch (physicsMenu.objectDirection)
                    {
                        case physicsSub.objectRotation.TurnOnY:
                            objectHeld.transform.eulerAngles = new Vector3(0, playerCam.transform.eulerAngles.y, 0);
                            break;
                        case physicsSub.objectRotation.FaceForward:
                            objectHeld.transform.LookAt(playerCam.transform.position);
                            break;
                    }
                }

                if (distance < zoomSystem.minZoom)
                {
                    distance = zoomSystem.minZoom + 0.1f;
                }
                else if (distance > zoomSystem.maxZoom)
                {
                    distance = zoomSystem.maxZoom - 0.1f;
                }

                if (zoomSystem.enabled)
                {
                    if (zoomSystem.useAxis)
                    {
                        if (Input.GetAxis(zoomSystem.zoomAxisButton) > 0 && isObjectHeld)
                        {
                            distance = distance + Input.GetAxis(zoomSystem.zoomAxisButton);
                        }
                        else if (Input.GetAxis(zoomSystem.zoomAxisButton) < 0 && isObjectHeld)
                        {
                            distance = distance + Input.GetAxis(zoomSystem.zoomAxisButton);
                        }
                    }
                    else
                    {
                        if (Input.GetButton(zoomSystem.zoomInButton) && isObjectHeld)
                        {
                            distance = distance + 0.05f;
                        }
                        else if (Input.GetButton(zoomSystem.zoomOutButton) && isObjectHeld)
                        {
                            distance = distance - 0.05f;
                        }
                    }
                }
            }
        }
    }

    void OnGUI()
    {
        if (crosshairsSystem.enabled)
        {
            if (isObjectHeld) //Object Is Being Held Crosshair
            {
                GUI.DrawTexture(new Rect(Screen.width / 2 - (crosshairsSystem.crosshairTextures[2].width / 2), Screen.height / 2 - (crosshairsSystem.crosshairTextures[2].height / 2),
                                        crosshairsSystem.crosshairTextures[2].width,
                                        crosshairsSystem.crosshairTextures[2].height),
                                        crosshairsSystem.crosshairTextures[2]);

                //if (physicsMenu.placeObjectBack && physicsMenu.canPlaceBack)
                //{
                //    GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 20, 100, 20), "PLACE BACK");
                //}
            }
            else if (objectCan) //Object Can Be Held Crosshair
            {
                GUI.DrawTexture(new Rect(Screen.width / 2 - (crosshairsSystem.crosshairTextures[1].width / 2), Screen.height / 2 - (crosshairsSystem.crosshairTextures[1].height / 2),
                                        crosshairsSystem.crosshairTextures[1].width,
                                        crosshairsSystem.crosshairTextures[1].height),
                                        crosshairsSystem.crosshairTextures[1]);
            }
            else if (!isObjectHeld && !objectCan) //Default Crosshair
            {
                if (crosshairsSystem.crosshairTextures[0] == null)
                {
                    Debug.LogError("Crosshairs are null");
                }
                else
                {
                    GUI.DrawTexture(new Rect(Screen.width / 2 - (crosshairsSystem.crosshairTextures[0].width / 2), Screen.height / 2 - (crosshairsSystem.crosshairTextures[0].height / 2),
                                            crosshairsSystem.crosshairTextures[0].width,
                                            crosshairsSystem.crosshairTextures[0].height),
                                            crosshairsSystem.crosshairTextures[0]);
                }
            }
        }
    }

    private void ThrowObject()
    {
        throwingSystem.obj = objectHeld;
        throwingSystem.throwing = true;
        ResetPickUp(true);
        throwingSystem.obj.GetComponent<Rigidbody>().AddForce(playerCam.transform.forward * throwingSystem.strength);
        if(audioSystem.enabled)
        {
            GetComponent<AudioSource>().PlayOneShot(audioSystem.throwAudio);
        }
    }

    private void ResetPickUp(bool disableGravity)
    {
        if (disableGravity && isObjectHeld)
        {
            objectHeld.GetComponent<Rigidbody>().useGravity = true;
        }
        if (objectHoldingOpacity.alphaSet && objectHoldingOpacity.enabled)
        {
            Color alpha = objectHeld.GetComponent<Renderer>().material.color;
            alpha.a = 1f;
            objectHeld.GetComponent<Renderer>().material.color = alpha;
            objectHoldingOpacity.alphaSet = false;
        }
        if (objectHeld != null)
        {
            objectHeld.GetComponent<Rigidbody>().freezeRotation = false;
        }
        isObjectHeld = false;
        physicsMenu.canPlaceBack = false;
        objectHeld = null;
        timeHeld = intTimeHeld;
    }

}

[System.Serializable]
public class physicsSub
{
    public enum objectRotation { FaceForward, TurnOnY, None };
    public objectRotation objectDirection;
    public bool placeObjectBack = false;
    public float placeDistance = 3f;
    public bool keepRotation = false;
    [System.NonSerialized]
    public Quaternion intRotation;
    [System.NonSerialized]
    public bool objectRotated = false;
    [System.NonSerialized]
    public Quaternion objectRot;
    [System.NonSerialized]
    public Vector3 objectPos;
    [System.NonSerialized]
    public bool canPlaceBack = false;
}

[System.Serializable]
public class throwingSystemMenu //Throwing System
{
    public bool enabled = false;
    public string throwButton = "Fire2";
    public float strength = 100f;

    [System.NonSerialized]
    public GameObject obj = null;
    [System.NonSerialized]
    public bool throwing = false;
    [System.NonSerialized]
    public float throwTime = 1f;
    [System.NonSerialized]
    public float defaultThrowTime;
}

[System.Serializable]
public class crosshairSystem //Crosshair System - You are no longer required to just remove the code, just disable it from the inspector!
{
    public bool enabled = true;

    public Texture2D[] crosshairTextures; //Array of textures to use for the crosshair
    //0 = default | 1 = Object can be held | 2 = Object is being held currently
}

[System.Serializable]
public class rotationSystemSub
{
    public bool enabled = false;

    public string rotateButton = "Rotate"; //A Input name from the Input Manager.
    public float xRotationSpeed = 5; //Rotation speed of the Rigidbody
    public float yRotationSpeed = 5; //Rotation speed of the Rigidbody
    public enum lockingRotation { None, X, Y };
    public lockingRotation lockRotationTo;
    //Change "MouseLook" to your own Mouse Script name. The one currently used is from the default FPS controller package.
    //public MouseLook[] mouseScripts;
    
    //public RigidbodyFirstPersonController[] mouseScripts;
    [System.NonSerialized]
    public float rotY = 0F;
}

[System.Serializable]
public class objectAlphaSub
{
    public bool enabled = true;

    public float transparency = 0.5f;
    [System.NonSerialized]
    public bool alphaSet = false;
}

[System.Serializable]
public class audioSoundsSub
{
    public bool enabled = true;

    public AudioClip pickedUpAudio;
    public AudioClip objectHeldAudio;
    public AudioClip throwAudio;
    [System.NonSerialized]
    public bool letGoFired = false;
}

[System.Serializable]
public class objectZoomSub
{
    public bool enabled = false;

    public string zoomInButton;
    public string zoomOutButton;
    public bool useAxis = true; //if true, it will use the Axis Button for both zooming in and out. If alse, will use the two buttons instead
    public string zoomAxisButton = "Mouse ScrollWheel";
    public float minZoom = 1.5f; //Set the minimum amount for how close the object can be held. Will use maxDistanceHeld for maximum distance your object can be held.
    [System.NonSerialized]
    public float maxZoom; //Leave default to allow maxDistanceHeld to use the variable.
    [System.NonSerialized]
    public float intDistance;
}

[System.Serializable]
public class objectFreezing
{
    public bool enabled = false;

    public string freezeButton = "Freeze"; //Set a custom button from the Input Manager to whatever you choose. I made a "Freeze" and set it to "r"
    [System.NonSerialized]
    public bool objectFrozen = false;
}