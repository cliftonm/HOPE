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
	public class LinkedInInterface : WindowedBaseReceptor
    {
		public override string Name { get { return "Linked In"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		protected LinkedInClient linkedInClient;
		protected string consumerKey;
		protected string consumerSecret;
		protected string accessToken;
		protected bool haveAccessToken;
		protected LinkedInGroupPost selectedPost;

		protected TreeView tvGroups;
		protected TextBox tbText;
		protected Button btnComment;

		public LinkedInInterface(IReceptorSystem rsys)
			: base("LinkedIn.xml", true, rsys)
		{
			LoadConfiguration();
			InitializeClient();
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();

			if (String.IsNullOrEmpty(accessToken))
			{
				Authenticate();
			}
			else
			{
				LoadGroups();
			}
		}

		protected override void InitializeUI()
		{
			base.InitializeUI();

			tvGroups = (TreeView)mycroParser.ObjectCollection["tvGroups"];
			tbText = (TextBox)mycroParser.ObjectCollection["tbText"];
			btnComment = (Button)mycroParser.ObjectCollection["btnComment"];
			tvGroups.NodeMouseClick += OnNodeMouseClick;
			tvGroups.NodeMouseDoubleClick += OnNodeMouseDoubleClick;
		}

		// Kludgy!!!
		protected TextBox tbCommentText;

		protected void AddNewComment(object sender, EventArgs args)
		{
			Clifton.MycroParser.MycroParser mycroParser = new Clifton.MycroParser.MycroParser();
			mycroParser.ObjectCollection["form"] = this;
			Form form = mycroParser.Load<Form>("LinkedInComment.xml", this);
			tbCommentText = (TextBox)mycroParser.ObjectCollection["tbCommentText"];
			form.ShowDialog();
		}

		protected void MakeComment(object sender, EventArgs args)
		{
			AddComment(selectedPost, tbCommentText.Text);
			((Form)((Control)sender).Parent).Close();
		}

		protected void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			const string CRLF="\r\n";
			StringBuilder sb = new StringBuilder();

			if (e.Node.Tag is LinkedInGroup)
			{
				LinkedInGroup group = (LinkedInGroup)e.Node.Tag;
				sb.Append("Group: "+group.Name);
				sb.Append(CRLF);
				sb.Append(CRLF);
				sb.Append("Category: " + group.Category);
				sb.Append(CRLF);
				sb.Append(CRLF);
				sb.Append("Description: " + group.ShortDescription);
				btnComment.Enabled = false;
			}
			else if (e.Node.Tag is LinkedInGroupPost)
			{
				LinkedInGroupPost post = (LinkedInGroupPost)e.Node.Tag;
				sb.Append("Title: " + post.Title);
				sb.Append(CRLF);
				sb.Append(CRLF);
				sb.Append("By: " + post.Creator.FirstName + " " + post.Creator.LastName);
				sb.Append(CRLF);
				sb.Append(CRLF);
				sb.Append("On: "+post.CreationTime.ToString());
				sb.Append(CRLF);
				sb.Append(CRLF);
				sb.Append("Summary: " + post.Summary);
				btnComment.Enabled = true;
				selectedPost = post;
			}
			else if (e.Node.Tag is LinkedInGroupComment)
			{
				LinkedInGroupComment comment = (LinkedInGroupComment)e.Node.Tag;
				sb.Append("By: " + comment.Creator.FirstName + " " + comment.Creator.LastName);
				sb.Append(CRLF);
				sb.Append(CRLF);
				sb.Append("On: " + comment.CreationTime.ToString());
				sb.Append(CRLF);
				sb.Append(CRLF);
				sb.Append(comment.Text);
				btnComment.Enabled = true;
			}

			tbText.Text = sb.ToString();
		}

		protected void OnNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Node.Tag is LinkedInGroup)
			{
				LinkedInGroup group = (LinkedInGroup)e.Node.Tag;
				LoadPostsForGroup(e.Node, group);
			}
			else if (e.Node.Tag is LinkedInGroupPost)
			{
				LinkedInGroupPost post = (LinkedInGroupPost)e.Node.Tag;
				LoadCommentsForPost(e.Node, post);
			}
		}

		protected void OnBeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
//			LinkedInGroup group = (LinkedInGroup)e.Node.Tag;
//			LoadPostsForGroup(e.Node, group);
			e.Cancel = false;
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

		protected void LoadGroups()
		{
			if (haveAccessToken)
			{
				tvGroups.Nodes.Clear();
				tvGroups.Nodes.Add("Loading...");
				LinkedInGetGroupOptions options = new LinkedInGetGroupOptions();
				options.GroupOptions.SelectAll();
				linkedInClient.GetMemberGroups(options, ShowMemberGroups);
			}
		}

		protected async void LoadPostsForGroup(TreeNode node, LinkedInGroup group)
		{
			LinkedInGetGroupPostsOptions options = new LinkedInGetGroupPostsOptions();
			options.PostOptions.SelectAll();
			options.GroupId = group.Id;
			ShowLoading(node);

			await Task.Run(() =>
				{
					group.LoadPosts(options);
				});

			// Async operation might complete after form has been closed by user!
			if (form != null)
			{
				ShowGroupPosts(node, group);
				node.ExpandAll();
			}
		}

		protected async void LoadCommentsForPost(TreeNode node, LinkedInGroupPost post)
		{
			LinkedInGetGroupPostCommentsOptions options = new LinkedInGetGroupPostCommentsOptions();
			options.CommentOptions.SelectAll();
			options.PostId = post.Id;
			ShowLoading(node);

			await Task.Run(() =>
				{
					post.LoadComments(options);
				});

			// Async operation might complete after form has been closed by user!
			if (form != null)
			{
				ShowGroupPostComments(node, post);
				node.ExpandAll();
			}
		}

		protected void ShowMemberGroups(LinkedInResponse<IEnumerable<LinkedInGroup>> result)
		{
			if (result.Result != null && result.Status == LinkedInResponseStatus.OK)
			{
				// Async operation might complete after form has been closed by user!
				if (form != null)
				{
					form.BeginInvoke(() =>
						{
							tvGroups.Nodes.Clear();

							foreach (LinkedInGroup group in result.Result)
							{
								TreeNode node = tvGroups.Nodes.Add(group.Name);
								node.Tag = group;
							}
						});
				}
			}
			else
			{
				ReRun(result.Status, result.Message);
			}
		}

		protected void ShowGroupPosts(TreeNode node, LinkedInGroup group)
		{
			node.Nodes.Clear();

			foreach (LinkedInGroupPost post in group.Posts)
			{
				TreeNode childNode = node.Nodes.Add(post.Title);
				childNode.Tag = post;
			}
		}

		protected void ShowGroupPostComments(TreeNode node, LinkedInGroupPost post)
		{
			node.Nodes.Clear();

			foreach (LinkedInGroupComment comment in post.Comments)
			{
				TreeNode childNode = node.Nodes.Add(comment.Text.LimitLength(64));
				childNode.Tag = comment;
			}
		}

		protected void AddComment(LinkedInGroupPost post, string comment)
		{
			post.Comment(comment);
		}

		protected void ShowLoading(TreeNode node)
		{
			node.Nodes.Clear();
			node.Nodes.Add("Loading...");
			node.ExpandAll();
		}

		protected void ReRun(LinkedInResponseStatus status, string message)
		{
			switch (status)
			{
				case LinkedInResponseStatus.ExpiredToken:
				case LinkedInResponseStatus.InvalidAccessToken:
				case LinkedInResponseStatus.UnauthorizedAction:
					form.BeginInvoke(() => Authenticate());
					break;

				default:
					MessageBox.Show(message);
					break;
			}
		}
    }
}
