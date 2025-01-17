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

/* CHANGELOG
 *	11/30/05,Adam
 *		First time check in of the Teleporter platform
 */

/////////////////////////////////////////////////
//
// Automatically generated by the
// AddonGenerator script by Arya
//
/////////////////////////////////////////////////
using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class TeleporterAndSignAddon : BaseAddon
	{
		public override BaseAddonDeed Deed
		{
			get
			{
				return new TeleporterAndSignAddonDeed();
			}
		}

		[ Constructable ]
		public TeleporterAndSignAddon()
		{
			AddonComponent ac = null;
			ac = new AddonComponent( 1876 );
			AddComponent( ac, 0, 0, 0 );
			ac = new AddonComponent( 1872 );
			AddComponent( ac, 1, 0, 0 );
			ac = new AddonComponent( 7977 );
			AddComponent( ac, -2, -1, 15 );
			ac = new AddonComponent( 9 );
			AddComponent( ac, -2, -1, 0 );
			ac = new AddonComponent( 14170 );
			AddComponent( ac, 2, 0, 6 );
			ac = new AddonComponent( 1878 );
			AddComponent( ac, 3, 1, 0 );
			ac = new AddonComponent( 1880 );
			AddComponent( ac, 0, 1, 0 );
			ac = new AddonComponent( 1873 );
			AddComponent( ac, 1, 1, 0 );
			ac = new AddonComponent( 1873 );
			AddComponent( ac, 2, 1, 0 );
			ac = new AddonComponent( 1877 );
			AddComponent( ac, 0, -1, 0 );
			ac = new AddonComponent( 1879 );
			AddComponent( ac, 3, -1, 0 );
			ac = new AddonComponent( 1875 );
			AddComponent( ac, 2, -1, 0 );
			ac = new AddonComponent( 1875 );
			AddComponent( ac, 1, -1, 0 );
			ac = new AddonComponent( 1872 );
			AddComponent( ac, 2, 0, 0 );
			ac = new AddonComponent( 1874 );
			AddComponent( ac, 3, 0, 0 );

		}

		public TeleporterAndSignAddon( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class TeleporterAndSignAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new TeleporterAndSignAddon();
			}
		}

		[Constructable]
		public TeleporterAndSignAddonDeed()
		{
			Name = "TeleporterAndSign";
		}

		public TeleporterAndSignAddonDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void	Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}