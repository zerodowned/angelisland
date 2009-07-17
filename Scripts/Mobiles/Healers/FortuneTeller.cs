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

/* Scripts/Mobiles/Healers/FortuneTeller.cs 
 * Changelog
 *	06/28/06, Adam
 *		Logic cleanup
 */

using System;
using System.Collections;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	public class FortuneTeller : BaseHealer
	{
		public override bool CanTeach{ get{ return true; } }

		public override bool CheckTeach( SkillName skill, Mobile from )
		{
			if ( !base.CheckTeach( skill, from ) )
				return false;

			return ( skill == SkillName.Anatomy )
				|| ( skill == SkillName.Healing )
				|| ( skill == SkillName.Forensics )
				|| ( skill == SkillName.SpiritSpeak );
		}

		[Constructable]
		public FortuneTeller()
		{
			Title = "the fortune teller";

			SetSkill( SkillName.Anatomy, 85.0, 100.0 );
			SetSkill( SkillName.Healing, 90.0, 100.0 );
			SetSkill( SkillName.Forensics, 75.0, 98.0 );
			SetSkill( SkillName.SpiritSpeak, 65.0, 88.0 );
		}

		public override bool IsActiveVendor{ get{ return true; } }
	
		public override void InitSBInfo()
		{
			SBInfos.Add( new SBMage() );
			SBInfos.Add( new SBFortuneTeller() );
		}

		public override int GetRobeColor()
		{
			return RandomBrightHue();
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			switch ( Utility.Random( 3 ) )
			{
				case 0: AddItem( new SkullCap( RandomBrightHue() ) ); break;
				case 1: AddItem( new WizardsHat( RandomBrightHue() ) ); break;
				case 2: AddItem( new Bandana( RandomBrightHue() ) ); break;
			}

			AddItem( new Spellbook() );
		}

		public FortuneTeller( Serial serial ) : base( serial )
		{
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