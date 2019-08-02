using PassWinmenu.ExternalPrograms.Gpg;
using Shouldly;
using Xunit;

namespace PassWinmenuTests.Utilities
{
	public class StatusMessageTests
	{
		[Fact]
		public void Constructor_SetsProperties()
		{
			var statusCode = "SUCCESS";
			var messageText = "message";

			var message = new StatusMessage(statusCode, messageText);

			message.ShouldSatisfyAllConditions(
				() => message.StatusCode.ShouldBe(GpgStatusCode.SUCCESS),
				() => message.RawStatusCode.ShouldBe(statusCode),
				() => message.Message.ShouldBe(messageText)
			);
		}

		[Fact]
		public void Constructor_UnknownStatusCode_SetsUnknownStatusCode()
		{
			var statusCode = "NONEXISTENT_STATUS_CODE";

			var message = new StatusMessage(statusCode, null);

			message.ShouldSatisfyAllConditions(
				() => message.StatusCode.ShouldBe(GpgStatusCode.UnknownStatusCode),
				() => message.RawStatusCode.ShouldBe(statusCode)
			);
		}

		[Fact]
		public void ToString_IncludesRawStatusCodeAndMessage()
		{
			var statusCode = "NONEXISTENT_STATUS_CODE";
			var messageText = "message";
			var message = new StatusMessage(statusCode, messageText);

			var toString = message.ToString();

			toString.ShouldBe($"[{statusCode}] {messageText}");
		}
	}
}
