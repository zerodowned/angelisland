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

namespace Server.Engines.Chat
{
	public enum ChatCommand
	{
		/// <summary>
		/// Add a channel to top list.
		/// </summary>
		AddChannel = 0x3E8,
		/// <summary>
		/// Remove channel from top list.
		/// </summary>
		RemoveChannel = 0x3E9,
		/// <summary>
		/// Queries for a new chat nickname.
		/// </summary>
		AskNewNickname = 0x3EB,
		/// <summary>
		/// Closes the chat window.
		/// </summary>
		CloseChatWindow = 0x3EC,
		/// <summary>
		/// Opens the chat window.
		/// </summary>
		OpenChatWindow = 0x3ED,
		/// <summary>
		/// Add a user to current channel.
		/// </summary>
		AddUserToChannel = 0x3EE,
		/// <summary>
		/// Remove a user from current channel.
		/// </summary>
		RemoveUserFromChannel = 0x3EF,
		/// <summary>
		/// Send a message putting generic conference name at top when player leaves a channel.
		/// </summary>
		LeaveChannel = 0x3F0,
		/// <summary>
		/// Send a message putting Channel name at top and telling player he joined the channel.
		/// </summary>
		JoinedChannel = 0x3F1
	}
} 
