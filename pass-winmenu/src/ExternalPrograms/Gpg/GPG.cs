using System;
using System.Linq;
using PassWinmenu.Configuration;

namespace PassWinmenu.ExternalPrograms.Gpg
{
	/// <summary>
	/// Simple wrapper over GPG.
	/// </summary>
	internal class GPG : ICryptoService, ISignService
	{
		private readonly IGpgTransport gpgTransport;
		private readonly IGpgAgent gpgAgent;
		private readonly IGpgResultVerifier gpgResultVerifier;
		private readonly PinentryWatcher pinentryWatcher = new PinentryWatcher();
		private readonly bool enablePinentryFix;

		public GPG(IGpgTransport gpgTransport, IGpgAgent gpgAgent, IGpgResultVerifier gpgResultVerifier, GpgConfig gpgConfig)
		{
			this.gpgTransport = gpgTransport;
			this.gpgAgent = gpgAgent;
			this.gpgResultVerifier = gpgResultVerifier;
			this.enablePinentryFix = gpgConfig.PinentryFix;
		}

		/// <summary>
		/// Decrypt a file with GPG.
		/// </summary>
		/// <param name="file">The path to the file to be decrypted.</param>
		/// <returns>The contents of the decrypted file.</returns>
		/// <exception cref="GpgException">Thrown when decryption fails.</exception>
		public string Decrypt(string file)
		{
			if(enablePinentryFix) pinentryWatcher.BumpPinentryWindow();
			gpgAgent.EnsureAgentResponsive();
			var result = gpgTransport.CallGpg($"--decrypt \"{file}\"");
			gpgResultVerifier.VerifyDecryption(result);
			return result.Stdout;
		}

		/// <summary>
		/// Encrypt a string with GPG.
		/// </summary>
		/// <param name="data">The text to be encrypted.</param>
		/// <param name="outputFile">The path to the output file.</param>
		/// <param name="recipients">An array of GPG ids for which the file should be encrypted.</param>
		/// <exception cref="GpgException">Thrown when encryption fails.</exception>
		public void Encrypt(string data, string outputFile, params string[] recipients)
		{
			if (recipients == null) recipients = Array.Empty<string>();
			var recipientList = string.Join(" ", recipients.Select(r => $"--recipient \"{r}\""));

			var result = gpgTransport.CallGpg($"--output \"{outputFile}\" {recipientList} --encrypt", data);
			gpgResultVerifier.VerifyEncryption(result);
		}

		private void ListSecretKeys()
		{
			gpgAgent.EnsureAgentResponsive();
			var result = gpgTransport.CallGpg("--list-secret-keys");
			if (result.Stdout.Length == 0)
			{
				throw new GpgError("No private keys found. Pass-winmenu will not be able to decrypt your passwords.");
			}
			// At some point in the future we might have a use for this data,
			// But for now, all we really use this method for is to ensure the GPG agent is started.
			//Log.Send("Secret key IDs: ");
			//Log.Send(result.Stdout);
		}

		public void StartAgent()
		{
			// Looking up a private key will start the GPG agent.
			ListSecretKeys();
		}

		public string GetVersion()
		{
			var output = gpgTransport.CallGpg("--version");
			return output.Stdout.Split(new []{"\r\n"}, StringSplitOptions.RemoveEmptyEntries).First();
		}

		public string Sign(string message, string keyId)
		{
			if(enablePinentryFix) pinentryWatcher.BumpPinentryWindow();
			gpgAgent.EnsureAgentResponsive();
			var result = gpgTransport.CallGpg($"--detach-sign --armor", message);
			return result.Stdout;
		}
	}
}
