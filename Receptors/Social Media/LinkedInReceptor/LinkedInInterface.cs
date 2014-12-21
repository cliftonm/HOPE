/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

using LinkedIn.NET;
using LinkedIn.NET.Options;
using LinkedIn.NET.Groups;

namespace LinkedInReceptor
{
	public class LinkedInInterface : BaseReceptor
	{
		public override string Name { get { return "Linked In"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		protected LinkedInClient linkedInClient;
		protected string consumerKey;
		protected string consumerSecret;
		protected string accessToken;
		protected bool haveAccessToken;
		protected LinkedInGroupPost selectedPost;
		protected Dictionary<string, LinkedInGroup> groups;
		protected Dictionary<string, LinkedInGroup> postGroup;
		protected Dictionary<string, LinkedInGroupPost> posts;

		public LinkedInInterface(IReceptorSystem rsys)
			: base(rsys)
		{
			groups = new Dictionary<string, LinkedInGroup>();
			posts = new Dictionary<string, LinkedInGroupPost>();
			postGroup = new Dictionary<string, LinkedInGroup>();
			LoadConfiguration();
			InitializeClient();
			AddEmitProtocol("ExceptionMessage");
			AddEmitProtocol("LinkedInGroup");
			AddEmitProtocol("LinkedInPost");
			AddEmitProtocol("LinkedInComment");
			AddReceiveProtocol("QueryPosts", (Action<dynamic>)(signal => GetPosts(signal.LinkedInGroup.Id)));
			AddReceiveProtocol("QueryComments", (Action<dynamic>)(signal => GetComments(signal.LinkedInPost.Id)));			 
		}

		public override async void EndSystemInit()
		{
			base.EndSystemInit();

			if (String.IsNullOrEmpty(accessToken))
			{
				Authenticate();
			}
			else
			{
				try
				{
					await Task.Run(() => LoadAllGroups());

					foreach (LinkedInGroup group in groups.Values)
					{
						CreateCarrier("LinkedInGroup", signal =>
							{
								signal.Id = group.Id;
								signal.Name.Text.Value = group.Name;
							});
					}
				}
				catch (Exception ex)
				{
					EmitException(ex);
				}
			}
		}

		protected void LoadConfiguration()
		{
			try
			{
				string[] linkedInConfig = File.ReadAllLines("linkedin.config");
				consumerKey = linkedInConfig[0];
				consumerSecret = linkedInConfig[1];

				if (linkedInConfig.Length == 3)
				{
					accessToken = linkedInConfig[2];
					haveAccessToken = true;
				}
			}
			catch (Exception ex)
			{
				EmitException("LinkedIn.config file is missing or corrupt." + "\r\n" + ex.Message);
			}
		}

		protected void InitializeClient()
		{
			if ((!String.IsNullOrEmpty(consumerKey)) && (!String.IsNullOrEmpty(consumerSecret)))
			{
				linkedInClient = new LinkedInClient(consumerKey, consumerSecret);

				if (haveAccessToken)
				{
					linkedInClient.AccessToken = accessToken;
				}
			}
		}

		protected void Authenticate()
		{
		}

		/// <summary>
		/// Load synchronously.  Call with a await Task.Run(()=>LoadAllGroups()); to run async. 
		/// </summary>
		protected void LoadAllGroups()
		{
			if (haveAccessToken)
			{
				LinkedInGetGroupOptions options = new LinkedInGetGroupOptions();
				options.GroupOptions.SelectAll();
				LinkedInResponse<IEnumerable<LinkedInGroup>> groupResult = linkedInClient.GetMemberGroups(options);
				groupResult.Result.ForEach(g => groups[g.Id] = g);
			}
		}

		protected async void GetPosts(string groupId)
		{
			LinkedInGroup group = groups[groupId];
			LinkedInGetGroupPostsOptions postOptions = new LinkedInGetGroupPostsOptions();
			postOptions.PostOptions.SelectAll();
			postOptions.GroupId = group.Id;
			await Task.Run(()=>group.LoadPosts(postOptions));

			foreach (LinkedInGroupPost post in group.Posts)
			{
				if (!posts.ContainsKey(post.Id))
				{
					posts[post.Id] = post;
					postGroup[post.Id] = group;
				}

				CreateCarrier("LinkedInPost", signal=>
					{
						signal.LinkedInGroup.Name.Text.Value = group.Name;
						signal.LinkedInGroup.Id = group.Id;
						signal.Id = post.Id;
						signal.CreationTime = post.CreationTime;
						signal.Summary.Text.Value = post.Summary;
						signal.Title.Text.Value = post.Title;
						signal.LinkedInPostCreator.PersonName.Name.Text.Value = post.Creator.FirstName +" " + post.Creator.LastName;
					});
			}
		}

		protected async void GetComments(string postId)
		{
			LinkedInGroupPost post = posts[postId];
			LinkedInGroup group = postGroup[post.Id];
			LinkedInGetGroupPostCommentsOptions commentOptions = new LinkedInGetGroupPostCommentsOptions();
			commentOptions.CommentOptions.SelectAll();
			commentOptions.PostId = post.Id;
			await Task.Run(() => post.LoadComments(commentOptions));

			foreach (LinkedInGroupComment comment in post.Comments)
			{
				CreateCarrier("LinkedInComment", signal =>
					{
						signal.LinkedInGroup.Name.Text.Value = group.Name;
						signal.LinkedInGroup.Id = group.Id;
						signal.LinkedInPost.Title.Text.Value = post.Title;
						signal.LinkedInPost.Id = post.Id;
						signal.CreationTime = comment.CreationTime;
						signal.Comment = comment.Text;
						signal.LinkedInCommentCreator.PersonName.Name.Text.Value = comment.Creator.FirstName + " " + comment.Creator.LastName;
					});
			}
		}
/*
				foreach (LinkedInGroup group in groups.Result)
				{
					LinkedInGetGroupPostsOptions postOptions = new LinkedInGetGroupPostsOptions();
					postOptions.PostOptions.SelectAll();
					postOptions.GroupId = group.Id;
					group.LoadPosts(postOptions);

					foreach (LinkedInGroupPost post in group.Posts)
					{
						LinkedInGetGroupPostCommentsOptions commentOptions = new LinkedInGetGroupPostCommentsOptions();
						commentOptions.CommentOptions.SelectAll();
						commentOptions.PostId = post.Id;
						post.LoadComments(commentOptions);

						foreach (LinkedInGroupComment comment in post.Comments)
						{
							GroupPostComment c = new GroupPostComment() { Group = group, Post = post, Comment = comment };
							comments.Add(c);
						}
					}
				}
			}
 */ 

		protected void AddComment(LinkedInGroupPost post, string comment)
		{
			post.Comment(comment);
		}

		protected void ReRun(LinkedInResponseStatus status, string message)
		{
			switch (status)
			{
				case LinkedInResponseStatus.ExpiredToken:
				case LinkedInResponseStatus.InvalidAccessToken:
				case LinkedInResponseStatus.UnauthorizedAction:
					Authenticate();
					break;

				default:
					MessageBox.Show(message);
					break;
			}
		}
	}
}
