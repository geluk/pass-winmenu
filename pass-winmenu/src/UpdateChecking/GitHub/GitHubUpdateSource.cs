﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassWinmenu.UpdateChecking
{
	internal class GitHubUpdateSource : IUpdateSource
	{
		public ProgramVersion GetLatestVersion()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ProgramVersion> GetAllReleases()
		{
			throw new NotImplementedException();
		}

		private void FetchReleases()
		{
			
		}
	}
}
