using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using McSherry.SemanticVersioning;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PassWinmenu.UpdateChecking.GitHub
{
	internal class GitHubUpdateSource : IUpdateSource
	{
		private const string UpdateUrl = "https://api.github.com/repos/Baggykiin/pass-winmenu/releases";
		private readonly JsonSerializerSettings settings;

		public bool RequiresConnectivity => true;

		public GitHubUpdateSource()
		{
			settings = new JsonSerializerSettings
			{
				ContractResolver = new DefaultContractResolver
				{
					NamingStrategy = new SnakeCaseNamingStrategy()
				}
			};
		}

		public ProgramVersion GetLatestVersion()
		{
			var response = FetchReleases();
			var latest = response.OrderByDescending(r => r.Version).First();

			var version = new ProgramVersion
			{
				VersionNumber = latest.Version,
				DownloadLink = new Uri(latest.HtmlUrl),
				ReleaseDate = latest.PublishedAt,
				ReleaseNotes = new Uri(latest.HtmlUrl),
			};
			return version;
		}

		public IEnumerable<ProgramVersion> GetAllReleases()
		{
			throw new NotImplementedException();
		}

		private Release[] FetchReleases()
		{
			var rq = WebRequest.CreateHttp(UpdateUrl);
			rq.ServerCertificateValidationCallback += ValidateCertificate;
			rq.UserAgent = $"pass-winmenu/{Program.Version}";

			using (var response = rq.GetResponse())
			using (var stream = response.GetResponseStream())
			{
				if (stream == null)
				{
					throw new UpdateException("Unable to fetch response stream.");
				}
				var responseText = new StreamReader(stream).ReadToEnd();
				return JsonConvert.DeserializeObject<Release[]>(responseText, settings);
			}

		}

		private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None) return true;
			
			Log.Send($"Server certificate failed to validate: {sslPolicyErrors.ToString()}", LogLevel.Warning);
			return false;
		}
	}
}
