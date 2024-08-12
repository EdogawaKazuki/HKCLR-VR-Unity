using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseClient : MonoBehaviour
{
    #region Variables



	[SerializeField]
	private string _robotIP = "192.168.4.1";
	[SerializeField]
	private int _robotPort = 1234;

	private bool _connected = false;
	private bool _unlocked = true;
	private Thread _receiveThread;
	byte[] sendData;
	byte[] byteArray;
	int recvLen;

	public bool SendCmd = true;

	Socket ClientSocket;
	EndPoint ServerEndPoint;

	#endregion

	#region MonoBehaviour
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
			}
			catch (Exception ex)
			{
				print(ex);
			}
			//Connect();
		}
        else
		{
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
    public void SendAngleCommand(float[] angleList)
    {
        try{

            byteArray = new byte[sizeof(float) * 3 + 1];
            byteArray[0] = 2;
            //Debug.Log(robotJointAngleCmd[0]);
            Buffer.BlockCopy(BitConverter.GetBytes(angleList[0]), 0, byteArray, 1, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(angleList[1]), 0, byteArray, 5, 4);
            //sendData = Encoding.ASCII.GetBytes("angle," + String.Join(",", _robotController.GetJointAngles()));
            ClientSocket.SendTo(byteArray, byteArray.Length, SocketFlags.None, ServerEndPoint);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
	#endregion
}
