using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeadTrackingSender : MonoBehaviour
{
    #region Variables



	[SerializeField]
	private string _robotIP = "192.168.4.1";
	[SerializeField]
	private int _robotPort = 1234;

	private List<int> _cmdPwmList = new List<int>() { 0, 0, 0 };

	private bool _connected = false;
	private bool _unlocked = true;
	private bool _isESP32 = true;
	private TcpClient _client;
	private Thread _clientThread;
	private Thread _receiveThread;
	List<int> robotJointPWM;
	List<float> robotJointAngleCmd;
	byte[] sendData;
	byte[] recvData = new byte[2*3 + 4*3 + 2*3];
	byte[] byteArray;
	int recvLen;

	public bool SendCmd = true;

	Socket ClientSocket;
	EndPoint ServerEndPoint;
	
	int windowSize = 5;
	int count = 0;
	float tmp;
	float[] sum = { 0, 0, 0 };
	List<Queue<float>> tail;
	int filterIndex = 0;
    float[] _angleListRead = { 0, 0, 0 };

    TMP_Text rotx_text;
    TMP_Text roty_text;

    Vector2 CameraRotation = new();

	#endregion

	#region MonoBehaviour
	private void Start()
    {
		//_robotIP = PlayerPrefs.GetString("_robotIP");
		//_robotPort = PlayerPrefs.GetInt("_robotPort");
		//_robotIPIF.text = _robotIP;
		//_robotPortIF.text = _robotPort.ToString();
		//Connect();
		//Time.fixedDeltaTime = 0.05f;
        rotx_text = GameObject.Find("Canvas/RotX").GetComponent<TMP_Text>();
        roty_text = GameObject.Find("Canvas/RotY").GetComponent<TMP_Text>();
		tail = new List<Queue<float>>() { new Queue<float>(), new Queue<float>(), new Queue<float>() };
        SetConnect(true);
        Lock();
	}
    private void OnEnable()
    {
		//Time.fixedDeltaTime = 0.05f;
	}
    private void FixedUpdate()
	{
        Vector3 rot = Camera.main.transform.rotation.eulerAngles;
        CameraRotation.x = -(rot.x > 180 ? rot.x - 360 : rot.x);
        CameraRotation.y = -(rot.y > 180 ? rot.y - 360 : rot.y);
        rotx_text.text = CameraRotation.x.ToString();
        roty_text.text = CameraRotation.y.ToString();
        try{

            byteArray = new byte[sizeof(float) * 3 + 1];
            byteArray[0] = 2;
            //Debug.Log(robotJointAngleCmd[0]);
            Buffer.BlockCopy(BitConverter.GetBytes(CameraRotation.x), 0, byteArray, 1, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(CameraRotation.y), 0, byteArray, 5, 4);
            //sendData = Encoding.ASCII.GetBytes("angle," + String.Join(",", _robotController.GetJointAngles()));
            ClientSocket.SendTo(byteArray, byteArray.Length, SocketFlags.None, ServerEndPoint);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
	}
    private void OnApplicationQuit()
	{
        Unlock();
        SetConnect(false);
    }
    #endregion

    #region Methods
    public void SetRobotIP(string value)
    {
        _robotIP = value;
		PlayerPrefs.SetString("_robotIP", _robotIP);
		PlayerPrefs.Save();
		Debug.Log(value);
	}
    public void SetRobotPort(string value)
    {
        _robotPort = int.Parse(value);
		PlayerPrefs.SetInt("_robotPort", _robotPort);
		PlayerPrefs.Save();
		Debug.Log(value);
	}
    public void SetConnect(bool value)
    {
        if (value)
		{
            try
            {

				ServerEndPoint = new IPEndPoint(IPAddress.Parse(_robotIP), _robotPort);
				ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				ClientSocket.SendTimeout = 1000;
				ClientSocket.ReceiveTimeout = 1000;
				// _receiveThread = new Thread(new ThreadStart(ClientThread));
				// _connected = true;
				// _receiveThread.Start();
				//sendData = Encoding.ASCII.GetBytes("connect");
				//ClientSocket.SendTo(sendData, sendData.Length, SocketFlags.None, ServerEndPoint);
				//byteArray = new byte[2];
				//byteArray[0] = 0;
				//byteArray[1] = 1;
				//ClientSocket.SendTo(byteArray, byteArray.Length, SocketFlags.None, ServerEndPoint);
			}
			catch (Exception ex)
			{
				print(ex);
			}
			//Connect();
		}
        else
		{
			_receiveThread.Abort();
			_connected = false;
			//sendData = Encoding.ASCII.GetBytes("disconnect");
			//ClientSocket.SendTo(sendData, sendData.Length, SocketFlags.None, ServerEndPoint);
			byteArray = new byte[2];
			byteArray[0] = 0;
			byteArray[1] = 0;
			ClientSocket.SendTo(byteArray, byteArray.Length, SocketFlags.None, ServerEndPoint);
			//Disconnect();
		}
    }
	public bool IsConnected() { return _connected; }
	void ClientThread()
	{
		while (true)
        {

			try
			{
                //if (_unlocked)
				{
					recvLen = ClientSocket.ReceiveFrom(recvData, ref ServerEndPoint);
					//Debug.Log(BitConverter.ToInt16(recvData, 4));
					ParsePWMByte(recvData);
					//ParsePWM(Encoding.ASCII.GetString(recvData));
				}
			}
			catch (Exception e)
			{
                if (_connected)
				{
					//sendData = Encoding.ASCII.GetBytes("connect");
					//ClientSocket.SendTo(sendData, sendData.Length, SocketFlags.None, ServerEndPoint);
					//ClientSocket.Close();
					Debug.Log("Resend connecting request");
					byteArray = new byte[2];
					byteArray[0] = 0;
					byteArray[1] = 1;
					ClientSocket.SendTo(byteArray, byteArray.Length, SocketFlags.None, ServerEndPoint);
				}
				Debug.Log(e);
			}
		}

    }

    private void ParsePWMByte(byte[] byteArray)
	{
		if (filterIndex == 1 && count <= windowSize)
		{
			count++;
		}
		for (int i = 0; i < 3; i++)
		{
			// _feedbackPwmList[i] = BitConverter.ToInt16(byteArray, i * 2);
			//Debug.Log(BitConverter.ToInt16(byteArray, i * 2));
		}
		//Debug.Log(_feedbackPwmList[0] + " " + _feedbackPwmList[1] + " " + _feedbackPwmList[2] + " ");
		for (int i = 0; i < 3; i++)
		{
			tmp = BitConverter.ToSingle(byteArray, 3 * 2 + i * 4);
			if (filterIndex == 1)
			{
				if (count <= windowSize)
				{
					sum[i] += tmp;
					tail[i].Enqueue(tmp);
					_angleListRead[i] = sum[i] / count;
					//Debug.Log(tail[i].Count);
				}
                else
				{
					sum[i] += tmp;
					tail[i].Enqueue(tmp);
					sum[i] -= tail[i].Dequeue();
					//Debug.Log(tail[i].Count);
					_angleListRead[i] = sum[i] / windowSize;
				}
            }
            else if(filterIndex == 0 || filterIndex == 2)
			{
				_angleListRead[i] = tmp;
			}
			//Debug.Log(BitConverter.ToInt16(byteArray, 3 * 2 + i * 4));
		}
        //Debug.Log(" " + _angleListRead[0] + " " + _angleListRead[1] + " " + _angleListRead[2] + " ");
        if (SendCmd)
		{
			for (int i = 0; i < 3; i++)
			{
				_cmdPwmList[i] = BitConverter.ToInt16(byteArray, 3 * 2 + 3 * 4 + i * 2);
				//Debug.Log(BitConverter.ToInt16(byteArray, 3 * 2 + 3 * 4 + i * 2));
			}
			//Debug.Log(_cmdPwmList[0] + " " + _cmdPwmList[1] + " " + _cmdPwmList[2] + " ");
		}
	}

	public List<int> GetCmdPWM() { return _cmdPwmList; }
	public void Unlock() { 
		_unlocked = true;
		//sendData = Encoding.ASCII.GetBytes("unlock,true,end");
		//ClientSocket.SendTo(sendData, sendData.Length, SocketFlags.None, ServerEndPoint);
		byteArray = new byte[2];
		byteArray[0] = 1;
		byteArray[1] = 0;
		ClientSocket.SendTo(byteArray, byteArray.Length, SocketFlags.None, ServerEndPoint);
	}
	public void Lock()
    {
		_unlocked = false;
		//sendData = Encoding.ASCII.GetBytes("unlock,false,end");
		//ClientSocket.SendTo(sendData, sendData.Length, SocketFlags.None, ServerEndPoint);
		byteArray = new byte[2];
		byteArray[0] = 1;
		byteArray[1] = 1;
		ClientSocket.SendTo(byteArray, byteArray.Length, SocketFlags.None, ServerEndPoint);
	}
	public bool IsUnLocked()
    {
		return _unlocked;
    }
	public void SendPWMCmd(int index, int pwm)
	{
		_cmdPwmList[index] = pwm;
		byteArray = new byte[sizeof(float) * 3 + 1];
		byteArray[0] = 3;
		Buffer.BlockCopy(BitConverter.GetBytes((float)_cmdPwmList[0]), 0, byteArray, 1, 4);
		Buffer.BlockCopy(BitConverter.GetBytes((float)_cmdPwmList[1]), 0, byteArray, 5, 4);
		Buffer.BlockCopy(BitConverter.GetBytes((float)_cmdPwmList[2]), 0, byteArray, 9, 4);
		//sendData = Encoding.ASCII.GetBytes("angle," + String.Join(",", _robotController.GetJointAngles()));
		ClientSocket.SendTo(byteArray, byteArray.Length, SocketFlags.None, ServerEndPoint);
	}
	#endregion
}
