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

/* Items/Lights/Lantern.cs
 * ChangeLog:
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;

namespace Server.Items
{
	public class Lantern : BaseEquipableLight
	{
		public override int LitItemID
		{
			get
			{
				if ( ItemID == 0xA15 || ItemID == 0xA17 )
					return ItemID;

				return 0xA22;
			}
		}

		public override int UnlitItemID
		{
			get
			{
				if ( ItemID == 0xA18 )
					return ItemID;

				return 0xA25;
			}
		}

		[Constructable]
		public Lantern() : base( 0xA25 )
		{
			if ( Burnout )
				Duration = TimeSpan.FromMinutes( 20 );
			else
				Duration = TimeSpan.Zero;

			Burning = false;
			Light = LightType.Circle300;
			Weight = 2.0;
		}

		public Lantern( Serial serial ) : base( serial )
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

	public class LanternOfSouls : Lantern
	{
		public override int LabelNumber{ get{ return 1061618; } } // Lantern of Souls

		[Constructable]
		public LanternOfSouls()
		{
			Hue = 0x482;
		}

		public LanternOfSouls( Serial serial ) : base( serial )
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