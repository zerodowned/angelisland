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

/* Scripts/Mobiles/Monsters/Humanoid/Magic/GolemController.cs
 * ChangeLog
 *	04/27/09, plasma
 *		Reduced arcane gem drop rate to 10% from 70%
 *	1/1/09, Adam
 *		- Add potions and bandages
 *			Now uses real potions and real bandages
 *		- Cross heals is now turned on
 *		- Smart AI upgrade (adds healing with bandages)
 *	12/5/08, Adam
 *		black shirt&kilt now drop only from the spawner in Wrong
 *		(spawner lootPack)
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *  08/13/06, Kit
 *		Adjust active speed back up 
 *  07/02/06, Kit
 *		InitBody/InitOutfit additions
 *  12/10/05, Kit
 *		Increased Active speed to more closely match player running speed of 0.2.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *	6/5/05, Adam
 *		decrease to a 3% chance at a black kilt 
 *		decrease to a 3% at a black shirt
 *	6/4/05, Adam
 *		Add a 10% chance at a black kilt 
 *		Add a 10% chance at a black shirt
 *  5/30/05, Kit
 *		Changed active speed to allow for more AI time, allowing to be closer to human reaction time
 *		Added pouch to backpack, made bardimmue.
 *	5/15/05, Kit
 *		Changed to use New EvilMageAI and be 7x mages with human stats
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System; 
using Server;
using Server.Items;

namespace Server.Mobiles 
{ 
	[CorpseName( "a golem controller corpse" )] 
	public class GolemController : BaseCreature 
	{ 
		[Constructable] 
		public GolemController() : base( AIType.AI_BaseHybrid, FightMode.All | FightMode.Closest, 16, 1, 0.1, 0.25 ) 
		{
			BardImmune = true;
			FightStyle = FightStyle.Magic | FightStyle.Smart | FightStyle.Bless | FightStyle.Curse;
			UsesHumanWeapons = false;
			UsesBandages = true;
			UsesPotions = true;
			CanRun = true;
			CanReveal = true;	// magic and smart
			CrossHeals = true;	// classic Angel Island Golem Controllers

			Title = "the controller";

			Hue = 0x455;

			SetStr( 90 );
			SetDex( 35);
			SetInt( 100);
			
			SetDamage( 6, 12 );

			SetSkill( SkillName.EvalInt, 100.0 );
			SetSkill( SkillName.Magery, 100.0 );
			SetSkill( SkillName.Meditation, 100.0 );
			SetSkill( SkillName.MagicResist, 100.0 );
			SetSkill( SkillName.Inscribe, 100 );
			SetSkill( SkillName.Poisoning, 100 );
			SetSkill( SkillName.Wrestling, 100 );

			//overrided speeds since speedinfo changes things.
			Fame = 4000;
			Karma = -4000;

			InitBody();
			InitOutfit();

			VirtualArmor = 21;

			PackItem(new Bandage(Utility.RandomMinMax(VirtualArmor, VirtualArmor * 2)));
			PackStrongPotions(6, 12);
			PackItem(new Pouch());
		}
		
		public void AddArcane( Item item )
		{
			if ( item is IArcaneEquip )
			{
				IArcaneEquip eq = (IArcaneEquip)item;
				eq.CurArcaneCharges = eq.MaxArcaneCharges = 20;
			}

			item.Hue = ArcaneGem.DefaultArcaneHue;
			item.LootType = LootType.Newbied;

			AddItem( item );
		}

		public override bool ClickTitle{ get{ return false; } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool AlwaysMurderer{ get{ return true; } }

		public override void InitBody()
		{
			Name = NameList.RandomName( "golem controller" );
		
			if ( Female = Utility.RandomBool() )
				Body = 401;
			else
				Body = 400;
		}
		public override void InitOutfit()
		{
			WipeLayers();
			AddArcane( new Robe() );
			AddArcane( new ThighBoots() );
			AddArcane( new LeatherGloves() );
			AddArcane( new Cloak() );

			// black kilt now drops from the spawner in Wrong only
			Kilt kilt = new Kilt( 0x1 ); 
			kilt.LootType = LootType.Newbied;
			AddItem( kilt );

			// black shirt now drops from the spawner in Wrong only
			Shirt shirt = new Shirt( 0x1 ); 
			shirt.LootType = LootType.Newbied;
			AddItem( shirt );
			
		}

		/// <summary>
		/// plasma: GC uses the new abilty to sort target priority.
		///		This is so we can enable the use of ConstantFocus, which is
		///		used when GCs are spawned from the PowerVortex.
		/// </summary>
		public override FightMode[] FightModePriority
		{
			get
			{
				return new FightMode[] { FightMode.ConstantFocus, FightMode.All};
			}
		}

		public GolemController( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void GenerateLoot()
		{
			PackGold( 125, 175 );
			//drop 0-8 empty bottles on death
			for ( int i = 0; i < Utility.Random(8); ++i )
			{
				PackItem( new Bottle() );
			} 
			
			if ( 0.1 > Utility.RandomDouble() )
				PackItem( new ArcaneGem() );

			// Category 2 MID
			PackMagicItem( 1, 1, 0.05 );
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
