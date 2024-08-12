using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BaseController : MonoBehaviour
{
    TMP_Text rotx_text;
    TMP_Text roty_text;
    BaseClient baseClient;
    Vector2 CameraRotation = new();
    Vector2 RealCamPanTilt = new();
    public Transform cameraQuad;
    public Transform cameraQuadRotXContainer;
    public Transform cameraQuadRotYContainer;
    public Transform cameraQuadZoomContainer;
    public Transform eyeTransform;
    bool isEyeTracking = false;
    private KalmanFilter kalmanFilterX = new KalmanFilter();
    private KalmanFilter kalmanFilterY = new KalmanFilter();
    private KalmanFilter kalmanFilterRotX = new KalmanFilter();
    private SmallChangeFilter smallChangeFilterX = new SmallChangeFilter();
    private SmallChangeFilter smallChangeFilterY = new SmallChangeFilter();
    private SmallChangeFilter smallChangeFilterRotX = new SmallChangeFilter();
    // Start is called before the first frame update
    void Start()
    {
        rotx_text = transform.Find("Canvas/Webcam/RotX").GetComponent<TMP_Text>();
        roty_text = transform.Find("Canvas/Webcam/RotY").GetComponent<TMP_Text>();
        baseClient = GetComponent<BaseClient>();
        baseClient.SetConnect(true);
        baseClient.Unlock();
        
    }

    // Update is called once per frame
    void Update()
    {
        mouseMove();
        // mouseHit();
        EyeTracking();
        rotx_text.text = RealCamPanTilt.x.ToString();
        roty_text.text = RealCamPanTilt.y.ToString();
    }
    void FixedUpdate()
    {
        //baseClient.SendAngleCommand(new float[] { CameraRotation.x, CameraRotation.y, 0 });
    }
    void mouseMove()
    {
        if(Input.GetKey(KeyCode.Mouse0)){

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            CameraRotation.x += mouseX;
            CameraRotation.y += mouseY;
            // if (CameraRotation.x > 145)
            // {
            //     CameraRotation.x = 145;
            // }
            // if (CameraRotation.x < -145)
            // {
            //     CameraRotation.x = -145;
            // }
            // if (CameraRotation.y > 90)
            // {
            //     CameraRotation.y = 90;
            // }
            // if (CameraRotation.y < -35)
            // {
            //     CameraRotation.y = -35;
            // }
            Camera.main.transform.localEulerAngles = new Vector3(-CameraRotation.y, CameraRotation.x, 0);
        }
        if(Input.GetKey(KeyCode.Mouse1)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // Calculate the rotation to look in the direction of the hit point
            Quaternion rotation = Quaternion.LookRotation(ray.direction, Vector3.up);
            Vector3 eularAngles = rotation.eulerAngles;

            // Apply the rotation to the quad
            // cameraQuad.rotation = rotation;
            cameraQuadRotYContainer.localEulerAngles = new Vector3(0, eularAngles.y, 0);
            cameraQuadRotXContainer.localEulerAngles = new Vector3(eularAngles.x, 0, 0);

            // send the rotation to the robot
            baseClient.SendAngleCommand(new float[] { RealCamPanTilt.x, RealCamPanTilt.y, 0 });
        }
        float zoom = Input.GetAxis("Mouse ScrollWheel");
        if(zoom != 0){
            
            if(Input.GetKey(KeyCode.Mouse1)){
                cameraQuadZoomContainer.localScale += new Vector3(zoom, zoom, zoom);
            }
            else{
                Camera.main.fieldOfView -= zoom * 10;
            }
        }
        if(Input.GetKey("r")){
            CameraRotation = new Vector2(0, 0);
            Camera.main.transform.localEulerAngles = new Vector3(0, 0, 0);
            RealCamPanTilt = new Vector2(0, 0);
            baseClient.SendAngleCommand(new float[] { RealCamPanTilt.x, RealCamPanTilt.y, 0 });
            cameraQuadRotXContainer.localEulerAngles = new Vector3(0, 0, 0);
            cameraQuadRotYContainer.localEulerAngles = new Vector3(0, 0, 0);
        }

    }
    void EyeTracking(){
        if(Input.GetKeyDown(KeyCode.E)){
            isEyeTracking = !isEyeTracking;
        }
        if(isEyeTracking){
            Vector3 eularAngles = eyeTransform.localEulerAngles;

            // Apply the rotation to the quad
            // cameraQuad.rotation = rotation;
            // raw rotation & position
            // todo: add filter

            RealCamPanTilt = new Vector2(eularAngles.y, -eularAngles.x);
            Debug.Log("Pan: " + RealCamPanTilt.x + " Tilt: " + RealCamPanTilt.y);
            if (RealCamPanTilt.x > 180) RealCamPanTilt.x -= 360;
            if (RealCamPanTilt.y < -180) RealCamPanTilt.y += 360;
            if (RealCamPanTilt.x > 145)
            {
                RealCamPanTilt.x = 145;
            }
            if (RealCamPanTilt.x < -145)
            {
                RealCamPanTilt.x = -145;
            }
            if (RealCamPanTilt.y > 90)
            {
                RealCamPanTilt.y = 90;
            }
            if (RealCamPanTilt.y < -35)
            {
                RealCamPanTilt.y = -35;
            }
            float smoothedX = kalmanFilterX.Update(smallChangeFilterX.Update(RealCamPanTilt.x));
            float smoothedY = kalmanFilterY.Update(smallChangeFilterY.Update(RealCamPanTilt.y));
            float smoothedRotZ = kalmanFilterRotX.Update(smallChangeFilterRotX.Update(eularAngles.z));
            cameraQuad.eulerAngles = new Vector3(-smoothedY, smoothedX, smoothedRotZ);
            cameraQuad.position = eyeTransform.position;
            // send the rotation to the robot
            baseClient.SendAngleCommand(new float[] { smoothedX, smoothedY, 0 });

        }
        
    }
    void mouseHit()
    {
        if(isEyeTracking) return;
        // Convert mouse position to a ray from the main camera
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RealCamPanTilt.x = Mathf.Atan2(ray.direction.x, ray.direction.z) * Mathf.Rad2Deg;
        RealCamPanTilt.y = Mathf.Asin(ray.direction.y / ray.direction.magnitude) * Mathf.Rad2Deg;
        if (RealCamPanTilt.x > 145)
        {
            RealCamPanTilt.x = 145;
        }
        if (RealCamPanTilt.x < -145)
        {
            RealCamPanTilt.x = -145;
        }
        if (RealCamPanTilt.y > 90)
        {
            RealCamPanTilt.y = 90;
        }
        if (RealCamPanTilt.y < -35)
        {
            RealCamPanTilt.y = -35;
        }
        // Debug.Log("Pan: " + RealCamPanTilt.x + " Tilt: " + RealCamPanTilt.y);
    }

}
