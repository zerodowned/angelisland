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

/* Scripts/Engines/AngelIsland/AILevelSystem/Mobiles/VengefulSpirit.cs
 * ChangeLog
 *	7/15/07, Adam
 *		- Update mob's STR and not hits. Updating hits 'heals' the creature, and we don't want that
 *			Basically, all players that attack the mob will increase it's STR
 *	6/15/06, Adam
 *		- Move dynamic threat stuff into common base class BaseDynamicThreat
 *	4/8/05, Adam
 *		add the VirtualArmor to the CoreAI global variables and make setable
 *		withing the CoreManagementConsole
 *	9/26/05, Adam
 *		More rebalancing of stats and skills
 *		Normalize with their assigned mob equivalents (pixie, orcish mage, lich, meer eternal)
 *	9/25/05, Adam
 *		Basic rebalancing of stats and skills
 *	9/16/04, Adam
 *		Minor tweaks to the AttackSkill calc.
 *	9/15/04, Adam
 *		Totally redesign the way stats and skills are calculated based on "Threat Analysis"
 *	5/10/04, mith
 *		Modified the way we set this mob's hitpoints.
 *  4/29/04, mith
 *		Modified to use variables in CoreAI.
 */
using System;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a ghostly corpse" )]
	public class VengefulSpirit : BaseDynamicThreat
	{
		[Constructable]
		public VengefulSpirit() : base( AIType.AI_Mage, FightMode.All | FightMode.Weakest, 10, 1, 0.2, 0.4 )
		{
			Name = "a Vengeful Spirit";
			Body = 153;
			BaseSoundID = 0x482;
			BardImmune = true;
			BaseHits = CoreAI.SpiritSecondWaveHP; 
			BaseVirtualArmor = CoreAI.SpiritSecondWaveVirtualArmor;

			Fame = 0;
			Karma = 0;

			InitStats(BaseHits, BaseVirtualArmor);

			AddItem( new LightSource() );
		}

		public override void InitStats(int iHits, int iVirtualArmor)
		{
			// ORCISH MAGE - Stats
			// Adam: Setting Str and not hits makes hits and str equiv
			//	Don't set hits as it 'heals' the mob, we are instead increasing STR 
			//	which will bump hits too
			// SetStr( 116, 150 ); 
			SetStr(iHits); 
			SetDex( 91, 115 );
			SetInt( 161, 185 );
			//SetHits(BaseHits);
			SetDamage( 4, 14 );

			SetSkill( SkillName.EvalInt, 60.1, 72.5 );
			SetSkill( SkillName.Magery, 60.1, 72.5 );
			SetSkill( SkillName.Meditation, 85.1, 95.0 );
			SetSkill( SkillName.MagicResist, 60.1, 75.0 );
			SetSkill( SkillName.Tactics, 50.1, 65.0 );
			SetSkill( SkillName.Wrestling, 40.1, 50.0 );

			VirtualArmor = iVirtualArmor;		
		}

		public override bool InitialInnocent{ get{ return true; } }

		public VengefulSpirit( Serial serial ) : base( serial )
		{
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