using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Moq;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenuTests.Utilities;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.ExternalPrograms.Gpg
{
	public class GpgAgentTests
	{
		private readonly Mock<IProcesses> processes = new Mock<IProcesses>();


		[Fact]
		public void EnsureAgentResponsive_NoAgentRunning_ReturnsWithoutAction()
		{
			var installation = new GpgInstallationBuilder().Build();
			var agent = new GpgAgent(processes.Object, installation);

			agent.EnsureAgentResponsive();

			processes.Verify(p => p.GetProcessesByName(It.IsAny<string>()));
			processes.VerifyNoOtherCalls();
		}

		[Fact]
		public void EnsureAgentResponsive_ConnectAgentDoesNotExist_DoesNotThrow()
		{
			AddRunningAgentProcess();
			var installation = new GpgInstallationBuilder().Build();
			var agent = new GpgAgent(processes.Object, installation);
			installation.GpgConnectAgentExecutable.Delete();

			Should.NotThrow(() => agent.EnsureAgentResponsive());
		}

		[Fact]
		public void EnsureAgentResponsive_StartConnectAgentThrows_CatchesException()
		{
			AddRunningAgentProcess();
			processes.Setup(p => p.Start(It.IsAny<ProcessStartInfo>())).Throws<Exception>();
			var installation = new GpgInstallationBuilder().Build();
			var agent = new GpgAgent(processes.Object, installation);

			Should.NotThrow(() => agent.EnsureAgentResponsive());
		}

		[Fact]
		public void EnsureAgentResponsive_AgentRunning_StartsConnectAgent()
		{
			AddRunningAgentProcess();
			processes.Setup(p => p.Start(It.IsAny<ProcessStartInfo>())).Returns(() => new FakeProcess());
			var installation = new GpgInstallationBuilder().Build();
			var agent = new GpgAgent(processes.Object, installation);

			agent.EnsureAgentResponsive();

			processes.Verify(p => p.GetProcessesByName(It.IsAny<string>()));
			processes.Verify(p => p.Start(It.Is<ProcessStartInfo>(
				info => info.FileName == installation.GpgConnectAgentExecutable.FullName
			)));
		}

		[Theory]
		[InlineData("waiting for agent")]
		[InlineData("no running gpg-agent")]
		[InlineData("other output")]
		public void EnsureAgentResponsive_ConnectWaitsForAgentAndExits_Returns(string connectAgentOutput)
		{
			AddRunningAgentProcess();
			processes.Setup(p => p.Start(It.IsAny<ProcessStartInfo>()))
				.Returns(() => new FakeProcessBuilder().WithStandardError(connectAgentOutput).Build());
			var installation = new GpgInstallationBuilder().Build();
			var agent = new GpgAgent(processes.Object, installation);

			agent.EnsureAgentResponsive();

			processes.Verify(p => p.GetProcessesByName(It.IsAny<string>()));
			processes.Verify(p => p.Start(It.Is<ProcessStartInfo>(
				info => info.FileName == installation.GpgConnectAgentExecutable.FullName
			)));
		}

		[Fact]
		public void EnsureAgentResponsive_AgentDoesNotRespond_KillsConnectAgent()
		{
			// Arrange
			AddRunningAgentProcess();
			var connectAgentProcessMock = new Mock<IProcess>();
			connectAgentProcessMock.Setup(p => p.StandardError).Returns(CreateBlockingStreamReader());

			processes.Setup(p => p.Start(It.IsAny<ProcessStartInfo>()))
				.Returns(() => connectAgentProcessMock.Object);
			var installation = new GpgInstallationBuilder().Build();
			var agent = new GpgAgent(processes.Object, installation);

			// Act
			agent.EnsureAgentResponsive();

			// Assert
			connectAgentProcessMock.Verify(p => p.Kill(), Times.Once);
		}

		[Fact]
		public void EnsureAgentResponsive_ConnectAgentReadsButDoesNotExit_KillsConnectAgent()
		{
			// Arrange
			AddRunningAgentProcess();
			var connectAgentProcessMock = new Mock<IProcess>();
			connectAgentProcessMock.Setup(c => c.WaitForExit(It.IsAny<TimeSpan>())).Returns(false);
			connectAgentProcessMock.Setup(p => p.StandardError).Returns(new StreamReader(new MemoryStream(new byte[]{10})));

			processes.Setup(p => p.Start(It.IsAny<ProcessStartInfo>()))
				.Returns(() => connectAgentProcessMock.Object);
			var installation = new GpgInstallationBuilder().Build();
			var agent = new GpgAgent(processes.Object, installation);

			// Act
			agent.EnsureAgentResponsive();

			// Assert
			connectAgentProcessMock.Verify(p => p.Kill(), Times.Once);
		}

		[Fact]
		public void EnsureAgentResponsive_AgentDoesNotRespond_KillsRunningAgents()
		{
			// Arrange
			var installation = new GpgInstallationBuilder().Build();
			var runningAgentMocks = new[]
			{
				new Mock<IProcess>(),
				new Mock<IProcess>(),
			};
			foreach (var mock in runningAgentMocks)
			{
				mock.Setup(p => p.MainModuleName).Returns(installation.GpgAgentExecutable.FullName);
			}

			processes.Setup(p => p.GetProcessesByName("gpg-agent")).Returns(runningAgentMocks.Select(m => m.Object).ToArray);
			processes.Setup(p => p.GetProcesses()).Returns(runningAgentMocks.Select(m => m.Object).ToArray);
			processes.Setup(p => p.Start(It.IsAny<ProcessStartInfo>())).Returns(CreateProcessWithBlockingStandardOutput);
			var agent = new GpgAgent(processes.Object, installation);

			// Act
			agent.EnsureAgentResponsive();

			// Assert
			foreach (var mock in runningAgentMocks)
			{
				mock.Verify(m => m.Kill(), Times.Once);
			}
		}

		[Fact]
		public void EnsureAgentResponsive_AgentDoesNotRespond_LeavesOtherAgentsAlive()
		{
			// Arrange
			var installation = new GpgInstallationBuilder().Build();
			var runningAgentMocks = new[]
			{
				new Mock<IProcess>(),
				new Mock<IProcess>(),
			};
			runningAgentMocks[0].Setup(p => p.MainModuleName).Returns(installation.GpgAgentExecutable.FullName);
			runningAgentMocks[1].Setup(p => p.MainModuleName).Returns(@"C:\other\gpg-agent.exe");

			processes.Setup(p => p.GetProcessesByName("gpg-agent")).Returns(runningAgentMocks.Select(m => m.Object).ToArray);
			processes.Setup(p => p.GetProcesses()).Returns(runningAgentMocks.Select(m => m.Object).ToArray);
			processes.Setup(p => p.Start(It.IsAny<ProcessStartInfo>())).Returns(CreateProcessWithBlockingStandardOutput);
			var agent = new GpgAgent(processes.Object, installation);

			// Act
			agent.EnsureAgentResponsive();

			// Assert
			runningAgentMocks[0].Verify(m => m.Kill(), Times.Once);
			runningAgentMocks[1].Verify(m => m.Kill(), Times.Never);
		}

		private IProcess CreateProcessWithBlockingStandardOutput()
		{
			var blockingProcess = new Mock<IProcess>();
			blockingProcess.Setup(c => c.WaitForExit(It.IsAny<TimeSpan>())).Returns(false);
			blockingProcess.Setup(p => p.StandardError).Returns(CreateBlockingStreamReader());
			return blockingProcess.Object;
		}

		private StreamReader CreateBlockingStreamReader()
		{
			return new StreamReader(new BlockingStream(TimeSpan.FromSeconds(5)));
		}

		private void AddRunningAgentProcess()
		{
			processes.Setup(p => p.GetProcessesByName("gpg-agent")).Returns(new IProcess[] { new FakeProcess() });
		}
	}
}
