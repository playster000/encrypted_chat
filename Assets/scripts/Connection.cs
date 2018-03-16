using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.UI;
using UnityEngine;

public class Connection : MonoBehaviour {

	public InputField portField,ipAd,password,portToSendField;//password saved in ram in byte array type
	public Text receivedMessage, status;
	public Toggle notLoopToggle;
	bool notyet;
	private static long timer = 60645155093652;
	int inConversation = 0;
	Byte[] receiveBytes;
	static UdpClient minUdpClient;
	static IPEndPoint RemoteIpEndPoint;
	static IPEndPoint SendIPEndPoint;
	//public static long connectTime = 0;

	// Use this for initialization
	void Start ()
	{
		//Screen.SetResolution (294, 529, false);
		//StartCoroutine(refreshTimer());//bug for george?
	}
	
	// Update is called once per frame
	void Update () {
		/*if (minUdpClient != null && notyet) {
			notyet = false;
			StopCoroutine(RecordVoice.record());
			StopCoroutine(receive());
			StopCoroutine(receiveWithLag());
		}*/
	}

	public void sendInvite(){
		notyet = false;
		StartCoroutine (chatConnect ());
		StopCoroutine(RecordVoice.record());
		StopCoroutine(receive());
		StopCoroutine(receiveWithLag());
	}

	IEnumerator refreshTimer(){
		long netTime = 0;
		while (netTime==0) {
			netTime = Encrypt_Compress.GetNetworkTime();
			yield return new WaitForSeconds (2f);
		}
		long startTime = (long)(DateTime.UtcNow.Subtract (new DateTime (1970, 1, 1))).TotalMilliseconds;
		while (true) {
			timer = (netTime + (long)(DateTime.UtcNow.Subtract (new DateTime (1970, 1, 1))).TotalMilliseconds-startTime);
			yield return new WaitForSeconds (1);
		}
	}
	public static long getConnectionTime(){
		long connectionTime = timer/10000;
		long curMinute = (timer % 100000)/1000;

		return connectionTime;
	}

	public void startConnection(){
		StartCoroutine (chatConnect ());
	}

	IEnumerator chatConnect(){
		yield return new WaitForSeconds(1.1f);
		try{
			notyet = true;
			string ipaddress = ipAd.text;//"192.168.1.255";
			int port = int.Parse(portField.text);
			int portToSend = int.Parse(portToSendField.text);//10002;
			Byte[] sendBytes = Encoding.ASCII.GetBytes ("message.text");
			minUdpClient = new UdpClient (port);
			RemoteIpEndPoint = new IPEndPoint (IPAddress.Any, 0);//Creates an IPEndPoint to record the IP Address and port number of the sender. 
			SendIPEndPoint = new IPEndPoint (IPAddress.Parse (ipaddress), portToSend);
			int loops = 0;
			Debug.Log ("Starting");
			StartCoroutine(RecordVoice.record());
			StartCoroutine(receive());
			status.text = ("connecting");
		} catch (Exception e) {
			//Debug.Log(String.Format("Error: {0}", e.Message));
			status.text = ("connecting problem " + e.Message);
		}
		while (notyet) {
			/*
			minUdpClient.Send (sendBytes, sendBytes.Length, SendIPEndPoint);
			//System.Threading.Thread.Sleep (1000);
			//Debug.Log("sending UDP port : " + ((IPEndPoint)minUdpClient.Client.LocalEndPoint).Port.ToString());
			try {
				if (minUdpClient.Available > 0) {
					receiveBytes = minUdpClient.Receive (ref RemoteIpEndPoint);
					String returnData = Encoding.ASCII.GetString (receiveBytes);
					receivedMessage.text = ("Data Received:\n" + returnData + "\nfrom ip " + RemoteIpEndPoint.Address.ToString() + " from port " + RemoteIpEndPoint.Port.ToString ());
					status.text = ("UDP Connection Successful. Sending message and receiving message" +" ("+loops+")");
					//minUdpClient.Close ();
					//Console.ReadLine ();
					//notyet = false;
				} else {
					status.text = ("Sending message and waiting to receive message" +" ("+loops+")");
				}
			} catch (Exception e) {
				status.text = (e.ToString ());
			}*/
			yield return new WaitForSeconds(1);
		}
		minUdpClient.Close ();
		status.text = ("Disconnected");
	}
	IEnumerator sendLosslessMessage(UdpClient udpClient,byte[] sendBytes,IPEndPoint SendIPEndPoint){
		float defaultUploadSpeed = 10; //kbytes/s
		yield return new WaitForSeconds(1);
	}

	public static void sendByteArr(byte[] arr){
		minUdpClient.Send (arr, arr.Length, SendIPEndPoint);
	}
	IEnumerator receiveWithLag(){
		while (true) {
			yield return new WaitForSeconds (0.05f);
			if (minUdpClient.Available > 0) {
				yield return new WaitForSeconds (0.1f);
				Debug.Log ("receive data");
				receiveBytes = minUdpClient.Receive (ref RemoteIpEndPoint);
				status.text = ("connected "+RemoteIpEndPoint.ToString());
				RecordVoice.playSound (receiveBytes);
			}
		}
	}
	IEnumerator receive(){
		while (true) {
			yield return new WaitForSeconds (0.0005f);
			if (minUdpClient.Available > 0) {
				try {
					Debug.Log ("receive data");
					receiveBytes = minUdpClient.Receive (ref RemoteIpEndPoint);
					status.text = ("connected " + RemoteIpEndPoint.ToString ());
					RecordVoice.playSound (receiveBytes);
				} catch (Exception e) {
					//Debug.Log(String.Format("Error: {0}", e.Message));
					status.text = ("connecting problem " + e.Message);
				}
			}
		}
	}

}
