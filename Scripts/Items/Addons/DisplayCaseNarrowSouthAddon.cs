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
/*   changelog.
 *   08/03/06,Rhiannon
 *		Initial creation
 *
 *
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
	public class DisplayCaseNarrowSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed
		{
			get
			{
				return new DisplayCaseNarrowSouthAddonDeed();
			}
		}

		[ Constructable ]
		public DisplayCaseNarrowSouthAddon()
		{
			AddComponent( new AddonComponent( 2723 ), -1, 0, 0 );
			AddComponent( new AddonComponent( 2723 ), -1, 0, 6 );
			AddComponent( new AddonComponent( 2832 ), -1, 0, 3 );
			AddComponent( new AddonComponent( 2722 ), 0, 0, 6 );
			AddComponent( new AddonComponent( 2839 ), 0, 0, 3 );
			AddComponent( new AddonComponent( 2722 ), 1, 0, 6 );
			AddComponent( new AddonComponent( 2839 ), 1, 0, 3 );
			AddComponent( new AddonComponent( 2724 ), 2, 0, 0 );
			AddComponent( new AddonComponent( 2724 ), 2, 0, 6 );
			AddComponent( new AddonComponent( 2835 ), 2, 0, 3 );
			AddComponent( new AddonComponent( 2840 ), 2, 1, 0 );
			AddComponent( new AddonComponent( 2840 ), 2, 1, 6 );
			AddComponent( new AddonComponent( 2833 ), 2, 1, 3 );
			AddComponent( new AddonComponent( 2720 ), 1, 1, 6 );
			AddComponent( new AddonComponent( 2837 ), 1, 1, 3 );
			AddComponent( new AddonComponent( 2720 ), 0, 1, 6 );
			AddComponent( new AddonComponent( 2837 ), 0, 1, 3 );
			AddComponent( new AddonComponent( 2725 ), -1, 1, 0 );
			AddComponent( new AddonComponent( 2725 ), -1, 1, 6 );
			AddComponent( new AddonComponent( 2834 ), -1, 1, 3 );
			AddonComponent ac = null;
			ac = new AddonComponent( 2723 );
			AddComponent( ac, -1, 0, 0 );
			ac = new AddonComponent( 2722 );
			AddComponent( ac, 1, 0, 6 );
			ac = new AddonComponent( 2722 );
			AddComponent( ac, 0, 0, 6 );
			ac = new AddonComponent( 2723 );
			AddComponent( ac, -1, 0, 6 );
			ac = new AddonComponent( 2839 );
			AddComponent( ac, 1, 0, 3 );
			ac = new AddonComponent( 2839 );
			AddComponent( ac, 0, 0, 3 );
			ac = new AddonComponent( 2832 );
			AddComponent( ac, -1, 0, 3 );

		}

		public DisplayCaseNarrowSouthAddon( Serial serial ) : base( serial )
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

	public class DisplayCaseNarrowSouthAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new DisplayCaseNarrowSouthAddon();
			}
		}

		[Constructable]
		public DisplayCaseNarrowSouthAddonDeed()
		{
			Name = "narrow display case (south)";
		}

		public DisplayCaseNarrowSouthAddonDeed( Serial serial ) : base( serial )
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