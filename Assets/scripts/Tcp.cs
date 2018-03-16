using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Tcp : MonoBehaviour {

	static readonly object _lock = new object();
	static Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();

	// Use this for initialization
	void Start () {
		//StartCoroutine (client());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator server(){
		int count = 0;

		TcpListener ServerSocket = new TcpListener (IPAddress.Any, 5000);
		ServerSocket.Start ();
		StartCoroutine (handle_clients ());

		while (true) {
			yield return new WaitForSeconds (0.3f);
			while (ServerSocket.Pending ()) {
				TcpClient client = ServerSocket.AcceptTcpClient ();
				list_clients.Add (count, client);
				Debug.Log (String.Format ("Someone connected!!"));
				count++;

				//Thread t = new Thread(handle_clients);
				//t.Start(count);
			}
		}
	}

	IEnumerator client(){
		IPAddress ip = IPAddress.Parse("127.0.0.1");
		int port = 5000;
		TcpClient client = new TcpClient();
		client.Connect(ip, port);
		Debug.Log(String.Format("client connected!!"));
		NetworkStream ns = client.GetStream();
		//Thread thread = new Thread(o => ReceiveData((TcpClient)o));

		//thread.Start(client);
		while (true) {
			ReceiveDataClient (client);

			string s ="test";
			if (!string.IsNullOrEmpty(s)) {
				byte[] buffer = Encoding.ASCII.GetBytes (s);
				ns.Write (buffer, 0, buffer.Length);
			}


			//client.Client.Shutdown (SocketShutdown.Send);
			////thread.Join();
			//ns.Close ();
			//client.Close ();
			//Debug.Log (String.Format ("disconnect from server!!"));
			////Console.ReadKey();
			yield return new WaitForSeconds (0.3f);
		}
	}



	IEnumerator handle_clients()
	{
		Debug.Log ("handeling clients");
		//int count = (int)o;
		TcpClient client;

		while (true) {
			for (int i = list_clients.Count-1; i >= 0; i--) {
				client = list_clients [i];

				NetworkStream stream = client.GetStream ();
				if (stream.DataAvailable) {
					byte[] buffer = new byte[1024];
					int byte_count = stream.Read (buffer, 0, buffer.Length);

					if (byte_count == 0) {
						continue;
					}

					string data = Encoding.ASCII.GetString (buffer, 0, byte_count);
					serverSend("you suck!");
					Debug.Log (String.Format (data));
				}


				//list_clients.Remove (i);
				//client.Client.Shutdown (SocketShutdown.Both);
				//client.Close ();
			}
			yield return new WaitForSeconds (0.3f);
		}

	}

	public static void serverSend(string data)
	{
		byte[] buffer = Encoding.ASCII.GetBytes (data);


		foreach (TcpClient c in list_clients.Values) {
			NetworkStream stream = c.GetStream ();

			stream.Write (buffer, 0, buffer.Length);
		}
	}

	static void ReceiveDataClient(TcpClient client)
	{
		NetworkStream ns = client.GetStream();
		if (ns.DataAvailable) {
			byte[] receivedBytes = new byte[1024];
			int byte_count;

			byte_count = ns.Read (receivedBytes, 0, receivedBytes.Length);
			Debug.Log (String.Format (Encoding.ASCII.GetString (receivedBytes, 0, byte_count)));
			
		}
	}

}
