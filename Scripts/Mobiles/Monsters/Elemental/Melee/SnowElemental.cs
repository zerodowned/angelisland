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

/* Scripts/Mobiles/Monsters/Elemental/Melee/SnowElemental.cs
 * ChangeLog
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 7 lines removed.
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a snow elemental corpse" )]
	public class SnowElemental : BaseCreature
	{
		[Constructable]
		public SnowElemental() : base( AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a snow elemental";
			Body = 163;
			BaseSoundID = 263;

			SetStr( 326, 355 );
			SetDex( 166, 185 );
			SetInt( 71, 95 );

			SetHits( 196, 213 );

			SetDamage( 11, 17 );



			SetSkill( SkillName.MagicResist, 50.1, 65.0 );
			SetSkill( SkillName.Tactics, 80.1, 100.0 );
			SetSkill( SkillName.Wrestling, 80.1, 100.0 );

			Fame = 5000;
			Karma = -5000;

			VirtualArmor = 50;
		}

		public override int TreasureMapLevel{ get{ return Utility.RandomList( 2, 3 ); } }
		
		public override AuraType MyAura{ get{ return AuraType.Ice; } }

		public SnowElemental( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackItem( new IronOre( 3 ) );
			PackItem( new BlackPearl( 3 ) );
			PackGold( 175, 225 );
			PackMagicEquipment( 1, 5 );
			// Category 3 MID
			PackMagicItem( 1, 2, 0.10 );
			PackMagicItem( 1, 2, 0.05 );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
