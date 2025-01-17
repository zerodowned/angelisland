/***************************************************************************
 *                               CREDITS
 *                         -------------------
 *                         : (C) 2004-2009 Luke Tomasello (AKA Adam Ant)
 *                         :   and the Angel Island Software Team
 *                         :   luke@tomasello.com
 *                         :   Official Documentation:
 *                         :   www.game-master.net, wiki.game-master.net
 *                         :   Official Source Code (SVN Repository):
 *                         :   http://game-master.net:8050/svn/angelisland
 *                         : 
 *                         : (C) May 1, 2002 The RunUO Software Team
 *                         :   info@runuo.com
 *
 *   Give credit where credit is due!
 *   Even though this is 'free software', you are encouraged to give
 *    credit to the individuals that spent many hundreds of hours
 *    developing this software.
 *   Many of the ideas you will find in this Angel Island version of 
 *   Ultima Online are unique and one-of-a-kind in the gaming industry! 
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

/* Scripts/Commands/Report.cs
 * Changelog
 *	11/21/07, Adam
 *		don't announce to staff if staff is being reported!
 *	01/09/06 Taran Kain
 *		Initial version
 */

using System;
using System.Collections;
using Server;
using Server.Targeting;
using Server.Mobiles;
using Server.Gumps;

namespace Scripts.Commands
{
	/// <summary>
	/// Summary description for Report.
	/// </summary>
	public class Report
	{
		private static Hashtable m_Reporters = new Hashtable();

		public static void Initialize()
		{
			Server.Commands.Register("Report", AccessLevel.Player, new CommandEventHandler(Report_OnCommand));
			Timer.DelayCall(TimeSpan.FromHours(1.0), TimeSpan.FromHours(1.0), new TimerCallback(CleanReporters));
		}

		public static void Report_OnCommand(CommandEventArgs e)
		{
			if (m_Reporters[e.Mobile] != null && ((DateTime)m_Reporters[e.Mobile]) > DateTime.Now - TimeSpan.FromHours(4.0) && e.Mobile.AccessLevel == AccessLevel.Player)
				e.Mobile.SendMessage("You have already reported someone. You must wait to do it again.");
			else
			{
				e.Mobile.SendMessage("Target the player to report as abusive.");
				e.Mobile.Target = new ReportTarget();
			}
		}

		private static void CleanReporters()
		{
			ArrayList reporters = new ArrayList(m_Reporters.Keys);
			foreach (Mobile m in reporters)
			{
				if ((DateTime)m_Reporters[m] < DateTime.Now - TimeSpan.FromHours(4))
					m_Reporters.Remove(m);
			}
		}

		private class ReportTarget : Target
		{
			public ReportTarget()
				: base(11, false, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (!(targeted is PlayerMobile))
				{
					from.SendMessage("You can only report players as abusive.");
					return;
				}

				from.SendGump(new ReportGump(targeted as PlayerMobile));
			}
		}

		private class ReportGump : Gump
		{
			private PlayerMobile m_Player;

			public ReportGump(PlayerMobile player)
				: base(400, 350)
			{
				m_Player = player;

				AddPage(0);

				AddBackground(0, 0, 400, 350, 2600);

				AddHtml(0, 20, 400, 35, "<center>Report Abusive Player</center>", false, false);

				AddHtml(50, 55, 300, 140, "You are about to report " + player.Name + " to the Angel Island Staff. This is a serious charge, and is not in place for simple trash talking. Use this command to report racism, sexually offensive language and other such activities. All reports will be reviewed by an Administrator. Do you want to continue?", true, false);

				AddButton(200, 227, 4005, 4007, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(235, 230, 110, 35, 1011012, false, false); // CANCEL

				AddButton(65, 227, 4005, 4007, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(100, 230, 110, 35, 1011011, false, false); // CONTINUE				
			}

			public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
			{
				Mobile from = sender.Mobile;

				from.CloseGump(typeof(ReportGump));

				if (info.ButtonID == 1) // continue
				{
					m_Player.Report(from);
					Report.m_Reporters[from] = DateTime.Now;
					from.SendMessage("You have reported " + m_Player.Name + " to staff.");

					// don't announce to staff if staff is being reported!
					if (m_Player.AccessLevel <= AccessLevel.Player)
						foreach (Server.Network.NetState state in Server.Network.NetState.Instances)
						{
							Mobile m = state.Mobile;

							if (m != null && m.AccessLevel > AccessLevel.Player)
								m.SendMessage(from.Name + " has reported " + m_Player.Name + " as abusive.");
						}
				}
				else
					from.SendMessage("You choose not to report them.");
			}
		}
	}
}
