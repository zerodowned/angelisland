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

/* Scripts/Mobiles/Monsters/Misc/Magic/FrostNymph.cs
 *	ChangeLog:
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 7 lines removed.
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 *	5/20/04 Created by smerX
 *
 */
 
using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a pool of Nymph" )]
	public class FrostNymph : BaseCreature
	{
		[Constructable]
		public FrostNymph() : base( AIType.AI_Mage, FightMode.All | FightMode.Closest, 10, 1, 0.15, 0.35 )
		{
			Name = "a frost nymph";
			Body = 176;
			Hue = 0x4f2;
			BaseSoundID = 0x4B0;

			SetStr( 171, 200 );
			SetDex( 126, 145 );
			SetInt( 276, 305 );

			SetHits( 103, 120 );

			SetDamage( 24, 26 );



			SetSkill( SkillName.EvalInt, 100.0 );
			SetSkill( SkillName.Magery, 70.1, 80.0 );
			SetSkill( SkillName.Meditation, 85.1, 95.0 );
			SetSkill( SkillName.MagicResist, 80.1, 100.0 );
			SetSkill( SkillName.Tactics, 70.1, 90.0 );

			Fame = 8000;
			Karma = -8000;

			VirtualArmor = 50;
		}

		public override bool CanRummageCorpses{ get{ return true; } }

		public FrostNymph( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			Body = 51;
			PackGem();
			PackReg( 8, 12 );
			
			PackGold( 170, 220 );
			PackScroll( 2, 7 );
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
	} }
