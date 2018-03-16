using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class holer : MonoBehaviour
{

	public InputField portField,portFieldToSend,ipAd,message;
	public Text receivedMessage, status;
	public Toggle notLoopToggle;
	bool notyet;

	void Start ()
	{
		//Screen.SetResolution (294, 529, false);
	}
	public void startConnection(){
		notyet = false;
		StartCoroutine (chatConnect ());
	}
	IEnumerator chatConnect(){
		yield return new WaitForSeconds(1.1f);
		notyet = true;
		Console.Write ("Enter the destination IP: ");
		string ipaddress = ipAd.text;//"37.6.1.151";//"192.168.1.255";//Console.ReadLine();
		Console.Write ("Enter dest port: ");
		int port = int.Parse(portField.text);//11000;//Convert.ToInt32(Console.ReadLine());
		int portToSend = notLoopToggle.isOn ? int.Parse(portFieldToSend.text):0;//11000;//Convert.ToInt32(Console.ReadLine());
		Byte[] sendBytes = Encoding.ASCII.GetBytes (message.text);//"Hello, fellow hole puncher");
		UdpClient minUdpClient = new UdpClient (port);
		//Creates an IPEndPoint to record the IP Address and port number of the sender. 
		IPEndPoint RemoteIpEndPoint = new IPEndPoint (IPAddress.Any, 0);
		IPEndPoint SendIPEndPoint = new IPEndPoint (IPAddress.Parse (ipaddress), portToSend);
		int loops = 0;
		Debug.Log ("Starting");
		while (notyet) {
			loops++;
			for(int i=10000;i<64000;i++){
				if (!notLoopToggle.isOn) {
					SendIPEndPoint.Port = i;
				}
				minUdpClient.Send (sendBytes, sendBytes.Length, SendIPEndPoint);
				if (notLoopToggle.isOn)
					break;
			}
			//System.Threading.Thread.Sleep (1000);
			//Debug.Log("sending UDP port : " + ((IPEndPoint)minUdpClient.Client.LocalEndPoint).Port.ToString());
			try {
				if (minUdpClient.Available > 0) {  // <----- THIS IS SEXY
					Byte[] receiveBytes = minUdpClient.Receive (ref RemoteIpEndPoint);
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
			}
			yield return new WaitForSeconds(1);
		}
		minUdpClient.Close ();
		status.text = ("Disconnected");
	}
}

