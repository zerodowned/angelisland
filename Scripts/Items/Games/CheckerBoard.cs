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

namespace Server.Items
{
	public class CheckerBoard : BaseBoard
	{
		public override int LabelNumber{ get{ return 1016449; } } // a checker board

		public override int DefaultGumpID{ get{ return 0x91A; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 0, 0, 282, 210 ); }
		}

		[Constructable]
		public CheckerBoard() : base( 0xFA6 )
		{
		}

		public override void CreatePieces()
		{
			for ( int i = 0; i < 4; i++ )
			{
				CreatePiece( new PieceWhiteChecker( this ), ( 50 * i ) + 45, 25 );
				CreatePiece( new PieceWhiteChecker( this ), ( 50 * i ) + 70, 50 );
				CreatePiece( new PieceWhiteChecker( this ), ( 50 * i ) + 45, 75 );
				CreatePiece( new PieceBlackChecker( this ), ( 50 * i ) + 70, 150 );
				CreatePiece( new PieceBlackChecker( this ), ( 50 * i ) + 45, 175 );
				CreatePiece( new PieceBlackChecker( this ), ( 50 * i ) + 70, 200 );
			}
		}

		public CheckerBoard( Serial serial ) : base( serial )
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
	}
} 
