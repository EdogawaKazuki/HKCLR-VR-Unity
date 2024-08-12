using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Controller : MonoBehaviour
{
    public float speed = 10.0f;
    public float rotationSpeed = 100.0f;
    private float js_linear = 0.0f;
    private float js_angular = 0.0f;
    private XboxControllerInput XboxControllerInput;
    private RobotUDPClient RobotUDPClient;
    private bool usingVR = true;

    // Start is called before the first frame update
    void Start()
    {
        if(!usingVR) XboxControllerInput = GetComponent<XboxControllerInput>();
        RobotUDPClient = GetComponent<RobotUDPClient>();
        
    }

    // Update is called once per frame
    void Update()
    {
        // Update Object
        // if(XboxControllerInput.leftAnalogStickHorizontal != 0)
        // {
        //     transform.Translate(0, 0, XboxControllerInput.leftAnalogStickHorizontal * speed * Time.deltaTime);
        // }
        // if(XboxControllerInput.leftAnalogStickVertical != 0)
        // {
        //     transform.Translate(XboxControllerInput.leftAnalogStickVertical * speed * Time.deltaTime, 0, 0);
        // }
        // if(XboxControllerInput.rightAnalogStickHorizontal != 0)
        // {
        //     transform.RotateAround(transform.position, Vector3.up, XboxControllerInput.rightAnalogStickHorizontal * rotationSpeed * Time.deltaTime);
        // }
        // if(XboxControllerInput.rightAnalogStickVertical != 0)
        // {
        //     transform.RotateAround(transform.position, Vector3.right, XboxControllerInput.rightAnalogStickVertical * rotationSpeed * Time.deltaTime);
        // }

        // send cmd to robot
        if(usingVR){
            OVRInput.Update();
            // Debug.Log("Using VR");
            // Debug.Log("Button One: " + OVRInput.Get(OVRInput.Button.One));
            if(OVRInput.Get(OVRInput.Button.One)){
                // Debug.Log("Button One");
                js_angular = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
                js_linear = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
                if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) != 0)
                {
                    js_linear = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
                }
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) != 0)
                {
                    js_linear = -OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
                }
            }else{
                js_linear = 0.0f;
                js_angular = 0.0f;
            }
        }
        else{
            if(XboxControllerInput.leftBumper == true)
            {
                if(XboxControllerInput.leftAnalogStickHorizontal != 0)
                {
                    js_angular = XboxControllerInput.leftAnalogStickHorizontal;
                }
                if(XboxControllerInput.leftAnalogStickVertical != 0)
                {
                    js_linear = XboxControllerInput.leftAnalogStickVertical;
                }
                if(XboxControllerInput.rightTrigger != 0)
                {
                    js_linear = XboxControllerInput.rightTrigger;
                }
                if(XboxControllerInput.leftTrigger != 0)
                {
                    js_linear = -XboxControllerInput.leftTrigger;
                }
            }else{
                js_linear = 0.0f;
                js_angular = 0.0f;
            }
        }
        // RobotUDPClient.SendVelCmd(js_linear, js_angular);
    }
    void FixedUpdate()
    {
        Debug.Log("JS_Linear: " + js_linear + " JS_Angular: " + js_angular);         
        RobotUDPClient.SendVelCmd(js_linear, js_angular);
    }
}
