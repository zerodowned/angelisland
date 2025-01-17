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

/* Scripts\Engines\Quests\Witch Apprentice\Mobiles\Grizelda.cs
 * ChangeLog:
 *  3/18/08, Adam
 *		Remove references to virtue code
 */

using System;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Engines.Quests.Hag
{
	public class Grizelda : BaseQuester
	{
		public override bool ClickTitle{ get{ return true; } }

		[Constructable]
		public Grizelda() : base( "the Hag" )
		{
		}

		public Grizelda( Serial serial ) : base( serial )
		{
		}

		public override void InitBody()
		{
			InitStats( 100, 100, 25 );

			Hue = 0x83EA;

			Female = true;
			Body = 0x191;
			Name = "Grizelda";
		}

		public override void InitOutfit()
		{
			AddItem( new Robe( 0x1 ) );
			AddItem( new Sandals() );
			AddItem( new WizardsHat( 0x1 ) );
			AddItem( new GoldBracelet() );

			AddItem( new LongHair( 0x0 ) );

			Item staff = new GnarledStaff();
			staff.Movable = false;
			AddItem( staff );
		}

		public override void OnTalk( PlayerMobile player, bool contextMenu )
		{
			Direction = GetDirectionTo( player );

			QuestSystem qs = player.Quest;

			if ( qs is WitchApprenticeQuest )
			{
				if ( qs.IsObjectiveInProgress( typeof( FindApprenticeObjective ) ) )
				{
					PlaySound( 0x259 );
					PlaySound( 0x206 );
					qs.AddConversation( new HagDuringCorpseSearchConversation() );
				}
				else
				{
					QuestObjective obj = qs.FindObjective( typeof( FindGrizeldaAboutMurderObjective ) );

					if ( obj != null && !obj.Completed )
					{
						PlaySound( 0x420 );
						PlaySound( 0x20 );
						obj.Complete();
					}
					else if ( qs.IsObjectiveInProgress( typeof( KillImpsObjective ) )
						|| qs.IsObjectiveInProgress( typeof( FindZeefzorpulObjective ) ) )
					{
						PlaySound( 0x259 );
						PlaySound( 0x206 );
						qs.AddConversation( new HagDuringImpSearchConversation() );
					}
					else
					{
						obj = qs.FindObjective( typeof( ReturnRecipeObjective ) );

						if ( obj != null && !obj.Completed )
						{
							PlaySound( 0x258 );
							PlaySound( 0x41B );
							obj.Complete();
						}
						else if ( qs.IsObjectiveInProgress( typeof( FindIngredientObjective ) ) )
						{
							PlaySound( 0x259 );
							PlaySound( 0x206 );
							qs.AddConversation( new HagDuringIngredientsConversation() );
						}
						else
						{
							obj = qs.FindObjective( typeof( ReturnIngredientsObjective ) );

							if ( obj != null && !obj.Completed )
							{
								Container cont = GetNewContainer();

								cont.DropItem( new BlackPearl( 30 ) );
								cont.DropItem( new Bloodmoss( 30 ) );
								cont.DropItem( new Garlic( 30 ) );
								cont.DropItem( new Ginseng( 30 ) );
								cont.DropItem( new MandrakeRoot( 30 ) );
								cont.DropItem( new Nightshade( 30 ) );
								cont.DropItem( new SulfurousAsh( 30 ) );
								cont.DropItem( new SpidersSilk( 30 ) );

								cont.DropItem( new Cauldron() );
								cont.DropItem( new MoonfireBrew() );
								cont.DropItem( new TreasureMap( Utility.RandomMinMax( 1, 4 ), this.Map ) );

								if ( Utility.RandomBool() )
								{
									BaseWeapon weapon = Loot.RandomWeapon();

									if ( Core.AOS )
									{
										BaseRunicTool.ApplyAttributesTo( weapon, 2, 20, 30 );
									}
									else
									{
										weapon.DamageLevel = (WeaponDamageLevel)BaseCreature.RandomMinMaxScaled( 20, 30 );
										weapon.AccuracyLevel = (WeaponAccuracyLevel)BaseCreature.RandomMinMaxScaled( 20, 30 );
										weapon.DurabilityLevel = (WeaponDurabilityLevel)BaseCreature.RandomMinMaxScaled( 20, 30 );
									}

									cont.DropItem( weapon );
								}
								else
								{
									Item item;
							
									if ( Core.AOS )
									{
										item = Loot.RandomArmorOrShieldOrJewelry();

										if ( item is BaseArmor )
											BaseRunicTool.ApplyAttributesTo( (BaseArmor)item, 2, 20, 30 );
										else if ( item is BaseJewel )
											BaseRunicTool.ApplyAttributesTo( (BaseJewel)item, 2, 20, 30 );
									}
									else
									{
										BaseArmor armor = Loot.RandomArmorOrShield();
										item = armor;

										armor.ProtectionLevel = (ArmorProtectionLevel)BaseCreature.RandomMinMaxScaled( 20, 30 );
										armor.Durability = (ArmorDurabilityLevel)BaseCreature.RandomMinMaxScaled( 20, 30 );
									}

									cont.DropItem( item );
								}

								if ( player.BAC > 0 )
									cont.DropItem( new HangoverCure() );

								if ( player.PlaceInBackpack( cont ) )
								{
									// adam: 3/18/08 - virtues are obsolete
									//bool gainedPath = false;
									//if ( VirtueHelper.Award( player, VirtueName.Sacrifice, 1, ref gainedPath ) )
										//player.SendLocalizedMessage( 1054160 ); // You have gained in sacrifice.

									PlaySound( 0x253 );
									PlaySound( 0x20 );
									obj.Complete();
								}
								else
								{
									cont.Delete();
									player.SendLocalizedMessage( 1046260 ); // You need to clear some space in your inventory to continue with the quest.  Come back here when you have more space in your inventory.
								}
							}
						}
					}
				}
			}
			else
			{
				QuestSystem newQuest = new WitchApprenticeQuest( player );
				bool inRestartPeriod = false;

				if ( qs != null )
				{
					newQuest.AddConversation( new DontOfferConversation() );
				}
				else if ( QuestSystem.CanOfferQuest( player, typeof( WitchApprenticeQuest ), out inRestartPeriod ) )
				{
					PlaySound( 0x20 );
					PlaySound( 0x206 );
					newQuest.SendOffer();
				}
				else if ( inRestartPeriod )
				{
					PlaySound( 0x259 );
					PlaySound( 0x206 );
					newQuest.AddConversation( new RecentlyFinishedConversation() );
				}
			}
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
	}
}