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

/* Items/SkillItems/Magical/Misc/PotionKeg.cs
 * CHANGELOG:
 *	9/4/04, mith
 *		OnDragDrop(): Copied Else block from Spellbook, to prevent people dropping things on book to have it bounce back to original location.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class PotionKeg : Item
	{
		private PotionEffect m_Type;
		private int m_Held;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Held
		{
			get
			{
				return m_Held;
			}
			set
			{
				if ( m_Held != value )
				{
					this.Weight += (value - m_Held) * 0.8;

					m_Held = value;
					InvalidateProperties();
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public PotionEffect Type
		{
			get
			{
				return m_Type;
			}
			set
			{
				m_Type = value;
				InvalidateProperties();
			}
		}

		[Constructable]
		public PotionKeg() : base( 0x1940 )
		{
			this.Weight = 1.0;
		}

		public PotionKeg( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( (int) m_Type );
			writer.Write( (int) m_Held );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Type = (PotionEffect)reader.ReadInt();
					m_Held = reader.ReadInt();

					break;
				}
			}
		}

		public override int LabelNumber{ get{ return (m_Held > 0 ? 1041620 + (int)m_Type : 1041641); } }

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			int number;

			if ( m_Held <= 0 )
				number = 502246; // The keg is empty.
			else if ( m_Held < 5 )
				number = 502248; // The keg is nearly empty.
			else if ( m_Held < 20 )
				number = 502249; // The keg is not very full.
			else if ( m_Held < 30 )
				number = 502250; // The keg is about one quarter full.
			else if ( m_Held < 40 )
				number = 502251; // The keg is about one third full.
			else if ( m_Held < 47 )
				number = 502252; // The keg is almost half full.
			else if ( m_Held < 54 )
				number = 502254; // The keg is approximately half full.
			else if ( m_Held < 70 )
				number = 502253; // The keg is more than half full.
			else if ( m_Held < 80 )
				number = 502255; // The keg is about three quarters full.
			else if ( m_Held < 96 )
				number = 502256; // The keg is very full.
			else if ( m_Held < 100 )
				number = 502257; // The liquid is almost to the top of the keg.
			else
				number = 502258; // The keg is completely full.

			list.Add( number );
		}

		public override void OnSingleClick( Mobile from )
		{
			base.OnSingleClick( from );

			int number;

			if ( m_Held <= 0 )
				number = 502246; // The keg is empty.
			else if ( m_Held < 5 )
				number = 502248; // The keg is nearly empty.
			else if ( m_Held < 20 )
				number = 502249; // The keg is not very full.
			else if ( m_Held < 30 )
				number = 502250; // The keg is about one quarter full.
			else if ( m_Held < 40 )
				number = 502251; // The keg is about one third full.
			else if ( m_Held < 47 )
				number = 502252; // The keg is almost half full.
			else if ( m_Held < 54 )
				number = 502254; // The keg is approximately half full.
			else if ( m_Held < 70 )
				number = 502253; // The keg is more than half full.
			else if ( m_Held < 80 )
				number = 502255; // The keg is about three quarters full.
			else if ( m_Held < 96 )
				number = 502256; // The keg is very full.
			else if ( m_Held < 100 )
				number = 502257; // The liquid is almost to the top of the keg.
			else
				number = 502258; // The keg is completely full.

			this.LabelTo( from, number );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.InRange( GetWorldLocation(), 2 ) )
			{
				if ( m_Held > 0 )
				{
					Container pack = from.Backpack;

					if ( pack != null && pack.ConsumeTotal( typeof( Bottle ), 1 ) )
					{
						from.SendLocalizedMessage( 502242 ); // You pour some of the keg's contents into an empty bottle...

						BasePotion pot = FillBottle();

						if ( pack.TryDropItem( from, pot, false ) )
						{
							from.SendLocalizedMessage( 502243 ); // ...and place it into your backpack.
							from.PlaySound( 0x240 );

							if ( --Held == 0 )
								from.SendLocalizedMessage( 502245 ); // The keg is now empty.
						}
						else
						{
							from.SendLocalizedMessage( 502244 ); // ...but there is no room for the bottle in your backpack.
							pot.Delete();
						}
					}
					else
					{
						// TODO: Target a bottle
					}
				}
				else
				{
					from.SendLocalizedMessage( 502246 ); // The keg is empty.
				}
			}
			else
			{
				from.LocalOverheadMessage( Network.MessageType.Regular, 0x3B2, 1019045 ); // I can't reach that.
			}

		}

		public override bool OnDragDrop( Mobile from, Item item )
		{
			if ( item is BasePotion )
			{
				BasePotion pot = (BasePotion)item;

				if ( m_Held == 0 )
				{
					if ( GiveBottle( from ) )
					{
						m_Type = pot.PotionEffect;
						Held = 1;

						from.PlaySound( 0x240 );

						from.SendLocalizedMessage( 502237 ); // You place the empty bottle in your backpack.

						item.Delete();
						return true;
					}
					else
					{
						from.SendLocalizedMessage( 502238 ); // You don't have room for the empty bottle in your backpack.
						return false;
					}
				}
				else if ( pot.PotionEffect != m_Type )
				{
					from.SendLocalizedMessage( 502236 ); // You decide that it would be a bad idea to mix different types of potions.
					return false;
				}
				else if ( m_Held >= 100 )
				{
					from.SendLocalizedMessage( 502233 ); // The keg will not hold any more!
					return false;
				}
				else
				{
					if ( GiveBottle( from ) )
					{
						++Held;
						item.Delete();

						from.PlaySound( 0x240 );

						from.SendLocalizedMessage( 502237 ); // You place the empty bottle in your backpack.

						item.Delete();
						return true;
					}
					else
					{
						from.SendLocalizedMessage( 502238 ); // You don't have room for the empty bottle in your backpack.
						return false;
					}
				}
			}
			else
			{
				from.SendLocalizedMessage( 502232 ); // The keg is not designed to hold that type of object.

				// Adam: anything other than a potion will get dropped into your backpack
				// (so your best sword doesn't get dropped on the ground.)
				from.AddToBackpack( item );
				//	For richness, we add the drop sound of the item dropped.
				from.PlaySound( item.GetDropSound() );
				return true;
			}
		}

		public bool GiveBottle( Mobile m )
		{
			Container pack = m.Backpack;

			Bottle bottle = new Bottle();

			if ( pack == null || !pack.TryDropItem( m, bottle, false ) )
			{
				bottle.Delete();
				return false;
			}

			return true;
		}

		public BasePotion FillBottle()
		{
			switch ( m_Type )
			{
				default:
				case PotionEffect.Nightsight:		return new NightSightPotion();

				case PotionEffect.CureLesser:		return new LesserCurePotion();
				case PotionEffect.Cure:				return new CurePotion();
				case PotionEffect.CureGreater:		return new GreaterCurePotion();

				case PotionEffect.Agility:			return new AgilityPotion();
				case PotionEffect.AgilityGreater:	return new GreaterAgilityPotion();

				case PotionEffect.Strength:			return new StrengthPotion();
				case PotionEffect.StrengthGreater:	return new GreaterStrengthPotion();

				case PotionEffect.PoisonLesser:		return new LesserPoisonPotion();
				case PotionEffect.Poison:			return new PoisonPotion();
				case PotionEffect.PoisonGreater:	return new GreaterPoisonPotion();
				case PotionEffect.PoisonDeadly:		return new DeadlyPoisonPotion();

				case PotionEffect.Refresh:			return new RefreshPotion();
				case PotionEffect.RefreshTotal:		return new TotalRefreshPotion();

				case PotionEffect.HealLesser:		return new LesserHealPotion();
				case PotionEffect.Heal:				return new HealPotion();
				case PotionEffect.HealGreater:		return new GreaterHealPotion();

				case PotionEffect.ExplosionLesser:	return new LesserExplosionPotion();
				case PotionEffect.Explosion:		return new ExplosionPotion();
				case PotionEffect.ExplosionGreater:	return new GreaterExplosionPotion();
			}
		}

		public static void Initialize()
		{
			TileData.ItemTable[0x1940].Height = 4;
		}
	}
}