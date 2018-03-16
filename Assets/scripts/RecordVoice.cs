using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;

public class RecordVoice : MonoBehaviour {


	static int recordForSecs = 1;
	static int sampleQuality = 4;
	static AudioSource aud2,aud;

	// Use this for initialization
	void Start () {
		aud2 = GetComponents<AudioSource> () [1];
		aud = GetComponent<AudioSource>();
		//StartCoroutine (record ());
	}

	public static IEnumerator record(){
		Debug.Log (String.Format ("recording"));
		while (true) {
			aud.clip = Microphone.Start ("Built-in Microphone", false, recordForSecs, 44100 / sampleQuality);
			yield return new WaitForSeconds (recordForSecs);

			AudioClip cutClip1 = AudioClip.Create ("playback", aud.clip.samples, aud.clip.channels, aud.clip.frequency, false, false);
			float[] soundData = new float[aud.clip.samples * aud.clip.channels];
			aud.clip.GetData (soundData, 0);

			// create a byte array and copy the floats into it...
			var byteArray = new byte[soundData.Length * 4];
			Buffer.BlockCopy (soundData, 0, byteArray, 0, byteArray.Length);

			byteArray = Encrypt_Compress.Compress (byteArray);

			byte[][] IV_Key = Encrypt_Compress.genIVKey ("giannis", 6465456);//Connection.getConnectionTime ());//pass must be smaller than 32 char. pass<32
			//myAes.IV = IV_Key [0];
			//myAes.Key = IV_Key [1];

			// Encrypt the string to an array of bytes.
			byte[] encrypted = Encrypt_Compress.EncryptStringToBytes_Aes (Convert.ToBase64String (byteArray),//Encrypt(byteArray,//
				                  IV_Key [1], IV_Key [0]);

			Connection.sendByteArr (encrypted);
			/*for(int i=0;i<encrypted.Length;i++){
				if (UnityEngine.Random.Range (0,100000)>99998) {
					encrypted [i] = encrypted [i-1];
				}
			}*/
			//playSound(encrypted);
		}
	}

	public static void playSound(byte[] byteSound){
		AudioClip cutClip1 = AudioClip.Create("playback", (44100/sampleQuality)*recordForSecs, 1, 44100/sampleQuality, false, false);
		//aud2 = GetComponent<AudioSource>();

		byte[][] IV_Key = Encrypt_Compress.genIVKey ("giannis", 6465456);//Connection.getConnectionTime ());//pass must be smaller than 32 char. pass<32
		//myAes.IV = IV_Key [0];
		//myAes.Key = IV_Key [1];
		var roundtrip = Encrypt_Compress.DecryptStringFromBytes_Aes (byteSound, //Decrypt(byteSound,//
			IV_Key [1], IV_Key [0]);
		var byteArray = new byte[byteSound.Length];
		byteArray = Encrypt_Compress.Decompress (Convert.FromBase64String (roundtrip));

		// create a second float array and copy the bytes into it...
		var floatArray2 = new float[byteArray.Length / 4];
		Buffer.BlockCopy(byteArray, 0, floatArray2, 0, byteArray.Length);

		aud2.clip = cutClip1;
		aud2.clip.SetData (floatArray2,0);
		aud2.volume = 1.3f;
		aud2.Play();
	}
}
