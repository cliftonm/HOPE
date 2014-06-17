using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace HuntTheWumpus
{
	public class PlayerReceptor : BaseReceptor
    {
		public override string Name { get { return "Player"; } }

		protected Form form;
		protected Button[] moveButtons;
		protected Button[] shootButtons;
		protected Label lblPlayerIsDead;
		protected Label lblPlayerWins;
		protected Random rnd;

		int currentCaveNumber = -1;			// No starting cave.

		public PlayerReceptor(IReceptorSystem rsys)
			: base(rsys)
		{
			AddReceiveProtocol("HW_Player");
			AddReceiveProtocol("HW_GameState");
			AddEmitProtocol("HW_MoveTo");
			AddEmitProtocol("HW_ShootInto");

			rnd = new Random();

			form = new Form();
			form.Text = "Hunt The Wumpus";
			form.Location = new Point(600, 100);
			form.Size = new Size(320, 130);
			form.StartPosition = FormStartPosition.Manual;
			form.TopMost = true;

			moveButtons = new Button[3] { new Button(), new Button(), new Button() };
			shootButtons = new Button[3] { new Button(), new Button(), new Button() };

			moveButtons.ForEachWithIndex((b, idx) =>
				{
					b.Visible = false;
					b.Location = new Point(10 + idx * 100, 15);
					b.Size = new Size(80, 25);
					b.Click += OnMove;
					form.Controls.Add(b);
				});

			shootButtons.ForEachWithIndex((b, idx) =>
			{
				b.Visible = false;
				b.Location = new Point(10 + idx * 100, 50);
				b.Size = new Size(80, 25);
				b.Click += OnShoot;
				form.Controls.Add(b);
			});

			lblPlayerIsDead = new Label();
			lblPlayerIsDead.Text = "You are dead!";
			lblPlayerIsDead.Location = new Point(10, 15);
			lblPlayerIsDead.Size = new Size(300, 15);
			lblPlayerIsDead.TextAlign = ContentAlignment.MiddleCenter;
			lblPlayerIsDead.Visible = false;


			lblPlayerWins = new Label();
			lblPlayerWins.Text = "The Wumpus is dead!";
			lblPlayerWins.Location = new Point(10, 15);
			lblPlayerWins.Size = new Size(300, 15);
			lblPlayerWins.TextAlign = ContentAlignment.MiddleCenter;
			lblPlayerWins.Visible = false;

			form.Controls.Add(lblPlayerIsDead);
			form.Controls.Add(lblPlayerWins);

			form.Show();
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			dynamic signal = carrier.Signal;

			if (carrier.Protocol.DeclTypeName == "HW_Player")
			{
				currentCaveNumber = signal.CaveNumber;
				moveButtons[0].Text = "Move to " + signal.AdjoiningCave1;
				moveButtons[0].Tag = (int)signal.AdjoiningCave1;
				moveButtons[1].Text = "Move to " + signal.AdjoiningCave2;
				moveButtons[1].Tag = (int)signal.AdjoiningCave2;
				moveButtons[2].Text = "Move to " + signal.AdjoiningCave3;
				moveButtons[2].Tag = (int)signal.AdjoiningCave3;
				moveButtons.ForEach(b => b.Visible = true);

				shootButtons[0].Text = "Shoot into " + signal.AdjoiningCave1;
				shootButtons[0].Tag = (int)signal.AdjoiningCave1;
				shootButtons[1].Text = "Shoot into " + signal.AdjoiningCave2;
				shootButtons[1].Tag = (int)signal.AdjoiningCave2;
				shootButtons[2].Text = "Shoot into " + signal.AdjoiningCave3;
				shootButtons[2].Tag = (int)signal.AdjoiningCave3;
				shootButtons.ForEach(b => b.Visible = true);

			}
			else if (carrier.Protocol.DeclTypeName == "HW_GameState")
			{
				if (signal.PlayerEatenByWumpus || signal.PlayerFellIntoPit || signal.PlayerShotSelf)
				{
					moveButtons.ForEach(b => b.Visible = false);
					shootButtons.ForEach(b => b.Visible = false);
					lblPlayerIsDead.Visible = true;
				}
				else if (signal.SuperBatSnatch)
				{
					// Move to some random cave:
					CreateCarrier("HW_MoveTo", (outSignal) => outSignal.NewCaveNumber = rnd.Next(20));
				}
				else if (signal.WumpusIsDead)
				{
					moveButtons.ForEach(b => b.Visible = false);
					shootButtons.ForEach(b => b.Visible = false);
					lblPlayerWins.Visible = true;
				}
			}
		}

		protected void OnMove(object sender, EventArgs e)
		{
			int newCaveNumber = (int)((Button)sender).Tag;
			CreateCarrier("HW_MoveTo", (signal) => 
				{
					signal.FromCaveNumber = currentCaveNumber;
					signal.NewCaveNumber = newCaveNumber;
				});
		}

		protected void OnShoot(object sender, EventArgs e)
		{
			int caveNumber = (int)((Button)sender).Tag;
			CreateCarrier("HW_ShootInto", (signal) =>
				{
					signal.CaveNumber = caveNumber;
					signal.RemainingLife = 5;			// 5 rooms we can traverse with an arrow.
				});
		}
	}
}
