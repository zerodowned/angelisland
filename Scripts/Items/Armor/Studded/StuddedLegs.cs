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
 
 /* Scripts/Items/Armor/Studded/StuddedLegs.cs
 * ChangeLog
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 10 lines removed.
 *	9/11/04, Pigpen
 *	Add Wyrm Skin variant of this Studded piece.
 */	

using System;
using Server.Items;
using System.Collections;
using Server.Network;

namespace Server.Items
{
	[FlipableAttribute( 0x13da, 0x13e1 )]
	public class StuddedLegs : BaseArmor
	{

		public override int InitMinHits{ get{ return 35; } }
		public override int InitMaxHits{ get{ return 45; } }

		public override int AosStrReq{ get{ return 30; } }
		public override int OldStrReq{ get{ return 35; } }

		public override int ArmorBase{ get{ return 16; } }

		public override ArmorMaterialType MaterialType{ get{ return ArmorMaterialType.Studded; } }
		public override CraftResource DefaultResource{ get{ return CraftResource.RegularLeather; } }

		[Constructable]
		public StuddedLegs() : base( 0x13DA )
		{
			Weight = 5.0;
		}

		public StuddedLegs( Serial serial ) : base( serial )
		{
		}

		public override string OldName
		{
			get
			{
				return "studded leggings";
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			if ( Weight == 3.0 )
				Weight = 5.0;
		}
	}	
		public class WyrmSkinLeggings : BaseArmor
	{

		public override int InitMinHits{ get{ return 35; } }
		public override int InitMaxHits{ get{ return 45; } }

		public override int AosStrReq{ get{ return 30; } }
		public override int OldStrReq{ get{ return 35; } }

		public override int ArmorBase{ get{ return 16; } }

		public override ArmorMaterialType MaterialType{ get{ return ArmorMaterialType.Studded; } }
		public override CraftResource DefaultResource{ get{ return CraftResource.RegularLeather; } }

		[Constructable]
		public WyrmSkinLeggings() : base( 0x13DA )
		{
			Weight = 5.0;
			Hue = 1367;
			ProtectionLevel = ArmorProtectionLevel.Guarding;
			Durability = ArmorDurabilityLevel.Massive;
			if (Utility.RandomDouble() < 0.20)
				Quality = ArmorQuality.Exceptional;
			Name = "Wyrm Skin Leggings";
		}

		public WyrmSkinLeggings( Serial serial ) : base( serial )
		{
		}
		
		// Special version that DOES NOT show armor attributes and tags
		public override void OnSingleClick( Mobile from )
		{
			ArrayList attrs = new ArrayList();

			int number;

			if ( Name == null )
			{
				number = LabelNumber;
			}
			else
			{
				this.LabelTo( from, Name );
				number = 1041000;
			}

			if ( attrs.Count == 0 && Crafter == null && Name != null )
				return;

			EquipmentInfo eqInfo = new EquipmentInfo( number, Crafter, false, (EquipInfoAttribute[])attrs.ToArray( typeof( EquipInfoAttribute ) ) );

			from.Send( new DisplayEquipmentInfo( this, eqInfo ) );

		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			if ( Weight == 3.0 )
				Weight = 5.0;
		}
	}
}
