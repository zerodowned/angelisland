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

/* /Scripts/Multis/Boats/BaseBoatDeed.cs
 * ChangeLog
 *	6/10/04, mith
 *		Removed newbieness of boat deeds
 */

using System;
using Server;
using Server.Regions;
using Server.Targeting;

namespace Server.Multis
{
	public abstract class BaseBoatDeed : Item
	{
		private int m_MultiID;
		private Point3D m_Offset;

		[CommandProperty( AccessLevel.GameMaster )]
		public int MultiID{ get{ return m_MultiID; } set{ m_MultiID = value; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public Point3D Offset{ get{ return m_Offset; } set{ m_Offset = value; } }

		public BaseBoatDeed( int id, Point3D offset ) : base( 0x14F2 )
		{
			Weight = 1.0;

//			if ( !Core.AOS )
//				LootType = LootType.Newbied;

			m_MultiID = id & 0x3FFF;
			m_Offset = offset;
		}

		public BaseBoatDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_MultiID );
			writer.Write( m_Offset );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_MultiID = reader.ReadInt();
					m_Offset = reader.ReadPoint3D();

					break;
				}
			}

			if ( Weight == 0.0 )
				Weight = 1.0;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else
			{
				from.SendLocalizedMessage( 502482 ); // Where do you wish to place the ship?

				from.Target = new InternalTarget( this );
			}
		}

		public abstract BaseBoat Boat{ get; }

		public void OnPlacement( Mobile from, Point3D p )
		{
			if ( Deleted )
			{
				return;
			}
			else if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else
			{
				Map map = from.Map;

				if ( map == null )
					return;

				if ( from.AccessLevel < AccessLevel.GameMaster && (map == Map.Ilshenar || map == Map.Malas) )
				{
					from.SendLocalizedMessage( 1043284 ); // A ship can not be created here.
					return;
				}

				BaseBoat boat = Boat;

				if ( boat == null )
					return;

				p = new Point3D( p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z );

				if ( BaseBoat.IsValidLocation( p, map ) && boat.CanFit( p, map, boat.ItemID ) )
				{
					Delete();

					boat.Owner = from;
					boat.Anchored = true;

					uint keyValue = boat.CreateKeys( from );

					if ( boat.PPlank != null )
						boat.PPlank.KeyValue = keyValue;

					if ( boat.SPlank != null )
						boat.SPlank.KeyValue = keyValue;

					boat.MoveToWorld( p, map );
				}
				else
				{
					boat.Delete();
					from.SendLocalizedMessage( 1043284 ); // A ship can not be created here.
				}
			}
		}

		private class InternalTarget : MultiTarget
		{
			private BaseBoatDeed m_Deed;

			public InternalTarget( BaseBoatDeed deed ) : base( deed.MultiID, deed.Offset )
			{
				m_Deed = deed;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				IPoint3D ip = o as IPoint3D;

				if ( ip != null )
				{
					if ( ip is Item )
						ip = ((Item)ip).GetWorldTop();

					Point3D p = new Point3D( ip );

					Region region = Region.Find( p, from.Map );

					//if ( region is FeluccaDungeon || region is TrammelDungeon )
					if ( region is FeluccaDungeon )
						from.SendLocalizedMessage( 502488 ); // You can not place a ship inside a dungeon.
					else if ( region is HouseRegion )
						from.SendLocalizedMessage( 1042549 ); // A boat may not be placed in this area.
					else
						m_Deed.OnPlacement( from, p );
				}
			}
		}
	}
}