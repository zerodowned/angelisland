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

/* Scripts/Items/Armor/Bone/BoneLegs.cs
 * ChangeLog
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 15 lines removed.
 *  1/23/05, Froste
 *  Add Bone Magi variant of this Bone piece. Meddable, AR like leather, Exceptional.
 *	9/10/04, Pigpen
 *	Add Unholy Bone variant of this Bone piece.
 */

using System;
using Server.Items;
using System.Collections;
using Server.Network;

namespace Server.Items
{
	[FlipableAttribute( 0x1452, 0x1457 )]
	public class BoneLegs : BaseArmor
	{

		public override int InitMinHits{ get{ return 25; } }
		public override int InitMaxHits{ get{ return 30; } }

		public override int AosStrReq{ get{ return 55; } }
		public override int OldStrReq{ get{ return 40; } }

		public override int OldDexBonus{ get{ return -4; } }

		public override int ArmorBase{ get{ return 30; } }
		public override int RevertArmorBase{ get{ return 7; } }

		public override ArmorMaterialType MaterialType{ get{ return ArmorMaterialType.Bone; } }
		public override CraftResource DefaultResource{ get{ return CraftResource.RegularLeather; } }

		[Constructable]
		public BoneLegs() : base( 0x1452 )
		{
			Weight = 3.0;
		}

		public BoneLegs( Serial serial ) : base( serial )
		{
		}

		public override string OldName
		{
			get
			{
				return "bone leggings";
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
		}
	}	
	
		public class UnholyBoneLegs : BaseArmor
	{

		public override int InitMinHits{ get{ return 25; } }
		public override int InitMaxHits{ get{ return 30; } }

		public override int AosStrReq{ get{ return 55; } }
		public override int OldStrReq{ get{ return 40; } }

		public override int OldDexBonus{ get{ return -4; } }

		public override int ArmorBase{ get{ return 30; } }
		public override int RevertArmorBase{ get{ return 7; } }

		public override ArmorMaterialType MaterialType{ get{ return ArmorMaterialType.Bone; } }
		public override CraftResource DefaultResource{ get{ return CraftResource.RegularLeather; } }

		[Constructable]
		public UnholyBoneLegs() : base( 0x1452 )
		{
			Weight = 3.0;
			Hue = 1109;
			ProtectionLevel = ArmorProtectionLevel.Guarding;
			Durability = ArmorDurabilityLevel.Massive;
			if (Utility.RandomDouble() < 0.20)
				Quality = ArmorQuality.Exceptional;
			Name = "Unholy Bone Legs";
		}

		public UnholyBoneLegs( Serial serial ) : base( serial )
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
		}
	}

    public class BoneMagiLegs : BaseArmor
    {

        public override int InitMinHits { get { return 25; } }
        public override int InitMaxHits { get { return 30; } }

        public override int AosStrReq { get { return 55; } }
        public override int OldStrReq { get { return 40; } }

        // public override int OldDexBonus { get { return -4; } }

        public override int ArmorBase { get { return 13; } }
        public override int RevertArmorBase { get { return 7; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        [Constructable]
        public BoneMagiLegs() : base( 0x1452 )
        {
            Weight = 3.0;
            Quality = ArmorQuality.Exceptional;
            Name = "Legs of the Bone Magi";
        }

        public BoneMagiLegs(Serial serial) : base( serial )
        {
        }

        // Special version that DOES NOT show armor attributes and tags
        public override void OnSingleClick(Mobile from)
        {
            ArrayList attrs = new ArrayList();

            int number;

            if (Name == null)
            {
                number = LabelNumber;
            }
            else
            {
                this.LabelTo(from, Name);
                number = 1041000;
            }

            if (attrs.Count == 0 && Crafter == null && Name != null)
                return;

            EquipmentInfo eqInfo = new EquipmentInfo(number, Crafter, false, (EquipInfoAttribute[])attrs.ToArray(typeof(EquipInfoAttribute)));

            from.Send(new DisplayEquipmentInfo(this, eqInfo));

        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
