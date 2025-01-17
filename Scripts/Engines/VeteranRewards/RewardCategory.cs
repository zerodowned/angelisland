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

using System;
using System.Collections;

namespace Server.Engines.VeteranRewards
{
	public class RewardCategory
	{
		private int m_Name;
		private string m_NameString;
		private ArrayList m_Entries;

		public int Name{ get{ return m_Name; } }
		public string NameString{ get{ return m_NameString; } }
		public ArrayList Entries{ get{ return m_Entries; } }

		public RewardCategory( int name )
		{
			m_Name = name;
			m_Entries = new ArrayList();
		}

		public RewardCategory( string name )
		{
			m_NameString = name;
			m_Entries = new ArrayList();
		}
	}
} 
