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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace HelloWorldReceptor
{
    public class ReceptorDefinition : BaseReceptor
    {
		public override string Name { get { return "Hello World"; } }

		protected Timer timer;

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			InitializeRepeatedHelloEvent();
			AddEmitProtocol("LoggerMessage");
		}

		public override void Terminate()
		{
			timer.Stop();
			timer.Dispose();
		}

		protected void InitializeRepeatedHelloEvent()
		{
			timer = new Timer();
			timer.Interval = 1000 * 2;		// every 2 seconds.
			timer.Tick += SayHello;
			timer.Start();
		}

		protected void SayHello(object sender, EventArgs args)
		{
			if (Enabled)
			{
				CreateCarrier("LoggerMessage", signal =>
					{
						// TODO: We need "calculated" fields, ones that automatically populate with a constant (much like computed fields, which perform a computation.)
						signal.MessageTime = DateTime.Now;
						signal.TextMessage.Text.Value = "Hello world!";
					});
			}
		}
    }
}
