using System.Diagnostics;
using System.IO;
using Moq;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.ExternalPrograms.Gpg
{
	public class GpgTransportTests
	{
		[Fact]
		public void CallGpg_SetsResultFromProcess()
		{
			var installation = new GpgInstallationBuilder().Build();
			var processes = new Mock<IProcesses>();
			processes.Setup(p => p.Start(It.IsAny<ProcessStartInfo>()))
				.Returns(new FakeProcessBuilder()
					.WithStandardOutput("stdout data")
					.WithStandardError("stderr message 1\r\nstderr message 2")
					.WithExitCode(10)
					.Build());
			var transport = new GpgTransport(new StubHomedirResolver(@"C:\gpghome"), installation, processes.Object);

			var result = transport.CallGpg("--decrypt");

			result.ShouldSatisfyAllConditions(
				() => result.Stdout.ShouldBe("stdout data"),
				() => result.StderrMessages.ShouldBe(new[] {"stderr message 1", "stderr message 2"}),
				() => result.ExitCode.ShouldBe(10)
			);
		}

		[Fact]
		public void CallGpg_ProcessOutputsStatusMessages_AddsStatusMessagesToResult()
		{
			var installation = new GpgInstallationBuilder().Build();
			var processes = new Mock<IProcesses>();
			processes.Setup(p => p.Start(It.IsAny<ProcessStartInfo>()))
				.Returns(new FakeProcessBuilder()
					.WithStandardOutput("")
					.WithStandardError("[GNUPG:] ENC_TO keyId\r\n[GNUPG:] DECRYPTION_OKAY")
					.Build());
			var transport = new GpgTransport(new StubHomedirResolver(@"C:\gpghome"), installation, processes.Object);

			var result = transport.CallGpg("--decrypt");

			result.StatusMessages.Length.ShouldBe(2);
			result.StatusMessages.ShouldSatisfyAllConditions(
				() => result.StatusMessages[0].StatusCode.ShouldBe(GpgStatusCode.ENC_TO),
				() => result.StatusMessages[0].Message.ShouldBe("keyId"),
				() => result.StatusMessages[1].StatusCode.ShouldBe(GpgStatusCode.DECRYPTION_OKAY),
				() => result.StatusMessages[1].Message.ShouldBe(null)
			);
		}

		[Fact]
		public void CallGpg_WithInput_WritesToProcessStandardInput()
		{
			var installation = new GpgInstallationBuilder().Build();
			var processes = new Mock<IProcesses>();
			var inputStream = new Mock<Stream>();
			inputStream.Setup(s => s.CanWrite).Returns(true);
			processes.Setup(p => p.Start(It.IsAny<ProcessStartInfo>()))
				.Returns(new FakeProcessBuilder()
					.WithStandardOutput("")
					.WithStandardError("")
					.WithStandardInput(inputStream.Object)
					.Build());
			var transport = new GpgTransport(new StubHomedirResolver(@"C:\gpghome"), installation, processes.Object);

			transport.CallGpg("--encrypt", "input");

			inputStream.Verify(s => s.Write(It.IsAny<byte[]>(), 0, 5));
		}
	}
}
