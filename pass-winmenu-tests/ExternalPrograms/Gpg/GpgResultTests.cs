using PassWinmenu.ExternalPrograms.Gpg;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.ExternalPrograms.Gpg
{
	public class GpgResultTests
	{
		[Fact]
		public void GenerateError_IncludesStderrMessages()
		{
			var stdErrMessages = new[] {"message_1", "message_2"};
			var result = new GpgResult(0, null, new StatusMessage[0], stdErrMessages);

			try
			{
				result.GenerateError();
			}
			catch (GpgException e)
			{
				e.ShouldSatisfyAllConditions(
					() => e.Message.ShouldContain(stdErrMessages[0]),
					() => e.Message.ShouldContain(stdErrMessages[1])
				);
			}
		}
	}
}
