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
using System.Text;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
	public class HairDye : Item
	{
		public override int LabelNumber{ get{ return 1041060; } } // Hair Dye

		[Constructable]
		public HairDye() : base( 0xEFF )
		{
			Weight = 1.0;
		}

		public HairDye( Serial serial ) : base( serial )
		{
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.InRange( this.GetWorldLocation(), 1 ) )
			{
				from.CloseGump( typeof( HairDyeGump ) );
				from.SendGump( new HairDyeGump( this ) );
			}
			else
			{
				from.LocalOverheadMessage( MessageType.Regular, 906, 1019045 ); // I can't reach that.
			}	
		}
	}

	public class HairDyeGump : Gump
	{
		private HairDye m_HairDye;

		private class HairDyeEntry
		{
			private string m_Name;
			private int m_HueStart;
			private int m_HueCount;

			public string Name
			{
				get
				{
					return m_Name;
				}
			}

			public int HueStart
			{
				get
				{
					return m_HueStart;
				}
			}

			public int HueCount
			{
				get
				{
					return m_HueCount;
				}
			}

			public HairDyeEntry( string name, int hueStart, int hueCount )
			{
				m_Name = name;
				m_HueStart = hueStart;
				m_HueCount = hueCount;
			}
		}

		private static HairDyeEntry[] m_Entries = new HairDyeEntry[]
			{
				new HairDyeEntry( "*****", 1602, 26 ),
				new HairDyeEntry( "*****", 1628, 27 ),
				new HairDyeEntry( "*****", 1502, 32 ),
				new HairDyeEntry( "*****", 1302, 32 ),
				new HairDyeEntry( "*****", 1402, 32 ),
				new HairDyeEntry( "*****", 1202, 24 ),
				new HairDyeEntry( "*****", 2402, 29 ),
				new HairDyeEntry( "*****", 2213, 6 ),
				new HairDyeEntry( "*****", 1102, 8 ),
				new HairDyeEntry( "*****", 1110, 8 ),
				new HairDyeEntry( "*****", 1118, 16 ),
				new HairDyeEntry( "*****", 1134, 16 )
			};

		public HairDyeGump( HairDye dye ) : base( 50, 50 )
		{
			m_HairDye = dye;

			AddPage( 0 );

			AddBackground( 100, 10, 350, 355, 2600 );
			AddBackground( 120, 54, 110, 270, 5100 );

			AddHtmlLocalized( 70, 25, 400, 35, 1011013, false, false ); // <center>Hair Color Selection Menu</center>

			AddButton( 149, 328, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 185, 329, 250, 35, 1011014, false, false ); // Dye my hair this color!

			for ( int i = 0; i < m_Entries.Length; ++i )
			{
				AddLabel( 130, 59 + (i * 22), m_Entries[i].HueStart - 1, m_Entries[i].Name );
				AddButton( 207, 60 + (i * 22), 5224, 5224, 0, GumpButtonType.Page, i + 1 );
			}

			for ( int i = 0; i < m_Entries.Length; ++i )
			{
				HairDyeEntry e = m_Entries[i];

				AddPage( i + 1 );

				for ( int j = 0; j < e.HueCount; ++j )
				{
					AddLabel( 278 + ((j / 16) * 80), 52 + ((j % 16) * 17), e.HueStart + j - 1, "*****" );
					AddRadio( 260 + ((j / 16) * 80), 52 + ((j % 16) * 17), 210, 211, false, (i * 100) + j );
				}
			}
		}

		public override void OnResponse( NetState from, RelayInfo info )
		{
			if ( m_HairDye.Deleted )
				return;

			Mobile m = from.Mobile;
			int[] switches = info.Switches;

			if ( !m_HairDye.IsChildOf( m.Backpack ) ) 
			{
				m.SendLocalizedMessage( 1042010 ); //You must have the objectin your backpack to use it.
				return;
			}

			if ( info.ButtonID != 0 && switches.Length > 0 )
			{
				Item hair = m.Hair;
				Item beard = m.Beard;

				if ( hair == null && beard == null )
				{
					m.SendLocalizedMessage( 502623 );	// You have no hair to dye and cannot use this
				}
				else
				{
					// To prevent this from being exploited, the hue is abstracted into an internal list

					int entryIndex = switches[0] / 100;
					int hueOffset = switches[0] % 100;

					if ( entryIndex >= 0 && entryIndex < m_Entries.Length )
					{
						HairDyeEntry e = m_Entries[entryIndex];

						if ( hueOffset >= 0 && hueOffset < e.HueCount )
						{
							int hue = e.HueStart + hueOffset;

							if ( hair != null )
								hair.Hue = hue;

							if ( beard != null )
								beard.Hue = hue;

							m.SendLocalizedMessage( 501199 );  // You dye your hair
							m_HairDye.Delete();
							m.PlaySound( 0x4E );
						}
					}
				}
			}
			else
			{
				m.SendLocalizedMessage( 501200 ); // You decide not to dye your hair
			}
		}
	}
}�