using System;
using System.Linq;

namespace PassWinmenu.ExternalPrograms.Gpg
{
	internal class GpgResultVerifier : IGpgResultVerifier
	{
		public void VerifyDecryption(GpgResult result)
		{
			// Handle known failure conditions first.
			if (result.HasStatusCodes(GpgStatusCode.FAILURE, GpgStatusCode.NODATA))
			{
				throw new GpgError("The file to be decrypted does not look like a valid GPG file.");
			}

			if (result.HasStatusCodes(GpgStatusCode.DECRYPTION_FAILED, GpgStatusCode.NO_SECKEY))
			{
				var keyIds = result.StatusMessages.Where(m => m.StatusCode == GpgStatusCode.NO_SECKEY);

				throw new GpgError("None of your private keys appear to be able to decrypt this file.\n" +
				                   $"The file was encrypted for the following (sub)key(s): {string.Join(", ", keyIds.Select(m => m.Message))}");
			}

			if (result.HasStatusCodes(GpgStatusCode.DECRYPTION_FAILED) && result.StderrMessages.Any(m => m.Contains("Operation cancelled")))
			{
				throw new GpgError("Operation cancelled.");
			}

			// Next, handle generic failure conditions.
			if (result.HasStatusCodes(GpgStatusCode.DECRYPTION_FAILED))
			{
				throw new GpgError("GPG Couldn't decrypt this file. The following information may contain more details about the error that occurred:\n\n" +
				                   $"{string.Join("\n", result.StderrMessages)}");
			}

			if (result.HasStatusCodes(GpgStatusCode.FAILURE)) result.GenerateError();

			// Now look for an indication of a successful decryption.
			if (result.HasStatusCodes(GpgStatusCode.DECRYPTION_OKAY, GpgStatusCode.END_DECRYPTION))
			{
				return;
			}

			// As a last resort, consider a non-zero exit code to be a failure.
			result.EnsureNonZeroExitCode();
		}

		public void VerifyEncryption(GpgResult result)
		{
			if (result.HasStatusCodes(GpgStatusCode.FAILURE, GpgStatusCode.INV_RECP, GpgStatusCode.KEYEXPIRED))
			{
				var failedrcps = result.StatusMessages.Where(m => m.StatusCode == GpgStatusCode.INV_RECP).Select(m => m.Message.Substring(m.Message.IndexOf(" ", StringComparison.Ordinal)));
				throw new GpgError($"Invalid/unknown recipient(s): {string.Join(", ", failedrcps)}\n" +
				                   "The key(s) belonging to this recipient may have expired.");
			}

			if (result.HasStatusCodes(GpgStatusCode.FAILURE, GpgStatusCode.INV_RECP))
			{
				var failedrcps = result.StatusMessages.Where(m => m.StatusCode == GpgStatusCode.INV_RECP).Select(m => m.Message.Substring(m.Message.IndexOf(" ", StringComparison.Ordinal)));
				throw new GpgError($"Invalid/unknown recipient(s): {string.Join(", ", failedrcps)}\n" +
				                   "Make sure that you have imported and trusted the keys belonging to those recipients.");
			}

			if (result.HasStatusCodes(GpgStatusCode.FAILURE))
			{
				result.GenerateError();
			}

			if (result.HasStatusCodes(GpgStatusCode.END_ENCRYPTION))
			{
				return;
			}

			result.EnsureNonZeroExitCode();
		}
	}
}
