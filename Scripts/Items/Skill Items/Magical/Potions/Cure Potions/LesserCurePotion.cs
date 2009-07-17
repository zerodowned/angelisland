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

/* Items/SkillItems/Magical/Potions/Cure Potions/LesserCurePotion.cs
 * CHANGELOG:
 *	10/16/05, Pix
 *		Removed AOS cure levels.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;

namespace Server.Items
{
	public class LesserCurePotion : BaseCurePotion
	{
		private static CureLevelInfo[] m_OldLevelInfo = new CureLevelInfo[]
		{
			new CureLevelInfo( Poison.Lesser,  0.75 ), // 75% chance to cure lesser poison
			new CureLevelInfo( Poison.Regular, 0.50 ), // 50% chance to cure regular poison
			new CureLevelInfo( Poison.Greater, 0.15 )  // 15% chance to cure greater poison
		};

		public override CureLevelInfo[] LevelInfo{ get{ return m_OldLevelInfo; } }

		[Constructable]
		public LesserCurePotion() : base( PotionEffect.CureLesser )
		{
		}

		public LesserCurePotion( Serial serial ) : base( serial )
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