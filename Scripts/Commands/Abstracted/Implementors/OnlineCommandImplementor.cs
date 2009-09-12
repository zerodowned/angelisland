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

/* Scripts/Commands/Abstracted/Implementors/OnlineCommandImplementor.cs
 * CHANGELOG
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Network;

namespace Server.Scripts.Commands
{
	public class OnlineCommandImplementor : BaseCommandImplementor
	{
		public OnlineCommandImplementor()
		{
			Accessors = new string[] { "Online" };
			SupportRequirement = CommandSupport.Online;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.Administrator;
			Usage = "Online <command> [condition]";
			Description = "Invokes the command on all mobiles that are currently logged in. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Compile(Mobile from, BaseCommand command, ref string[] args, ref object obj)
		{
			try
			{
				ObjectConditional cond = ObjectConditional.Parse(from, ref args);

				bool items, mobiles;

				if (!CheckObjectTypes(command, cond, out items, out mobiles))
					return;

				if (!mobiles) // sanity check
				{
					command.LogFailure("This command does not support mobiles.");
					return;
				}

				ArrayList list = new ArrayList();

				//ArrayList states = NetState.Instances;
				List<NetState> states = NetState.Instances;

				for (int i = 0; i < states.Count; ++i)
				{
					NetState ns = states[i];
					Mobile mob = ns.Mobile;

					if (mob != null && cond.CheckCondition(mob))
						list.Add(mob);
				}

				obj = list;
			}
			catch (Exception ex)
			{
				LogHelper.LogException(ex);
				from.SendMessage(ex.Message);
			}
		}
	}
}