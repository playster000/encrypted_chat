using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Encrypt_Compress : MonoBehaviour {

	// Use this for initialization
	void Start () {
		try {


			/*string original = ("Here is some data to encrypt!");
			//string original = "Here is some data to encrypt! ere is some data to enc495rypt!";
			Debug.Log(String.Format("Original:\n{0}\nbyteLength {1}\n", original, Encoding.ASCII.GetBytes(original).Length));

			byte[] toCompress = Encoding.Unicode.GetBytes(original);
			toCompress = Compress(toCompress);
			// Create a new instance of the Aes
			// class.  This generates a new key and initialization 
			// vector (IV).
			using (Aes myAes = Aes.Create()) {

				byte[][] IV_Key = genIVKey("giannis",Connection.getConnectionTime());//pass must be smaller than 32 char. pass<32
				myAes.IV = IV_Key[0];
				myAes.Key = IV_Key[1];

				// Encrypt the string to an array of bytes.
				byte[] encrypted = EncryptStringToBytes_Aes(Convert.ToBase64String(toCompress),
					myAes.Key, myAes.IV);
				Debug.Log(String.Format("encrypted and compressed:\n{0}\nbyteLength {1}\n", Convert.ToBase64String(encrypted), encrypted.Length));


				// Decrypt the bytes to a string.
				string roundtrip = DecryptStringFromBytes_Aes(encrypted, 
					myAes.Key, myAes.IV);
				//Console.WriteLine("compressed Round Trip: {0}\n", roundtrip);
				roundtrip = Encoding.Unicode.GetString(Decompress(Convert.FromBase64String(roundtrip)));


				//Display the original data and the decrypted data.
				Debug.Log(String.Format("decrypted and decompressed:\n{0}\n", roundtrip));
				Debug.Log(String.Format("Key: {0}", Encoding.ASCII.GetString(myAes.Key)));
				Debug.Log(String.Format("IV: {0}", Encoding.ASCII.GetString(myAes.IV)));
				long timeStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
				Debug.Log(String.Format("Unix TimeStamp: {0} to string: {1} get length-3: {2} InternetTime {3}", timeStamp, Convert.ToBase64String(Encoding.UTF8.GetBytes(timeStamp.ToString())),
					timeStamp.ToString().ToCharArray()[timeStamp.ToString().Length - 3],GetNetworkTime()));
			}*/ //bug for george?

		} catch (Exception e) {
			Debug.Log(String.Format("Error: {0}", e.Message));
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static byte[][] genIVKey(string key, long timer){
		int extra = 0;
		char[] newKey = new char[32];
		char[] tempKey = key.ToCharArray ();
		char[] timerString = timer.ToString ().ToCharArray ();

		long passValue = 0;
		for(int i=0;i<tempKey.Length;i++){//find passvalue
			passValue += tempKey[i];
		}
		newKey [0] = (char)((passValue) % 127 + 33);
		for(int i=1;i<newKey.Length;i++){//fill Password
			newKey [i] = i-1<tempKey.Length ? tempKey[i-1] :'0';
		}
		extra = (int)(passValue % 12);
		extra++;
		/*for (int j = 0; j < extra; j++) {
			char last = newKey [newKey.Length - 1];
			for (int i = newKey.Length - 1; i >= 1; i--) {
				newKey [i] = newKey [i - 1];
			}
			newKey [0] = last;
		}*/
		for (int i = 0; i < timerString.Length; i++) {
			newKey [i] = (char)((newKey [i] + timerString [i]) % 127 + 33);
		}
		for (int i = 0; i < newKey.Length; i++) {
			if (i > 0)
				newKey [i] = (char)((newKey [i] + newKey [i - 1]) % 127 + 33);
			else
				newKey [i] = (char)((newKey [i] + newKey [i]) % 127 + 33);
		}

		string newKeyString =  new string(newKey);
		/*using (MD5 md5Hash = MD5.Create())//hash it
		{
			byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(newKeyString));
			StringBuilder sBuilder = new StringBuilder();
			// and format each one as a hexadecimal string.
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}
			newKeyString = sBuilder.ToString();
		}*/
		string iv = newKeyString.Substring (0 + extra, 16);

		byte[][] IV_Key = new byte[2][];
		IV_Key [0] = Encoding.ASCII.GetBytes (iv);
		IV_Key [1] = Encoding.ASCII.GetBytes (newKeyString);
		return IV_Key;
	}



	public static long GetNetworkTime()
	{
		try {
			DateTime networkDateTime;
			using (var response = 
				WebRequest.Create("http://www.google.com").GetResponse())
				//string todaysDates =  response.Headers["date"];
				networkDateTime = DateTime.ParseExact(response.Headers["date"],
					"ddd, dd MMM yyyy HH:mm:ss 'GMT'",
					CultureInfo.InvariantCulture.DateTimeFormat,
					DateTimeStyles.AssumeUniversal);

			return networkDateTime.ToLocalTime ().ToUniversalTime().Ticks/10000;//milisec
		} catch (Exception e) {
			Debug.Log(String.Format("Internet Time Error: {0}", e.Message));
		}
		return 0;
	}

	// stackoverflow.com/a/3294698/162671
	static uint SwapEndianness(ulong x)
	{
		return (uint)(((x & 0x000000ff) << 24) +
			((x & 0x0000ff00) << 8) +
			((x & 0x00ff0000) >> 8) +
			((x & 0xff000000) >> 24));
	}

	public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, 
		byte[] IV)
	{
		// Check arguments.
		if (plainText == null || plainText.Length <= 0)
			throw new ArgumentNullException("plainText");
		if (Key == null || Key.Length <= 0)
			throw new ArgumentNullException("Key");
		if (IV == null || IV.Length <= 0)
			throw new ArgumentNullException("IV");
		byte[] encrypted;
		// Create an Aes object
		// with the specified key and IV.
		using (Aes aesAlg = Aes.Create()) {
			aesAlg.Key = Key;
			aesAlg.IV = IV;

			// Create a decrytor to perform the stream transform.
			ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key
				, aesAlg.IV);

			// Create the streams used for encryption.
			using (MemoryStream msEncrypt = new MemoryStream()) {
				using (CryptoStream csEncrypt = new CryptoStream(msEncrypt
					, encryptor, CryptoStreamMode.Write)) {
					using (StreamWriter swEncrypt = new StreamWriter(
						csEncrypt)) {

						//Write all data to the stream.
						swEncrypt.Write(plainText);
					}
					encrypted = msEncrypt.ToArray();
				}
			}
		}


		// Return the encrypted bytes from the memory stream.
		return encrypted;

	}

	//Decrypt byte[]
	public static byte[] Decrypt(byte[] data,byte[] key,byte[] iv)
	{
		using (Rijndael alg = Rijndael.Create ()) {
			alg.Key = key;
			alg.IV = iv;

			using (var stream = new MemoryStream ())
			using (var decryptor = alg.CreateDecryptor ())
			using (var encrypt = new CryptoStream (stream, decryptor, CryptoStreamMode.Write)) {
				encrypt.Write (data, 0, data.Length);
				encrypt.FlushFinalBlock ();
				return stream.ToArray ();
			}
		}
	}

	//Encrypt byte[]
	public static byte[] Encrypt(byte[] data,byte[] key,byte[] iv)
	{
		using (Rijndael alg = Rijndael.Create ()) {
			alg.Key = key;
			alg.IV = iv;

			using (var stream = new MemoryStream ())
			using (var encryptor = alg.CreateEncryptor ())
			using (var encrypt = new CryptoStream (stream, encryptor, CryptoStreamMode.Write)) {
				encrypt.Write (data, 0, data.Length);
				encrypt.FlushFinalBlock ();
				return stream.ToArray ();
			}
		}
	}

	public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key
		, byte[] IV)
	{
		// Check arguments.
		if (cipherText == null || cipherText.Length <= 0)
			throw new ArgumentNullException("cipherText");
		if (Key == null || Key.Length <= 0)
			throw new ArgumentNullException("Key");
		if (IV == null || IV.Length <= 0)
			throw new ArgumentNullException("IV");

		// Declare the string used to hold
		// the decrypted text.
		string plaintext = null;

		// Create an Aes object
		// with the specified key and IV.
		using (Aes aesAlg = Aes.Create()) {
			aesAlg.Key = Key;
			aesAlg.IV = IV;

			// Create a decrytor to perform the stream transform.
			ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key
				, aesAlg.IV);

			// Create the streams used for decryption.
			using (MemoryStream msDecrypt = new MemoryStream(cipherText)) {
				using (CryptoStream csDecrypt = new CryptoStream(msDecrypt
					, decryptor, CryptoStreamMode.Read)) {
					using (StreamReader srDecrypt = new StreamReader(
						csDecrypt)) {

						// Read the decrypted bytes from the decrypting 

						// and place them in a string.
						plaintext = srDecrypt.ReadToEnd();
					}
				}
			}

		}

		return plaintext;

	}
	static byte[] ObjectToByteArray(object obj)
	{
		if (obj == null)
			return null;
		BinaryFormatter bf = new BinaryFormatter();
		using (MemoryStream ms = new MemoryStream()) {
			bf.Serialize(ms, obj);
			return ms.ToArray();
		}
	}
	public static byte[] Decompress(byte[] bytes)
	{
		using (var uncompressed = new MemoryStream())
		using (var compressed = new MemoryStream(bytes))
		using (var ds = new DeflateStream(compressed, CompressionMode.Decompress)) {
			//ds.CopyTo(uncompressed);
			CopyStream(ds,uncompressed,ds.BaseStream.Length);
			return uncompressed.ToArray();
		}
	}

	public static byte[] Compress(byte[] bytes)
	{
		using (var memoryStream = new MemoryStream()) {
			using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
				deflateStream.Write(bytes, 0, bytes.Length);

			return memoryStream.ToArray();
		}
	}
	public static void CopyStream(Stream input, Stream output,long byteSize)
	{
		byte[] b = new byte[byteSize*3];//limited size
		int r;
		while ((r = input.Read(b, 0, b.Length)) > 0)
			output.Write(b, 0, r);
	}

}
