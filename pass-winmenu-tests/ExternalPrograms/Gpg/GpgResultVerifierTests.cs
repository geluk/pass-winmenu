using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.ExternalPrograms.Gpg
{
	public class GpgResultVerifierTests
	{
		[Fact]
		public void VerifyDecryption_ZeroExitCode_DoesNotThrow()
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithExitCode(0)
				.Build();

			Should.NotThrow(() => verifier.VerifyDecryption(result));
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(1)]
		[InlineData(1000)]
		public void VerifyDecryption_NonZeroExitCode_ThrowsGpgException(int exitCode)
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithExitCode(exitCode)
				.Build();

			Should.Throw<GpgException>(() => verifier.VerifyDecryption(result));
		}

		[Fact]
		public void VerifyDecryption_NoData_ThrowsGpgError()
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithStatusMessage(GpgStatusCode.FAILURE, null)
				.WithStatusMessage(GpgStatusCode.NODATA, null)
				.Build();

			Should.Throw<GpgError>(() => verifier.VerifyDecryption(result));
		}

		[Fact]
		public void VerifyDecryption_NoSecretKey_ThrowsGpgError()
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithStatusMessage(GpgStatusCode.DECRYPTION_FAILED, null)
				.WithStatusMessage(GpgStatusCode.NO_SECKEY, null)
				.Build();

			Should.Throw<GpgError>(() => verifier.VerifyDecryption(result));
		}

		[Fact]
		public void VerifyDecryption_OperationCancelled_ThrowsGpgError()
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithStatusMessage(GpgStatusCode.DECRYPTION_FAILED, null)
				.WithStdErrMessage("Operation cancelled")
				.Build();

			Should.Throw<GpgError>(() => verifier.VerifyDecryption(result));
		}

		[Fact]
		public void VerifyDecryption_GenericDecryptionFailed_ThrowsGpgError()
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithStatusMessage(GpgStatusCode.DECRYPTION_FAILED, null)
				.Build();

			Should.Throw<GpgError>(() => verifier.VerifyDecryption(result));
		}

		[Fact]
		public void VerifyDecryption_GenericFailure_ThrowsGpgException()
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithStatusMessage(GpgStatusCode.FAILURE, null)
				.Build();

			Should.Throw<GpgException>(() => verifier.VerifyDecryption(result));
		}

		[Fact]
		public void VerifyDecryption_DecryptionStatusOkay_DoesNotThrow()
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithStatusMessage(GpgStatusCode.DECRYPTION_OKAY, null)
				.WithStatusMessage(GpgStatusCode.END_DECRYPTION, null)
				.Build();

			Should.NotThrow(() => verifier.VerifyDecryption(result));
		}

		// Corner case: If GPG has warnings (for instance about the key store being of a newer version)
		// but is still able to decrypt the data successfully, it may still exit with a non-zero exit code.
		// Decryption should succeed here.
		[Fact]
		public void VerifyDecryption_NonZeroExitCodeButDecryptionStatusOkay_DoesNotThrow()
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithStatusMessage(GpgStatusCode.DECRYPTION_OKAY, null)
				.WithStatusMessage(GpgStatusCode.END_DECRYPTION, null)
				.WithExitCode(2)
				.Build();

			Should.NotThrow(() => verifier.VerifyDecryption(result));
		}
		
		[Fact]
		public void VerifyEncryption_ZeroExitCode_DoesNotThrow()
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithExitCode(0)
				.Build();

			Should.NotThrow(() => verifier.VerifyEncryption(result));
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(1)]
		[InlineData(1000)]
		public void VerifyEncryption_NonZeroExitCode_ThrowsGpgException(int exitCode)
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithExitCode(exitCode)
				.Build();

			Should.Throw<GpgException>(() => verifier.VerifyEncryption(result));
		}

		[Fact]
		public void VerifyEncryption_KeyExpired_ThrowsGpgError()
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithStatusMessage(GpgStatusCode.FAILURE)
				.WithStatusMessage(GpgStatusCode.INV_RECP, "rcp_a rcp_b")
				.WithStatusMessage(GpgStatusCode.KEYEXPIRED)
				.Build();

			var error = Should.Throw<GpgError>(() => verifier.VerifyEncryption(result));
		}

		[Fact]
		public void VerifyEncryption_InvalidRecipient_ThrowsGpgError()
		{
			var verifier = new GpgResultVerifier();
			var result = new GpgResultBuilder()
				.WithStatusMessage(GpgStatusCode.FAILURE)
				.WithStatusMessage(GpgStatusCode.INV_RECP, "rcp_a rcp_b")
				.Build();

			Should.Throw<GpgError>(() => verifier.VerifyEncryption(result));
		}
	}
}
