using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using McSherry.SemanticVersioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassWinmenu.UpdateChecking;

namespace PassWinmenu.UdateChecking
{
	[TestClass]
	public class UpdateCheckerUnitTests
	{
		private const string Category = "Updates: update sources";

		[TestMethod, TestCategory(Category)]
		public void UpdateChecker_SendsEvent()
		{
			var source = new DummyUpdateSource
			{
				LatestVersion = new ProgramVersion
				{
					VersionNumber = new SemanticVersion(0, 0, 0)
				}
			};

			var checker = new UpdateChecker(source, new SemanticVersion(1, 0, 0))
			{
				CheckInterval = TimeSpan.FromMilliseconds(1)
			};

			var raised = false;
			checker.UpdateAvailable += (sender, args) =>
			{
				raised = true;
			};
			checker.Start();

			// Allow the update checker to raise the event.
			Thread.Sleep(100);

			Assert.IsTrue(raised);
		}


	}
}
