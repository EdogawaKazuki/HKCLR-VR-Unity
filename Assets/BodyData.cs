using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyData : MonoBehaviour
{
    public bool _hasData = false;
    public bool _dataChangedSinceLastQuery = false;
    private OVRPlugin.BodyState _bodyState;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (OVRPlugin.GetBodyState4(OVRPlugin.Step.Render, OVRPlugin.BodyJointSet.UpperBody, ref _bodyState))
        {
            _hasData = true;
            _dataChangedSinceLastQuery = true;
            transform.localRotation = new Quaternion(_bodyState.JointLocations[5].Pose.Orientation.x, _bodyState.JointLocations[5].Pose.Orientation.y, _bodyState.JointLocations[5].Pose.Orientation.z, _bodyState.JointLocations[5].Pose.Orientation.w);
            // Debug.Log("BodyData: " + _bodyState.JointLocations[5].Pose);
            // Debug.Log("Body angle: " + transform.rotation.eulerAngles.y);
        }
        else
        {
            _hasData = false;
        }
    }

}
