using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McSherry.SemanticVersioning;

namespace PassWinmenu.UpdateChecking.GitHub
{
	public class ReleasesResponse
	{
		public Release[] Releases { get; set; }
	}

	public class Release
	{
		public string Url { get; set; }
		public string AssetsUrl { get; set; }
		public string UploadUrl { get; set; }
		public string HtmlUrl { get; set; }
		public int Id { get; set; }
		public string NodeId { get; set; }
		public string TagName { get; set; }
		public string TargetCommitish { get; set; }
		public string Name { get; set; }
		public bool Draft { get; set; }
		public Author Author { get; set; }
		public bool Prerelease { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime PublishedAt { get; set; }
		public Asset[] Assets { get; set; }
		public string TarballUrl { get; set; }
		public string ZipballUrl { get; set; }
		public string Body { get; set; }
		public SemanticVersion Version => SemanticVersion.Parse(TagName, ParseMode.Lenient);
	}

	public class Author
	{
		public string Login { get; set; }
		public int Id { get; set; }
		public string NodeId { get; set; }
		public string AvatarUrl { get; set; }
		public string GravatarId { get; set; }
		public string Url { get; set; }
		public string HtmlUrl { get; set; }
		public string FollowersUrl { get; set; }
		public string FollowingUrl { get; set; }
		public string GistsUrl { get; set; }
		public string StarredUrl { get; set; }
		public string SubscriptionsUrl { get; set; }
		public string OrganizationsUrl { get; set; }
		public string ReposUrl { get; set; }
		public string EventsUrl { get; set; }
		public string ReceivedEventsUrl { get; set; }
		public string Type { get; set; }
		public bool SiteAdmin { get; set; }
	}

	public class Asset
	{
		public string Url { get; set; }
		public int Id { get; set; }
		public string NodeId { get; set; }
		public string Name { get; set; }
		public string Label { get; set; }
		public Uploader Uploader { get; set; }
		public string ContentType { get; set; }
		public string State { get; set; }
		public int Size { get; set; }
		public int DownloadCount { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string BrowserDownloadUrl { get; set; }
	}

	public class Uploader
	{
		public string Login { get; set; }
		public int Id { get; set; }
		public string NodeId { get; set; }
		public string AvatarUrl { get; set; }
		public string GravatarId { get; set; }
		public string Url { get; set; }
		public string HtmlUrl { get; set; }
		public string FollowersUrl { get; set; }
		public string FollowingUrl { get; set; }
		public string GistsUrl { get; set; }
		public string StarredUrl { get; set; }
		public string SubscriptionsUrl { get; set; }
		public string OrganizationsUrl { get; set; }
		public string ReposUrl { get; set; }
		public string EventsUrl { get; set; }
		public string ReceivedEventsUrl { get; set; }
		public string Type { get; set; }
		public bool SiteAdmin { get; set; }
	}

}
