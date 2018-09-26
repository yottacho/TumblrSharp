using DontPanic.TumblrSharp.OAuth;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace DontPanic.TumblrSharp.Client
{
	/// <summary>
	/// Encapsulates the Tumblr API.
	/// </summary>
	public class TumblrClientWithoutApiKey : TumblrClient
    {
		private bool disposed;
		//private readonly string apiKey;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="TumblrClient"/> class.
		/// </summary>
        /// <param name="hashProvider">
        /// A <see cref="IHmacSha1HashProvider"/> implementation used to generate a
        /// HMAC-SHA1 hash for OAuth purposes.
        /// </param>
		/// <param name="consumerKey">
		/// The consumer key.
		/// </param>
		/// <param name="consumerSecret">
		/// The consumer secret.
		/// </param>
		/// <param name="oAuthToken">
		/// An optional access token for the API. If no access token is provided, only the methods
		/// that do not require OAuth can be invoked successfully.
		/// </param>
		/// <remarks>
		///  You can get a consumer key and a consumer secret by registering an application with Tumblr:<br/>
		/// <br/>
		/// http://www.tumblr.com/oauth/apps
		/// </remarks>
        public TumblrClientWithoutApiKey(IHmacSha1HashProvider hashProvider, string consumerKey, string consumerSecret, Token oAuthToken = null)
			: base(hashProvider, consumerKey, consumerSecret, oAuthToken)
		{
			//this.apiKey = consumerKey; 
		}

		#region Public Methods

		#region Blog Methods

		#region GetBlogInfoAsync

		/// <summary>
		/// Asynchronously retrieves general information about the blog, such as the title, number of posts, and other high-level data.
		/// </summary>
		/// <remarks>
		/// See: http://www.tumblr.com/docs/en/api/v2#blog-info.
		/// </remarks>
		/// <param name="blogName">
		/// The name of the blog.
		/// </param>
		/// <returns>
		/// A <see cref="Task{T}"/> that can be used to track the operation. If the task succeeds, the <see cref="Task{T}.Result"/> will
		/// carry a <see cref="BlogInfo"/> instance. Otherwise <see cref="Task.Exception"/> will carry a <see cref="TumblrException"/>
		/// representing the error occurred during the call.
		/// </returns>
		/// <exception cref="ObjectDisposedException">
		/// The object has been disposed.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="blogName"/> is <b>null</b>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="blogName"/> is empty.
		/// </exception>
		public new Task<BlogInfo> GetBlogInfoAsync(string blogName)
		{
			if (disposed)
				throw new ObjectDisposedException("TumblrClient");

			if (blogName == null)
				throw new ArgumentNullException("blogName");

			if (blogName.Length == 0)
				throw new ArgumentException("Blog name cannot be empty.", "blogName");

			MethodParameterSet parameters = new MethodParameterSet();
			//parameters.Add("api_key", apiKey);

			return CallApiMethodAsync<BlogInfoResponse, BlogInfo>(
				new BlogMethod(blogName, "info", OAuthToken, HttpMethod.Get, parameters),
				r => r.Blog,
				CancellationToken.None);
		}

		#endregion

		#region GetPostsAsync

		/// <summary>
		/// Asynchronously retrieves published posts from a blog.
		/// </summary>
		/// <remarks>
		/// See: http://www.tumblr.com/docs/en/api/v2#posts
		/// </remarks>
		/// <param name="blogName">
		/// The name of the blog.
		/// </param>
		/// <param name="startIndex">
		/// The offset at which to start retrieving the posts. Use 0 to start retrieving from the latest post.
		/// </param>
		/// <param name="count">
		/// The number of posts to retrieve. Must be between 1 and 20.
		/// </param>
		/// <param name="type">
		/// The <see cref="PostType"/> to retrieve.
		/// </param>
		/// <param name="includeReblogInfo">
		/// Whether or not to include reblog info with the posts.
		/// </param>
		/// <param name="includeNotesInfo">
		/// Whether or not to include notes info with the posts.
		/// </param>
		/// <param name="filter">
		/// A <see cref="PostFilter"/> to apply.
		/// </param>
		/// <param name="tag">
		/// A tag to filter by.
		/// </param>
		/// <returns>
		/// A <see cref="Task{T}"/> that can be used to track the operation. If the task succeeds, the <see cref="Task{T}.Result"/> will
		/// carry a <see cref="Posts"/> instance. Otherwise <see cref="Task.Exception"/> will carry a <see cref="TumblrException"/>
		/// representing the error occurred during the call.
		/// </returns>
		/// <exception cref="ObjectDisposedException">
		/// The object has been disposed.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="blogName"/> is <b>null</b>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="blogName"/> is empty.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <list type="bullet">
		/// <item>
		///		<description>
		///			<paramref name="startIndex"/> is less than 0.
		///		</description>
		///	</item>
		///	<item>
		///		<description>
		///			<paramref name="count"/> is less than 1 or greater than 20.
		///		</description>
		///	</item>
		/// </list>
		/// </exception>
		public new Task<Posts> GetPostsAsync(string blogName, long startIndex = 0, int count = 20, PostType type = PostType.All, bool includeReblogInfo = false, bool includeNotesInfo = false, PostFilter filter = PostFilter.Html, string tag = null)
		{
			if (disposed)
				throw new ObjectDisposedException("TumblrClient");

			if (blogName == null)
				throw new ArgumentNullException("blogName");

			if (blogName.Length == 0)
				throw new ArgumentException("Blog name cannot be empty.", "blogName");

			if (startIndex < 0)
				throw new ArgumentOutOfRangeException("startIndex", "startIndex must be greater or equal to zero.");

			if (count < 1 || count > 20)
				throw new ArgumentOutOfRangeException("count", "count must be between 1 and 20.");

			string methodName = null;
			switch (type)
			{
				case PostType.Text: methodName = "posts/text"; break;
				case PostType.Quote: methodName = "posts/quote"; break;
				case PostType.Link: methodName = "posts/link"; break;
				case PostType.Answer: methodName = "posts/answer"; break;
				case PostType.Video: methodName = "posts/video"; break;
				case PostType.Audio: methodName = "posts/audio"; break;
				case PostType.Photo: methodName = "posts/photo"; break;
				case PostType.Chat: methodName = "posts/chat"; break;
				case PostType.All:
				default: methodName = "posts"; break;
			}

			MethodParameterSet parameters = new MethodParameterSet();
			//parameters.Add("api_key", apiKey);
			parameters.Add("offset", startIndex, 0);
			parameters.Add("limit", count, 0);
			parameters.Add("reblog_info", includeReblogInfo, false);
			parameters.Add("notes_info", includeNotesInfo, false);
			parameters.Add("filter", filter.ToString().ToLowerInvariant(), "html");
			parameters.Add("tag", tag);

			return CallApiMethodAsync<Posts>(
				new BlogMethod(blogName, methodName, null, HttpMethod.Get, parameters),
				CancellationToken.None);
		}

		#endregion

		#region GetPostAsync

		/// <summary>
		/// Asynchronously retrieves a specific post by id.
		/// </summary>
		/// <param name="blogName">
		/// Blog name to reference
		/// </param>
		/// <param name="id">
		/// The id of the post to retrieve.
		/// </param>
		/// <param name="includeReblogInfo">
		/// Whether or not to include reblog info with the posts.
		/// </param>
		/// <param name="includeNotesInfo">
		/// Whether or not to include notes info with the posts.
		/// </param>
		/// <returns>
		/// A <see cref="Task{T}"/> that can be used to track the operation. If the task succeeds, the <see cref="Task{T}.Result"/> will
		/// carry a <see cref="BasePost"/> instance representing the desired post. Otherwise <see cref="Task.Exception"/> will carry a 
		/// <see cref="TumblrException"/> if the post with the specified id cannot be found.
		/// </returns>
		/// <exception cref="ObjectDisposedException">
		/// The object has been disposed.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="blogName"/> is <b>null</b>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="blogName"/> is empty.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///	<paramref name="id"/> is less than 0.
		/// </exception>
		public new Task<BasePost> GetPostAsync(string blogName, long id, bool includeReblogInfo = false, bool includeNotesInfo = false)
		{
			if (disposed)
				throw new ObjectDisposedException("TumblrClient");

		    if (blogName == null)
		        throw new ArgumentNullException("blogName");

		    if (blogName.Length == 0)
		        throw new ArgumentException("Blog name cannot be empty.", "blogName");

			if (id < 0)
				throw new ArgumentOutOfRangeException("id", "id must be greater or equal to zero.");

			MethodParameterSet parameters = new MethodParameterSet();
			//parameters.Add("api_key", apiKey);
			parameters.Add("id", id, 0);
			parameters.Add("reblog_info", includeReblogInfo, false);
			parameters.Add("notes_info", includeNotesInfo, false);

			return CallApiMethodAsync<Posts, BasePost>(
				new BlogMethod(blogName, "posts", null, HttpMethod.Get, parameters),
				p => p.Result.FirstOrDefault(),
				CancellationToken.None);
		}

		#endregion

		#region GetBlogLikesAsync

		/// <summary>
		/// Asynchronously retrieves the publicly exposed likes from a blog.
		/// </summary>
		/// <remarks>
		/// See: http://www.tumblr.com/docs/en/api/v2#blog-likes
		/// </remarks>
		/// <param name="blogName">
		/// The name of the blog.
		/// </param>
		/// <param name="startIndex">
		/// The offset at which to start retrieving the likes. Use 0 to start retrieving from the latest like.
		/// </param>
		/// <param name="count">
		/// The number of likes to retrieve. Must be between 1 and 20.
		/// </param>
		/// <returns>
		/// A <see cref="Task{T}"/> that can be used to track the operation. If the task succeeds, the <see cref="Task{T}.Result"/> will
		/// carry a <see cref="Likes"/> instance. Otherwise <see cref="Task.Exception"/> will carry a <see cref="TumblrException"/>
		/// representing the error occurred during the call.
		/// </returns>
		/// <exception cref="ObjectDisposedException">
		/// The object has been disposed.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="blogName"/> is <b>null</b>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="blogName"/> is empty.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <list type="bullet">
		/// <item>
		///		<description>
		///			<paramref name="startIndex"/> is less than 0.
		///		</description>
		///	</item>
		///	<item>
		///		<description>
		///			<paramref name="count"/> is less than 1 or greater than 20.
		///		</description>
		///	</item>
		/// </list>
		/// </exception>
		public new Task<Likes> GetBlogLikesAsync(string blogName, int startIndex = 0, int count = 20)
		{
			if (disposed)
				throw new ObjectDisposedException("TumblrClient");

			if (blogName == null)
				throw new ArgumentNullException("blogName");

			if (blogName.Length == 0)
				throw new ArgumentException("Blog name cannot be empty.", "blogName");

			if (startIndex < 0)
				throw new ArgumentOutOfRangeException("startIndex", "startIndex must be greater or equal to zero.");

			if (count < 1 || count > 20)
				throw new ArgumentOutOfRangeException("count", "count must be between 1 and 20.");

			MethodParameterSet parameters = new MethodParameterSet();
			//parameters.Add("api_key", apiKey);
			parameters.Add("offset", startIndex, 0);
			parameters.Add("limit", count, 0);

			return CallApiMethodAsync<Likes>(
				new BlogMethod(blogName, "likes", null, HttpMethod.Get, parameters),
				CancellationToken.None);
		}

		#endregion

		#endregion

		#region User Methods

        #region GetTaggedPostsAsync

        /// <summary>
        /// Asynchronously retrieves posts that have been tagged with a specific <paramref name="tag"/>.
        /// </summary>
        /// <remarks>
        /// See: http://www.tumblr.com/docs/en/api/v2#m-up-tagged
        /// </remarks>
        /// <param name="tag">
        /// The tag on the posts to retrieve.
        /// </param>
        /// <param name="before">
        /// The timestamp of when to retrieve posts before. 
        /// </param>
        /// <param name="count">
        /// The number of posts to retrieve.
        /// </param>
        /// <param name="filter">
        /// A <see cref="PostFilter"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task{T}"/> that can be used to track the operation. If the task succeeds, the <see cref="Task{T}.Result"/> will
        /// carry an array of posts. Otherwise <see cref="Task.Exception"/> will carry a <see cref="TumblrException"/>
        /// representing the error occurred during the call.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// The object has been disposed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="tag"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="tag"/> is empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// This <see cref="TumblrClient"/> instance does not have an OAuth token specified.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="count"/> is less than 1 or greater than 20.
        /// </exception>
        public new Task<BasePost[]> GetTaggedPostsAsync(string tag, DateTime? before = null, int count = 20, PostFilter filter = PostFilter.Html)
		{
			if (disposed)
				throw new ObjectDisposedException("TumblrClient");

			if (tag == null)
				throw new ArgumentNullException("tag");

			if (tag.Length == 0)
				throw new ArgumentException("Tag cannot be empty.", "tag");

			if (count < 1 || count > 20)
				throw new ArgumentOutOfRangeException("count", "count must be between 1 and 20.");

			MethodParameterSet parameters = new MethodParameterSet();
			//parameters.Add("api_key", apiKey);
			parameters.Add("tag", tag);
			parameters.Add("before", before.HasValue ? DateTimeHelper.ToTimestamp(before.Value).ToString() : null, null);
			parameters.Add("limit", count, 0);
			parameters.Add("filter", filter.ToString().ToLowerInvariant(), "html");

			return CallApiMethodAsync<BasePost[]>(
				new ApiMethod("https://api.tumblr.com/v2/tagged", OAuthToken, HttpMethod.Get, parameters),
				CancellationToken.None,
				new JsonConverter[] { new PostArrayConverter() });
		}

        #endregion

		#endregion

        #endregion

		#region IDisposable Implementation

		/// <summary>
		/// Disposes of the object.
		/// </summary>
		/// <param name="disposing">
		/// <b>true</b> if managed resources have to be disposed; otherwise <b>false</b>.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			disposed = true;
			base.Dispose();
		}

        #endregion
    }
}
