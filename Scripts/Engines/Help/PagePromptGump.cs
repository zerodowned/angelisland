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

using System;
using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Engines.Help
{
	public class PagePromptGump : Gump
	{
		private Mobile m_From;
		private PageType m_Type;

		public PagePromptGump( Mobile from, PageType type ) : base( 0, 0 )
		{
			m_From = from;
			m_Type = type;

			from.CloseGump( typeof( PagePromptGump ) );

			AddBackground( 50, 50, 540, 350, 2600 );

			AddPage( 0 );

			AddHtmlLocalized( 264, 80, 200, 24, 1062524, false, false ); // Enter Description
			AddHtmlLocalized( 120, 108, 420, 48, 1062638, false, false ); // Please enter a brief description (up to 200 characters) of your problem:

			AddBackground( 100, 148, 440, 200, 3500 );
			AddTextEntry( 120, 168, 400, 200, 1153, 0, "" );

			AddButton( 175, 355, 2074, 2075, 1, GumpButtonType.Reply, 0 ); // Okay
			AddButton( 405, 355, 2073, 2072, 0, GumpButtonType.Reply, 0 ); // Cancel
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( info.ButtonID == 0 )
			{
				m_From.SendLocalizedMessage( 501235, "", 0x35 ); // Help request aborted.
			}
			else
			{
				TextRelay entry = info.GetTextEntry( 0 );
				string text = ( entry == null ? "" : entry.Text.Trim() );

				if ( text.Length == 0 )
				{
					m_From.SendMessage( 0x35, "You must enter a description." );
					m_From.SendGump( new PagePromptGump( m_From, m_Type ) );
				}
				else
				{
					m_From.SendLocalizedMessage( 501234, "", 0x35 ); /* The next available Counselor/Game Master will respond as soon as possible.
																	  * Please check your Journal for messages every few minutes.
																	  */

					PageQueue.Enqueue( new PageEntry( m_From, text, m_Type ) );
				}
			}
		}
	}
}
 
