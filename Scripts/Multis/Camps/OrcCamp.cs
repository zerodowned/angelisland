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
using Server.Items;
using Server.Mobiles;

namespace Server.Multis
{
	public class OrcCamp : BaseCamp
	{
		private Mobile m_Prisoner;
		private BaseDoor m_Gate;

		[Constructable]
		public OrcCamp() : base( 0x1D4C )
		{
		}

		public override void AddComponents()
		{
			IronGate gate = new IronGate( DoorFacing.EastCCW );
			m_Gate = gate;

			gate.KeyValue = Key.RandomValue();
			gate.Locked = true;

			AddItem( gate, -2, 1, 0 );

			MetalChest chest = new MetalChest();

			chest.ItemID = 0xE7C;
			chest.DropItem( new Key( KeyType.Iron, gate.KeyValue ) );

			TreasureMapChest.Fill( chest, 2 );

			AddItem( chest, 4, 4, 0 );

			AddMobile( new Orc(), 15, 0, -2, 0 );
			AddMobile( new Orc(), 15, 0,  1, 0 );
			AddMobile( new OrcishMage(), 15, 0, -1, 0 );
			AddMobile( new OrcCaptain(), 15, 0,  0, 0 );

			switch ( Utility.Random( 2 ) )
			{
				case 0: m_Prisoner = new Noble(); break;
				case 1: m_Prisoner = new SeekerOfAdventure(); break;
			}

			m_Prisoner.YellHue = Utility.RandomList( 0x57, 0x67, 0x77, 0x87, 0x117 );

			AddMobile( m_Prisoner, 2, -2, 0, 0 );
		}

		public override void OnEnter( Mobile m )
		{
			base.OnEnter( m );

			if ( m.Player && m_Prisoner != null && m_Gate != null && m_Gate.Locked )
			{
				int number;

				switch ( Utility.Random( 4 ) )
				{
					default:
					case 0: number =  502264; break; // Help a poor prisoner!
					case 1: number =  502266; break; // Aaah! Help me!
					case 2: number = 1046000; break; // Help! These savages wish to end my life!
					case 3: number = 1046003; break; // Quickly! Kill them for me! HELP!!
				}

				m_Prisoner.Yell( number );
			}
		}

		public OrcCamp( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_Prisoner );
			writer.Write( m_Gate );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Prisoner = reader.ReadMobile();
					m_Gate = reader.ReadItem() as BaseDoor;
					break;
				}
			}
		}
	}
} 
