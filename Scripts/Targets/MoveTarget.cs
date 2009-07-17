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
/* ChangeLog:
	6/5/04, Pix
		Merged in 1.0RC0 code.
*/

using System;
using Server;
using Server.Targeting;
using Server.Scripts.Commands;

namespace Server.Targets
{
	public class MoveTarget : Target
	{
		private object m_Object;

		public MoveTarget( object o ) : base( -1, true, TargetFlags.None )
		{
			m_Object = o;
		}

		protected override void OnTarget( Mobile from, object o )
		{
			IPoint3D p = o as IPoint3D;

			if ( p != null )
			{
				if ( !BaseCommand.IsAccessible( from, m_Object ) )
				{
					from.SendMessage( "That is not accessible." );
					return;
				}

				if ( p is Item )
					p = ((Item)p).GetWorldTop();

				Server.Scripts.Commands.CommandLogging.WriteLine( from, "{0} {1} moving {2} to {3}", from.AccessLevel, Server.Scripts.Commands.CommandLogging.Format( from ), Server.Scripts.Commands.CommandLogging.Format( m_Object ), new Point3D( p ) );

				if ( m_Object is Item )
				{
					Item item = (Item)m_Object;

					if ( !item.Deleted )
						item.MoveToWorld( new Point3D( p ), from.Map );
				}
				else if ( m_Object is Mobile )
				{
					Mobile m = (Mobile)m_Object;

					if ( !m.Deleted )
						m.MoveToWorld( new Point3D( p ), from.Map );
				}
			}
		}
	}
}