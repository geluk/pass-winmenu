using Moq;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.ExternalPrograms.Gpg
{
	public class GpgTests
	{
		[Fact]
		public void Decrypt_ChecksAgentAlive()
		{
			var transportMock = new Mock<IGpgTransport>();
			transportMock.Setup(t => t.CallGpg(It.IsAny<string>(), null)).Returns(GetSuccessResult);
			var agentMock = new Mock<IGpgAgent>();
			var gpg = new GPG(transportMock.Object, agentMock.Object, StubGpgResultVerifier.AlwaysValid, false);

			gpg.Decrypt("file");

			agentMock.Verify(a => a.EnsureAgentResponsive(), Times.Once);
		}

		[Fact]
		public void Decrypt_CallsGpg()
		{
			var transportMock = new Mock<IGpgTransport>();
			transportMock.Setup(t => t.CallGpg(It.IsAny<string>(), null)).Returns(GetSuccessResult);
			var gpg = new GPG(transportMock.Object, Mock.Of<IGpgAgent>(), StubGpgResultVerifier.AlwaysValid, false);

			gpg.Decrypt("file");

			transportMock.Verify(t => t.CallGpg(It.IsAny<string>(), null), Times.Once);
		}

		[Theory]
		[InlineData("password")]
		[InlineData("password\nline2")]
		[InlineData("password\r\nline2")]
		[InlineData("\npassword\r\n")]
		public void Decrypt_ReturnsFileContent(string fileContent)
		{
			var transportMock = new Mock<IGpgTransport>();
			transportMock.Setup(
					t => t.CallGpg(It.IsNotNull<string>(), null))
				.Returns(new GpgResultBuilder()
					.WithStdout(fileContent)
					.Build());
			var gpg = new GPG(transportMock.Object, Mock.Of<IGpgAgent>(), StubGpgResultVerifier.AlwaysValid, false);

			var decryptedContent = gpg.Decrypt("file");

			decryptedContent.ShouldBe(fileContent);
		}

		[Fact]
		public void Encrypt_NoRecipients_CallsGpg()
		{
			var transportMock = new Mock<IGpgTransport>();
			transportMock.Setup(t => t.CallGpg(It.IsNotNull<string>(), It.IsNotNull<string>())).Returns(GetSuccessResult);
			var gpg = new GPG(transportMock.Object, Mock.Of<IGpgAgent>(), StubGpgResultVerifier.AlwaysValid, false);

			gpg.Encrypt("data", "file");

			transportMock.Verify(t => t.CallGpg(It.IsNotNull<string>(), It.IsNotNull<string>()), Times.Once);
		}

		[Fact]
		public void Encrypt_NullRecipients_CallsGpg()
		{
			var transportMock = new Mock<IGpgTransport>();
			transportMock.Setup(t => t.CallGpg(It.IsNotNull<string>(), It.IsNotNull<string>())).Returns(GetSuccessResult);
			var gpg = new GPG(transportMock.Object, Mock.Of<IGpgAgent>(), StubGpgResultVerifier.AlwaysValid, false);

			gpg.Encrypt("data", "file", null);

			transportMock.Verify(t => t.CallGpg(It.IsNotNull<string>(), It.IsNotNull<string>()), Times.Once);
		}

		[Fact]
		public void Encrypt_Recipients_CallsGpgWithRecipients()
		{
			var transportMock = new Mock<IGpgTransport>();
			transportMock.Setup(t => t.CallGpg(It.IsNotNull<string>(), It.IsNotNull<string>())).Returns(GetSuccessResult);
			var gpg = new GPG(transportMock.Object, Mock.Of<IGpgAgent>(), StubGpgResultVerifier.AlwaysValid, false);

			gpg.Encrypt("data", "file", "rcp_0", "rcp_1");

			transportMock.Verify(t => t.CallGpg(It.Is<string>(args =>
				args.Contains("rcp_0") && args.Contains("rcp_1")), It.IsNotNull<string>()), Times.Once);
		}

		[Fact]
		public void StartAgent_ChecksAgentAlive()
		{
			var transportMock = new Mock<IGpgTransport>();
			transportMock.Setup(
					t => t.CallGpg(It.IsNotNull<string>(), It.IsAny<string>()))
				.Returns(new GpgResultBuilder()
					.WithStdout("secret keys")
					.Build());
			var agentMock = new Mock<IGpgAgent>();
			var gpg = new GPG(transportMock.Object, agentMock.Object, StubGpgResultVerifier.AlwaysValid, false);

			gpg.StartAgent();

			agentMock.Verify(a => a.EnsureAgentResponsive(), Times.Once);
		}

		[Fact]
		public void StartAgent_CallsListSecretKeys()
		{
			var transportMock = new Mock<IGpgTransport>();
			transportMock.Setup(
					t => t.CallGpg(It.IsNotNull<string>(), It.IsAny<string>()))
				.Returns(new GpgResultBuilder()
					.WithStdout("secret keys")
					.Build());
			var gpg = new GPG(transportMock.Object, Mock.Of<IGpgAgent>(), StubGpgResultVerifier.AlwaysValid, false);

			gpg.StartAgent();

			transportMock.Verify(t => t.CallGpg(It.IsRegex("--list-secret-keys"), null), Times.Once);
		}

		[Fact]
		public void StartAgent_NoSecretKeys_ThrowsGpgError()
		{
			var transportMock = new Mock<IGpgTransport>();
			transportMock.Setup(
					t => t.CallGpg(It.IsNotNull<string>(), It.IsAny<string>()))
				.Returns(new GpgResultBuilder()
					.WithStdout("")
					.Build());
			var gpg = new GPG(transportMock.Object, Mock.Of<IGpgAgent>(), StubGpgResultVerifier.AlwaysValid, false);

			Should.Throw<GpgError>(() => gpg.StartAgent());
		}

		[Fact]
		public void GetVersion_ReturnsFirstOutputLine()
		{
			var transportMock = new Mock<IGpgTransport>();
			transportMock.Setup(
					t => t.CallGpg(It.IsNotNull<string>(), It.IsAny<string>()))
				.Returns(new GpgResultBuilder()
					.WithStdout("GPG version 1.0\r\nmore info")
					.Build());
			var gpg = new GPG(transportMock.Object, Mock.Of<IGpgAgent>(), StubGpgResultVerifier.AlwaysValid, false);

			var version = gpg.GetVersion();

			version.ShouldBe("GPG version 1.0");
		}

		private GpgResult GetSuccessResult()
		{
			return new GpgResultBuilder().Build();
		}
	}

	class StubGpgResultVerifier : IGpgResultVerifier
	{
		private readonly bool valid;

		private StubGpgResultVerifier(bool valid)
		{
			this.valid = valid;
		}

		public void VerifyDecryption(GpgResult result)
		{
			if (!valid)
			{
				throw new GpgError("Invalid result.");
			}
		}

		public void VerifyEncryption(GpgResult result)
		{
			if (!valid)
			{
				throw new GpgError("Invalid result.");
			}
		}

		public static IGpgResultVerifier AlwaysValid => new StubGpgResultVerifier(true);
	}
}
