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
using Server.Items;

namespace Server.Mobiles
{
	public class SBMaceWeapon: SBInfo
	{
		private ArrayList m_BuyInfo = new InternalBuyInfo();
		private IShopSellInfo m_SellInfo = new InternalSellInfo();

		public SBMaceWeapon()
		{
		}

		public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
		public override ArrayList BuyInfo { get { return m_BuyInfo; } }

		public class InternalBuyInfo : ArrayList
		{
			public InternalBuyInfo()
			{
				Add( new GenericBuyInfo( typeof( Club ), 27, 20, 0x13B4, 0 ) );
				Add( new GenericBuyInfo( typeof( HammerPick ), 31, 20, 0x143D, 0 ) );
				Add( new GenericBuyInfo( typeof( Mace ), 38, 20, 0xF5C, 0 ) );
				Add( new GenericBuyInfo( typeof( Maul ), 31, 20, 0x143B, 0 ) );
				Add( new GenericBuyInfo( typeof( WarHammer ), 27, 20, 0x1439, 0 ) );
				Add( new GenericBuyInfo( typeof( WarMace ), 37, 20, 0x1407, 0 ) );
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			public InternalSellInfo()
			{
/*				Add( typeof( Club ), 13 );
 *				Add( typeof( HammerPick ), 15 );
 *				Add( typeof( Mace ), 30 );
 *				Add( typeof( Maul ), 15 );
 *				Add( typeof( WarHammer ), 13 );
 *				Add( typeof( WarMace ), 18 );
 */			}
		}
	}
}
