/***************************************************************************
 *                               VirtueInfo.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id: VirtueInfo.cs,v 1.1 2005/02/22 00:56:31 adam Exp $
 *   $Author: adam $
 *   $Date: 2005/02/22 00:56:31 $
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

namespace Server
{
	[PropertyObject]
	public class VirtueInfo
	{
		private int[] m_Values;

		public int[] Values
		{
			get
			{
				return m_Values;
			}
		}

		public int GetValue(int index)
		{
			if (m_Values == null)
				return 0;
			else
				return m_Values[index];
		}

		public void SetValue(int index, int value)
		{
			if (m_Values == null)
				m_Values = new int[8];

			m_Values[index] = value;
		}

		public override string ToString()
		{
			return "...";
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Humility { get { return GetValue(0); } set { SetValue(0, value); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Sacrifice { get { return GetValue(1); } set { SetValue(1, value); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Compassion { get { return GetValue(2); } set { SetValue(2, value); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Spirituality { get { return GetValue(3); } set { SetValue(3, value); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Valor { get { return GetValue(4); } set { SetValue(4, value); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Honor { get { return GetValue(5); } set { SetValue(5, value); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Justice { get { return GetValue(6); } set { SetValue(6, value); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Honesty { get { return GetValue(7); } set { SetValue(7, value); } }

		public VirtueInfo()
		{
		}

		public VirtueInfo(GenericReader reader)
		{
			int version = reader.ReadByte();

			switch (version)
			{
				case 0:
					{
						int mask = reader.ReadByte();

						if (mask != 0)
						{
							m_Values = new int[8];

							for (int i = 0; i < 8; ++i)
								if ((mask & (1 << i)) != 0)
									m_Values[i] = reader.ReadInt();
						}

						break;
					}
			}
		}

		public static void Serialize(GenericWriter writer, VirtueInfo info)
		{
			writer.Write((byte)0); // version

			if (info.m_Values == null)
			{
				writer.Write((byte)0);
			}
			else
			{
				int mask = 0;

				for (int i = 0; i < 8; ++i)
					if (info.m_Values[i] != 0)
						mask |= 1 << i;

				writer.Write((byte)mask);

				for (int i = 0; i < 8; ++i)
					if (info.m_Values[i] != 0)
						writer.Write((int)info.m_Values[i]);
			}
		}
	}
}