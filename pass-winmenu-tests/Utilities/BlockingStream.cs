using System;
using System.IO;
using System.Threading.Tasks;

namespace PassWinmenuTests.Utilities
{
	public class BlockingStream : Stream
	{
		private readonly TimeSpan blockTime;

		public BlockingStream(TimeSpan blockTime)
		{
			this.blockTime = blockTime;
		}

		public override void Flush()
		{

		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			Task.Delay(blockTime).Wait();
			return 0;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			Task.Delay(blockTime).Wait();
		}

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotImplementedException();
		public override long Position
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}
	}

}
