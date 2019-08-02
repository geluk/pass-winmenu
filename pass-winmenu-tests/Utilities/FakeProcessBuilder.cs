using System;
using System.IO;
using System.Text;

namespace PassWinmenuTests.Utilities
{
	public class FakeProcessBuilder
	{
		private TimeSpan exitTime;
		private StreamReader standardError;
		private StreamReader standardOutput;
		private int exitCode;
		private Stream inputStream;

		public FakeProcess Build()
		{
			return new FakeProcess
			{
				ExitCode = exitCode,
				ExitTime = exitTime,
				StandardError = standardError,
				StandardOutput = standardOutput,
				StandardInput = inputStream == null ? null : new StreamWriter(inputStream)
			};
		}

		public FakeProcessBuilder WithStandardError(string standardError)
		{
			this.standardError = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(standardError)));
			return this;
		}

		public FakeProcessBuilder WithExitTime(TimeSpan exitTime)
		{
			this.exitTime = exitTime;
			return this;
		}

		public FakeProcessBuilder WithStandardOutput(string standardOutput)
		{
			this.standardOutput = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(standardOutput)));
			return this;
		}

		public FakeProcessBuilder WithExitCode(int exitCode)
		{
			this.exitCode = exitCode;
			return this;
		}

		public FakeProcessBuilder WithStandardInput(Stream inputStream)
		{
			this.inputStream = inputStream;
			return this;
		}
	}
}
