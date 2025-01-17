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

/* /Scripts/Engines/Reward System/RewardConfirmGump.cs
 * Created 5/23/04 by mith
 * ChangeLog
 */

using System;
using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Engines.RewardSystem
{
	public class RewardConfirmGump : Gump
	{
		private Mobile m_From;
		private RewardEntry m_Entry;

		public RewardConfirmGump( Mobile from, RewardEntry entry ) : base( 0, 0 )
		{
			m_From = from;
			m_Entry = entry;

			from.CloseGump( typeof( RewardConfirmGump ) );

			AddPage( 0 );

			AddBackground( 10, 10, 500, 300, 2600 );

			AddHtmlLocalized( 30, 55, 300, 35, 1006000, false, false ); // You have selected:

			if ( entry.NameString != null )
				AddHtml( 335, 55, 150, 35, entry.NameString, false, false );
			else
				AddHtmlLocalized( 335, 55, 150, 35, entry.Name, false, false );

			AddHtmlLocalized( 30, 95, 300, 35, 1006001, false, false ); // This will be assigned to this character:
			AddLabel( 335, 95, 0, from.Name );

			AddHtmlLocalized( 35, 160, 450, 90, 1006002, true, true ); // Are you sure you wish to select this reward for this character?  You will not be able to transfer this reward to another character on another shard.  Click 'ok' below to confirm your selection or 'cancel' to go back to the selection screen.

			AddButton( 60, 265, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 95, 266, 150, 35, 1006044, false, false ); // Ok

			AddButton( 295, 265, 4017, 4019, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 330, 266, 150, 35, 1006045, false, false ); // Cancel
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( info.ButtonID == 1 )
			{
				Item item = m_Entry.Construct();

				if ( item != null )
				{
					item.Weight = 10.0;
					item.Name = "A statue honoring " + m_From.Name + ", Angel Island Pioneer";
					if ( RewardSystem.UpdateRewardCodes( m_From ) )
						m_From.AddToBackpack( item );
					else
						item.Delete();
				}
			}
		}
	}
}