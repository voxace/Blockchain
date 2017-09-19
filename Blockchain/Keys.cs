using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain
{
	class Keys
	{

		// Testing asymmetric signature verification
		//string messageToSign = "Hello World!";
		//string signedMessage = Keys.SignData(messageToSign, Ledger.steedy_private_key);
		//MessageBox.Show(signedMessage);
		//bool success = Keys.VerifyData(messageToSign, signedMessage, Ledger.steedy_pub_key);
		//MessageBox.Show("Is this message sent by me? " + success);

		public static Tuple<string, string> CreateKeyPair()
		{
			CspParameters cspParams = new CspParameters { ProviderType = 1 };
			RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
			string publicKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(false));
			string privateKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(true));
			return new Tuple<string, string>(privateKey, publicKey);
		}

		public static byte[] Encrypt(string publicKey, string data)
		{
			CspParameters cspParams = new CspParameters { ProviderType = 1 };
			RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
			rsaProvider.ImportCspBlob(Convert.FromBase64String(publicKey));
			byte[] plainBytes = Encoding.UTF8.GetBytes(data);
			byte[] encryptedBytes = rsaProvider.Encrypt(plainBytes, false);
			return encryptedBytes;
		}

		public static string Decrypt(string privateKey, byte[] encryptedBytes)
		{
			CspParameters cspParams = new CspParameters { ProviderType = 1 };
			RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
			rsaProvider.ImportCspBlob(Convert.FromBase64String(privateKey));
			byte[] plainBytes = rsaProvider.Decrypt(encryptedBytes, false);
			string plainText = Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);
			return plainText;
		}

		public static string SignData(string message, string privateKey)
		{
			Console.WriteLine(privateKey);

			byte[] signedBytes;
			using (var rsa = new RSACryptoServiceProvider())
			{
				var encoder = new UTF8Encoding();
				byte[] originalData = encoder.GetBytes(message);

				try
				{
					rsa.ImportCspBlob(Convert.FromBase64String(privateKey));
					signedBytes = rsa.SignData(originalData, CryptoConfig.MapNameToOID("SHA512"));
				}
				catch (CryptographicException e)
				{
					Console.WriteLine(e.Message);
					return null;
				}
				finally
				{
					rsa.PersistKeyInCsp = false;
				}
			}
			return Convert.ToBase64String(signedBytes);
		}

		public static bool VerifyData(string originalMessage, string signedMessage, string publicKey)
		{
			bool success = false;
			using (var rsa = new RSACryptoServiceProvider())
			{
				var encoder = new UTF8Encoding();
				byte[] bytesToVerify = encoder.GetBytes(originalMessage);
				byte[] signedBytes = Convert.FromBase64String(signedMessage);
				try
				{
					rsa.ImportCspBlob(Convert.FromBase64String(publicKey));
					SHA512Managed Hash = new SHA512Managed();
					byte[] hashedData = Hash.ComputeHash(signedBytes);
					success = rsa.VerifyData(bytesToVerify, CryptoConfig.MapNameToOID("SHA512"), signedBytes);
				}
				catch (CryptographicException e)
				{
					Console.WriteLine(e.Message);
				}
				finally
				{
					rsa.PersistKeyInCsp = false;
				}
			}
			return success;
		}

		public static string createSig(string sender_pub_key, string recipient_pub_key, Double amount, string sender_private_key)
		{
			return Keys.SignData((sender_pub_key + recipient_pub_key + amount.ToString()), sender_private_key);
		}

	}
}
