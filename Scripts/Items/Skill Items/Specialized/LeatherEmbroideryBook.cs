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

/* Scripts/Items/Skill Items/Specialized/LeatherEmbroideryBook.cs
 * ChangeLog:
 *	04/02/2008
 *		Initial creation
 */

using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
	public class LeatherEmbroideryBook : Item
	{
		public const double c_RequiredTailoring = 90.0;
		public const double c_RequiredInscribe = 80.0;

		[Constructable]
		public LeatherEmbroideryBook() : base( 0xFF4 )
		{
			Name = "Leather Embroidering";
			Weight = 1.0;
		}

		public LeatherEmbroideryBook(Serial serial)
			: base(serial)
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

		public override void OnDoubleClick( Mobile from )
		{
			PlayerMobile pm;
			
			if(from is PlayerMobile)
				pm = (PlayerMobile)from;
			else
				return;

			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
			else if (pm.LeatherEmbroidering)
			{
				pm.SendMessage("You have already learned this.");
			}
			else if (pm.Skills[SkillName.Tailoring].Base < c_RequiredTailoring
			         || pm.Skills[SkillName.Inscribe].Base < c_RequiredInscribe)
			{
				pm.SendMessage("Only one who is a both a Master Tailor and an Expert Scribe can learn from this book.");
			}
			else
			{
				pm.LeatherEmbroidering = true;
				pm.SendMessage("You have learned the art of embroidering leather. Use heavy embroidery floss to customize your hand-crafted goods.");
				Delete();
			}
		}
	}
}
