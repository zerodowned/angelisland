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

/* Scripts/Items/Weapons/Knives/BasilisksFang.cs	
 * ChangeLog:
 *  11/9/08, Adam
 *      Replace old MaxHits and Hits with MaxHitPoints and HitPoints (RunUO 2.0 compatibility)
 *	1/15/05, Adam
 *		First version based on GuardiansKatana.cs
 */

using System;
using Server.Network;
using System.Collections;
using Server.Targeting;
using Server.Items;

namespace Server.Items
{
	[FlipableAttribute( 0xF52, 0xF51 )]
	public class BasilisksFang : BaseKnife
	{
		public override WeaponAbility PrimaryAbility{ get{ return WeaponAbility.InfectiousStrike; } }
		public override WeaponAbility SecondaryAbility{ get{ return WeaponAbility.ShadowStrike; } }

//		public override int AosStrengthReq{ get{ return 10; } }
//		public override int AosMinDamage{ get{ return 10; } }
//		public override int AosMaxDamage{ get{ return 11; } }
//		public override int AosSpeed{ get{ return 56; } }
//
//		public override int OldMinDamage{ get{ return 3; } }
//		public override int OldMaxDamage{ get{ return 15; } }
		public override int OldStrengthReq{ get{ return 1; } }
		public override int OldSpeed{ get{ return 55; } }
																		    
		public override int OldDieRolls{ get{ return 3; } }
		public override int OldDieMax{ get{ return 5; } }
		public override int OldAddConstant{ get{ return 0; } }

		public override int InitMinHits{ get{ return 31; } }
		public override int InitMaxHits{ get{ return 40; } }

		public override SkillName DefSkill{ get{ return SkillName.Fencing; } }
		public override WeaponType DefType{ get{ return WeaponType.Piercing; } }
		public override WeaponAnimation DefAnimation{ get{ return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public BasilisksFang() : base( 0xF52 )
		{
			Weight = 1.0;
			Name = "fang of the basilisk";
			Hue = 2006; // basilisk hue
		}

		public BasilisksFang( Serial serial ) : base( serial )
		{
		}

		public override void OnHit(Mobile attacker, Mobile defender)
		{
			base.OnHit(attacker, defender);

			if (!Core.AOS && Poison != null && PoisonCharges > 0)
			{
				if (Utility.RandomDouble() >= 0.5) // 50% chance to poison
					defender.ApplyPoison(attacker, Poison);
			}

			this.PoisonCharges = 30;
			this.HitPoints = this.MaxHitPoints;
		}

		// Special version that DOES NOT show attributes and tags
		public override void OnSingleClick(Mobile from)
		{
			ArrayList attrs = new ArrayList();

			int number;

			if (Name == null)
			{
				number = LabelNumber;
			}
			else
			{
				this.LabelTo(from, Name);
				number = 1041000;
			}

			if (attrs.Count == 0 && Crafter == null && Name != null)
				return;

			EquipmentInfo eqInfo = new EquipmentInfo(number, Crafter, false, (EquipInfoAttribute[])attrs.ToArray(typeof(EquipInfoAttribute)));

			from.Send(new DisplayEquipmentInfo(this, eqInfo));

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