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

/* Scripts\Engines\IOBSystem\Mobiles\Guards\Types\FactionWizard.cs
 * ChangeLog
 *  12/17/08, Adam
 *		Initial creation
 */

using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.IOBSystem
{
	public class FactionWizard : BaseFactionGuard
	{
		[Constructable]
		public FactionWizard()
			: base("the wizard")
		{
			BardImmune = true;
			FightStyle = FightStyle.Magic | FightStyle.Smart | FightStyle.Bless | FightStyle.Curse;
			UsesHumanWeapons = true;
			UsesBandages = true;
			UsesPotions = true;
			CanRun = true;
			CanReveal = true; // magic and smart

			GenerateBody(false, false);

			SetStr(151, 175);
			SetDex(61, 85);
			SetInt(151, 175);

			VirtualArmor = 32;

			SetSkill(SkillName.Macing, 110.0, 120.0);
			SetSkill(SkillName.Wrestling, 110.0, 120.0);
			SetSkill(SkillName.Tactics, 110.0, 120.0);
			SetSkill(SkillName.MagicResist, 110.0, 120.0);
			SetSkill(SkillName.Healing, 110.0, 120.0);
			SetSkill(SkillName.Anatomy, 110.0, 120.0);

			SetSkill(SkillName.Magery, 110.0, 120.0);
			SetSkill(SkillName.EvalInt, 110.0, 120.0);
			SetSkill(SkillName.Meditation, 110.0, 120.0);

			AddItem(new WizardsHat());
			AddItem(new Sandals());
			AddItem(new Robe());
			AddItem(new LeatherGloves());
			AddItem(new GnarledStaff());

			PackItem(new Bandage(Utility.RandomMinMax(VirtualArmor, VirtualArmor * 2)));
			PackStrongPotions(6, 12);
			PackItem(new Pouch());
		}

		public FactionWizard(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}