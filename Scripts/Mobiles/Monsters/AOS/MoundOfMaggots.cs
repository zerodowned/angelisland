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

/* Scripts/Mobiles/Monsters/AOS/MoundOfMaggots.cs
 * ChangeLog
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 4 lines removed.
 *  9/21/04, Jade
 *      Increased gold drop from (50, 250) to (150, 300)
 */

using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a maggoty corpse" )] // TODO: Corpse name?
	public class MoundOfMaggots : BaseCreature
	{
		[Constructable]
		public MoundOfMaggots() : base( AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.3, 0.6 )
		{
			Name = "a mound of maggots";
			Body = 319;
			BaseSoundID = 898;

			SetStr( 61, 70 );
			SetDex( 61, 70 );
			SetInt( 10 );

			SetMana( 0 );

			SetDamage( 3, 9 );



			SetSkill( SkillName.Tactics, 50.0 );
			SetSkill( SkillName.Wrestling, 50.1, 60.0 );

			Fame = 1000;
			Karma = -1000;

			VirtualArmor = 24;
		}

		public override Poison PoisonImmune{ get{ return Poison.Lethal; } }

		public override int TreasureMapLevel{ get{ return 1; } }

		public MoundOfMaggots( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackGem();
			PackGold( 150, 300 );
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
