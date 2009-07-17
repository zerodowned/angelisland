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

/* Scripts/Mobiles/Animals/Slimes/Jwilson.cs
 * ChangeLog
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 */

using System;
using Server.Mobiles;

namespace Server.Mobiles
{
	[CorpseName( "a jwilson corpse" )]
	public class Jwilson : BaseCreature
	{
		[Constructable]
		public Jwilson() : base( AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.3, 0.6 )
		{
			Hue = Utility.RandomList(0x89C,0x8A2,0x8A8,0x8AE);
			this.Body = 0x33;
			this.Name = ("a jwilson");
			this.VirtualArmor = 8;

			this.InitStats(Utility.Random(22,13),Utility.Random(16,6),Utility.Random(16,5));

			this.Skills[SkillName.Wrestling].Base = Utility.Random(24,17);
			this.Skills[SkillName.Tactics].Base = Utility.Random(18,14);
			this.Skills[SkillName.MagicResist].Base = Utility.Random(15,6);
			this.Skills[SkillName.Poisoning].Base = Utility.Random(31,20);

			this.Fame = Utility.Random(0,1249);
			this.Karma = Utility.Random(0,-624);
		}

		public Jwilson(Serial serial) : base(serial)
		{
		}

		public override int GetAngerSound() 
		{ 
			return 0x1C8; 
		} 

		public override int GetIdleSound() 
		{ 
			return 0x1C9; 
		} 

		public override int GetAttackSound() 
		{ 
			return 0x1CA; 
		} 

		public override int GetHurtSound() 
		{ 
			return 0x1CB; 
		} 

		public override int GetDeathSound() 
		{ 
			return 0x1CC; 
		} 

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
