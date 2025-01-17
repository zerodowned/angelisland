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
 *   9/16/04,Lego
 *           Changed Display Name of deed 
 *
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
	public class NujelmMediumSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed
		{
			get
			{
				return new NujelmMediumSouthAddonDeed();
			}
		}

		public override bool BlocksDoors { get { return false; } }

		[ Constructable ]
		public NujelmMediumSouthAddon()
		{
			AddonComponent ac = null;
			ac = new AddonComponent( 2751 );
			AddComponent( ac, 0, 0, 0 );
			ac = new AddonComponent( 2751 );
			AddComponent( ac, 0, 1, 0 );
			ac = new AddonComponent( 2751 );
			AddComponent( ac, 1, 0, 0 );
			ac = new AddonComponent( 2751 );
			AddComponent( ac, 1, 1, 0 );
			ac = new AddonComponent( 2751 );
			AddComponent( ac, 2, 0, 0 );
			ac = new AddonComponent( 2751 );
			AddComponent( ac, 2, 1, 0 );
			ac = new AddonComponent( 2751 );
			AddComponent( ac, -1, 0, 0 );
			ac = new AddonComponent( 2751 );
			AddComponent( ac, -1, 1, 0 );
			ac = new AddonComponent( 2754 );
			AddComponent( ac, 3, 2, 0 );
			ac = new AddonComponent( 2755 );
			AddComponent( ac, -2, -1, 0 );
			ac = new AddonComponent( 2756 );
			AddComponent( ac, -2, 2, 0 );
			ac = new AddonComponent( 2757 );
			AddComponent( ac, 3, -1, 0 );
			ac = new AddonComponent( 2806 );
			AddComponent( ac, -2, 0, 0 );
			ac = new AddonComponent( 2806 );
			AddComponent( ac, -2, 1, 0 );
			ac = new AddonComponent( 2807 );
			AddComponent( ac, 0, -1, 0 );
			ac = new AddonComponent( 2807 );
			AddComponent( ac, 1, -1, 0 );
			ac = new AddonComponent( 2807 );
			AddComponent( ac, 2, -1, 0 );
			ac = new AddonComponent( 2807 );
			AddComponent( ac, -1, -1, 0 );
			ac = new AddonComponent( 2808 );
			AddComponent( ac, 3, 0, 0 );
			ac = new AddonComponent( 2808 );
			AddComponent( ac, 3, 1, 0 );
			ac = new AddonComponent( 2809 );
			AddComponent( ac, 0, 2, 0 );
			ac = new AddonComponent( 2809 );
			AddComponent( ac, 1, 2, 0 );
			ac = new AddonComponent( 2809 );
			AddComponent( ac, 2, 2, 0 );
			ac = new AddonComponent( 2809 );
			AddComponent( ac, -1, 2, 0 );

		}

		public NujelmMediumSouthAddon( Serial serial ) : base( serial )
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

	public class NujelmMediumSouthAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new NujelmMediumSouthAddon();
			}
		}

		[Constructable]
		public NujelmMediumSouthAddonDeed()
		{
			Name = "Nujelm Medium Carpet [South]";
		}

		public NujelmMediumSouthAddonDeed( Serial serial ) : base( serial )
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