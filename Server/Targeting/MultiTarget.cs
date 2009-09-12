/***************************************************************************
 *                               MultiTarget.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id: MultiTarget.cs,v 1.1 2005/02/22 00:58:25 adam Exp $
 *   $Author: adam $
 *   $Date: 2005/02/22 00:58:25 $
 *
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

using System;
using Server;
using Server.Network;

namespace Server.Targeting
{
	public abstract class MultiTarget : Target
	{
		private int m_MultiID;
		private Point3D m_Offset;

		public int MultiID
		{
			get
			{
				return m_MultiID;
			}
			set
			{
				m_MultiID = value;
			}
		}

		public Point3D Offset
		{
			get
			{
				return m_Offset;
			}
			set
			{
				m_Offset = value;
			}
		}

		public MultiTarget(int multiID, Point3D offset)
			: this(multiID, offset, 10, true, TargetFlags.None)
		{
		}

		public MultiTarget(int multiID, Point3D offset, int range, bool allowGround, TargetFlags flags)
			: base(range, allowGround, flags)
		{
			m_MultiID = multiID;
			m_Offset = offset;
		}

		public override Packet GetPacket()
		{
			return new MultiTargetReq(this);
		}
	}
}