/*
 *	This program is the CONFIDENTIAL and PROPRIETARY property
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c) 2004 Tomasello Software LLC.
 *	This is an unpublished work, and is subject to limited distribution and
 *	restricted disclosure only. ALL RIGHTS RESERVED.
 *
 *			RESTRICTED RIGHTS LEGEND
 *	Use, duplication, or disclosure by the Government is subject to
 *	restrictions set forth in subparagraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Server/Mobile.cs
 * CHANGELOG:
 *	1/7/09, Adam
 *		Recast LootType.Internal as LootType.Special. This is like blessed+Cursed (stealable, unlootable)
 *	1/4/09, Adam
 *		Add IntMapStorage like we do for Items
 *	12/24/08, Adam
 *		Add missing  p.Acquire() and Packet.Release(p) for UnicodeMessage
 *	12/23/08, Adam
 *		Add missing network and timer synchronization from RunUO 2.0
 *	07/27/08, weaver
 *		Fixed a problem with the removal logic so the gumps actually get removed.
 *	07/27/08, weaver
 *		Correctly remove gumps from the NetState object on CloseGump() (integrated RunUO fix).
 *	7/23/08, Adam
 *		EventSink.InvokeItemLifted() when an item is lifted to drop player mobiles when items are removed
 *			from underneath them.
 *	5/16/08, Adam
 *		Update Move() to do OnMoveOff() and OnMoveOver() processing outside of the list enumeration.
 *		It's leagal to do something like this.Delete() in OnMoveOver(), but this would crash the server.
 *	4/24/08, Adam
 *		making public virtual TimeSpan ComputeMovementSpeed(Direction dir) virtual so that it can be correctly overridden in PlayerMobile.
 *	1/19/08, Adam
 *		When making Lift() checks we check to see if we are lifting an item from an NPC. the old code just said that if 
 *		NonLocalLift() returns false then LRReason.TryToSteal should be returned to the client which results in a "you must steal it" message.
 *		This is really the wrong behavior if you OWN the npc in question. For instance, your own Player Vendor may not allow you to 
 *		lift an item if it is not-for-sale and non-empty. The PlayerVendor issues a reasonable message, but the client message 
 *		about having to steal it is confusing. We therefore now check to see if the one lifting the item is the owner of the npc; if so,
 *		we set the response packet to LRReason.Inspecific which generates no message on the client. all that's left is the 'nice' message
 *		spoken by the NPC PlayerVendor.
 *  12/17/07, Adam
 *      Add new StaffOwned property to indicate this vendor (or whatever) is staff owned.
 *	11/21/07, Adam
 *		Change the BaseWeapon damage bonus (based on strength) to take into account the new mobile.STRBonusCap.
 *		This new STRBonusCap allows playerMobiles to have super STR while 'capping' the STR bonus whereby preventing one-hit killing.
 *	08/01/07, Pix
 *		Changed hue of name if beneficial actions on that char will have consequences.
 *  03/25/07, plasma
 *      Added new virtual functions BoneDecayTime() and CorpseDecayTime()
 *  3/19/07, Adam
 *      Add IsChampion flag to allow us to designate any creature as a champ via our Mobile Factory
 *	2/05/07 Taran Kain
 *		Added some flexibility in FastWalkDetection
 *  1/26/07, Adam
 *      Minor code organization, no logic changes
 *  1/08/07 Taran Kain
 *      Removed CheckSkill delegates
 *      Moved SkillCheck logic from Scripts/Misc/SkillCheck.cs to here, virtualized to allow inheritance/overriding
 *	12/27/06, Pix
 *		In Deserialize() - added special logic to ensure the DefensiveSpell lock in m_Actions exists if it should
 *  12/21/06, Kit
 *      Modified CloseAllGumps() to send relay info of button id 0(right click cancel) to any gumps to be closed.
 *      Added CloseAllGumps() to disconnect/logout logic.
 *	11/20/06 Taran Kain
 *		Added *RegenRate logic.
 *		Virtualized VirtualArmor, RawStr, RawDex, RawInt, StatCap.
 *		Added StrMax, DexMax, IntMax. Default value is 100.
 *  7/29/06, Kit
 *		Add Region EquipItem call/test to EquipItem function
*	7/28/06, Adam
 *		Remove OLD access levels
 *  7/26/06, Rhiannon
 *		Changed AccessLevel set function to disallow changing AccessLevel to Owner or ReadOnly.
 *  7/24/06, Rhiannon
 *		Added GetHueForNameInList() to be used by gumps (like WhoGump) that show names in color.
 *  7/24/06, Kit
 *		MAke access level default to 100 aka Player not 0 which is oldplayer.
 *		Fixs problem with players/creatures spawning with wrong access level.
 *	7/23/06, Adam
 *		Add the OldGameMaster ==> GameMaster converter
 *	7/22/06, Adam
 *		Replace the name lookup table for the different access levels with a switch statement as 
 *			the enums are no longer sequential.
 *  7/21/06, Rhiannon
 *		Add new access levels
 *		Convert old access levels to new access levels
 *	7/4/06, Adam
 *		Give ToDelete AccessLevel.GameMaster
 *	7/1/06, Adam
 *		Convert the ToDelete flag to the new MobileFlags bit array
 *  6/29/06, Kit
 *		Changed m_Tithing to m_Flags, added new MobileFlag set.
 *	5/30/06, Pix
 *		Changed LogoutLocation to be viewable with [props.
 *	5/20/06, Pix
 *		Added virtual property DamageEntryExpireTimeSeconds and changed DamageEntry system
 *		to use this property.
 *	4/28/06, weaver
 *		- Added IsAudibleTo(), a new check to see if this mobile is audible to another (passes audible
 *		check flag to LineOfSight routine which then ignores NoShoot flags, which alone should not block
 *		audibility.
 *		- Replaced calls from DoSpeech() and PublicOverheadMessage(s) to InLOS() with IsAudibleTo() calls.
 *	4/4/06, weaver
 *		Added new overloaded version of CanBeHarmful() with an additional parameter
 *		to handle instances where the death of the mobile attempting to damage is
 *		irrelevant in the estimation of damage caused.
 *	2/26/06, Pix
 *		Added RemoveGumps() call so we can remove all gumps from a netstate
 *		(primarily to fix the 'freeze-on-death' bug with ressurection gumps)
 *	2/11/06, Adam
 *		Modify CanBeRenamedBy() so that Counselors cannot rename players.
 *	2/10/06, Adam
 *		1. Add new virtual ProcessItem() to process (in a generic way) an item the player is carrying.
 *		In PlayerMobile this facility us used for placement of a guild stone that is now carried on the player instead
 *		of in deed form (as the FreezeDry system and guild deeds were not compatable.)
 *		2. Add a new function RequestItem(Type type) to get a reference to an item type a mobile may be carrying.
 *		3. Add new DeathMoveResult processing to support DeathMoveResult.KeepInternal.
 *			DeathMoveResult.KeepInternal is set if item.LootType == LootType.Internal.
 *  01/09/06 Taran Kain
 *		Commented out some OnSpeech calls, hope we don't break anything.. was calling them extraneously
 *  01/03/05, Pix
 *		Added SendAlliedChat() virtual for Allied chat system.
 *  12/24/05, Kit
 *		Added bool/property CanFly for use with mobs useing flying movement.
 *		Added array to hold static id's of tiles we can fly over.
 *  11/07/05, Kit
 *		Moved Weapon special moves concussion,parablow,etc from playermobile to here.
 *  10/10/05 Taran Kain
 *		Changed some ints to doubles for more of a floating-point math pipeline
 *  10/08/05 Taran Kain
 *		Added StatChange event to Mobile
 *		Added ClearStatMods()
 *	10/2/05, erlein
 *		Made Paralyze() function virtual
 *	9/26/05, Adam
 *		AggressiveAction(): break out of aggro list loops when we have our answer 
 *		These loops would loop through list items, even after the answer was known.
 *		In a 'Champ' spawn where there may be dozens of aggressors at any one time,
 *		these 4 loops would be consuming undue CPU cycles.
 *	9/24/05, Adam
 *		Add a simple warning to the Alive property:
 *		Even though you may be tempted, do not add an IsDeadBondedPet to this test
 *		A dead bonded pet is still considered 'Alive'
 *	7/26/05, Adam
 *		Massive AOS cleanout
 *	7/5/05, Pix
 *		Added SendGuildChat virtual function to support GuildChat in PlayerMobile
 *  5/30/05, Kit
 *		Changed PlaySound() to virtual
 *	4/27/05, Adam
 *		Give Counselors read-only access to ShortTermMurders 
 *	02/21/05, Adam
 *		Add m_ToDelete property
 */

/***************************************************************************
 *                                Mobile.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id: Mobile.cs,v 1.83 2009/01/07 18:11:36 adam Exp $
 *   $Author: adam $
 *   $Date: 2009/01/07 18:11:36 $
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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Menus;
using Server.Prompts;
using Server.Guilds;
using Server.Network;
using Server.Targeting;
using Server.HuePickers;
using Server.Items;
using Server.Mobiles;
using Server.ContextMenus;
using Server.Accounting;

namespace Server
{
	[Flags]
	public enum StatType
	{
		Str = 1,
		Dex = 2,
		Int = 4,
		All = 7
	}

	public enum StatLockType : byte
	{
		Up,
		Down,
		Locked
	}

	public delegate void TargetCallback(Mobile from, object targeted);
	public delegate void TargetStateCallback(Mobile from, object targeted, object state);

	public class TimedSkillMod : SkillMod
	{
		private DateTime m_Expire;

		public TimedSkillMod(SkillName skill, bool relative, double value, TimeSpan delay)
			: this(skill, relative, value, DateTime.Now + delay)
		{
		}

		public TimedSkillMod(SkillName skill, bool relative, double value, DateTime expire)
			: base(skill, relative, value)
		{
			m_Expire = expire;
		}

		public override bool CheckCondition()
		{
			return (DateTime.Now < m_Expire);
		}
	}

	public class EquipedSkillMod : SkillMod
	{
		private Item m_Item;
		private Mobile m_Mobile;

		public EquipedSkillMod(SkillName skill, bool relative, double value, Item item, Mobile mobile)
			: base(skill, relative, value)
		{
			m_Item = item;
			m_Mobile = mobile;
		}

		public override bool CheckCondition()
		{
			return (!m_Item.Deleted && !m_Mobile.Deleted && m_Item.Parent == m_Mobile);
		}
	}

	public class DefaultSkillMod : SkillMod
	{
		public DefaultSkillMod(SkillName skill, bool relative, double value)
			: base(skill, relative, value)
		{
		}

		public override bool CheckCondition()
		{
			return true;
		}
	}

	public class DamageEntry
	{
		private Mobile m_Damager;
		private int m_DamageGiven;
		private DateTime m_LastDamage;
		private ArrayList m_Responsible;

		public Mobile Damager { get { return m_Damager; } }
		public int DamageGiven { get { return m_DamageGiven; } set { m_DamageGiven = value; } }
		public DateTime LastDamage { get { return m_LastDamage; } set { m_LastDamage = value; } }
		public bool HasExpired { get { return (DateTime.Now > (m_LastDamage + m_ExpireDelay)); } }
		public ArrayList Responsible { get { return m_Responsible; } set { m_Responsible = value; } }

		private TimeSpan m_ExpireDelay = TimeSpan.FromMinutes(2.0);

		public TimeSpan ExpireDelay
		{
			get { return m_ExpireDelay; }
			set { m_ExpireDelay = value; }
		}

		public DamageEntry(Mobile damager)
		{
			m_Damager = damager;
		}
	}

	public abstract class SkillMod
	{
		private Mobile m_Owner;
		private SkillName m_Skill;
		private bool m_Relative;
		private double m_Value;
		private bool m_ObeyCap;

		public SkillMod(SkillName skill, bool relative, double value)
		{
			m_Skill = skill;
			m_Relative = relative;
			m_Value = value;
		}

		public bool ObeyCap
		{
			get { return m_ObeyCap; }
			set
			{
				m_ObeyCap = value;

				if (m_Owner != null)
				{
					Skill sk = m_Owner.Skills[m_Skill];

					if (sk != null)
						sk.Update();
				}
			}
		}

		public Mobile Owner
		{
			get
			{
				return m_Owner;
			}
			set
			{
				if (m_Owner != value)
				{
					if (m_Owner != null)
						m_Owner.RemoveSkillMod(this);

					m_Owner = value;

					if (m_Owner != value)
						m_Owner.AddSkillMod(this);
				}
			}
		}

		public void Remove()
		{
			Owner = null;
		}

		public SkillName Skill
		{
			get
			{
				return m_Skill;
			}
			set
			{
				if (m_Skill != value)
				{
					Skill oldUpdate = (m_Owner == null ? m_Owner.Skills[m_Skill] : null);

					m_Skill = value;

					if (m_Owner != null)
					{
						Skill sk = m_Owner.Skills[m_Skill];

						if (sk != null)
							sk.Update();
					}

					if (oldUpdate != null)
						oldUpdate.Update();
				}
			}
		}

		public bool Relative
		{
			get
			{
				return m_Relative;
			}
			set
			{
				if (m_Relative != value)
				{
					m_Relative = value;

					if (m_Owner != null)
					{
						Skill sk = m_Owner.Skills[m_Skill];

						if (sk != null)
							sk.Update();
					}
				}
			}
		}

		public bool Absolute
		{
			get
			{
				return !m_Relative;
			}
			set
			{
				if (m_Relative == value)
				{
					m_Relative = !value;

					if (m_Owner != null)
					{
						Skill sk = m_Owner.Skills[m_Skill];

						if (sk != null)
							sk.Update();
					}
				}
			}
		}

		public double Value
		{
			get
			{
				return m_Value;
			}
			set
			{
				if (m_Value != value)
				{
					m_Value = value;

					if (m_Owner != null)
					{
						Skill sk = m_Owner.Skills[m_Skill];

						if (sk != null)
							sk.Update();
					}
				}
			}
		}

		public abstract bool CheckCondition();
	}

	public class ResistanceMod
	{
		private Mobile m_Owner;
		private ResistanceType m_Type;
		private int m_Offset;

		public Mobile Owner
		{
			get { return m_Owner; }
			set { m_Owner = value; }
		}

		public ResistanceType Type
		{
			get { return m_Type; }
			set
			{
				if (m_Type != value)
				{
					m_Type = value;

					//if ( m_Owner != null )
					//m_Owner.UpdateResistances();
				}
			}
		}

		public int Offset
		{
			get { return m_Offset; }
			set
			{
				if (m_Offset != value)
				{
					m_Offset = value;

					//if ( m_Owner != null )
					//m_Owner.UpdateResistances();
				}
			}
		}

		public ResistanceMod(ResistanceType type, int offset)
		{
			m_Type = type;
			m_Offset = offset;
		}
	}

	public class StatMod
	{
		private StatType m_Type;
		private string m_Name;
		private double m_Offset;
		private TimeSpan m_Duration;
		private DateTime m_Added;

		public StatType Type { get { return m_Type; } }
		public string Name { get { return m_Name; } }
		public double Offset { get { return m_Offset; } }

		public bool HasElapsed()
		{
			if (m_Duration == TimeSpan.Zero)
				return false;

			return (DateTime.Now - m_Added) >= m_Duration;
		}

		public StatMod(StatType type, string name, double offset, TimeSpan duration)
		{
			m_Type = type;
			m_Name = name;
			m_Offset = offset;
			m_Duration = duration;
			m_Added = DateTime.Now;
		}
	}

	[CustomEnum(new string[] { "North", "Right", "East", "Down", "South", "Left", "West", "Up" })]
	public enum Direction : byte
	{
		North = 0x0,
		Right = 0x1,
		East = 0x2,
		Down = 0x3,
		South = 0x4,
		Left = 0x5,
		West = 0x6,
		Up = 0x7,
		Mask = 0x7,
		Running = 0x80,
		ValueMask = 0x87
	}

	[Flags]
	public enum MobileDelta
	{
		None = 0x00000000,
		Name = 0x00000001,
		Flags = 0x00000002,
		Hits = 0x00000004,
		Mana = 0x00000008,
		Stam = 0x00000010,
		Stat = 0x00000020,
		Noto = 0x00000040,
		Gold = 0x00000080,
		Weight = 0x00000100,
		Direction = 0x00000200,
		Hue = 0x00000400,
		Body = 0x00000800,
		Armor = 0x00001000,
		StatCap = 0x00002000,
		GhostUpdate = 0x00004000,
		Followers = 0x00008000,
		Properties = 0x00010000,
		TithingPoints = 0x00020000,
		Resistances = 0x00040000,
		WeaponDamage = 0x00080000,
		Hair = 0x00100000,
		FacialHair = 0x00200000,
		Race = 0x00400000,

		Attributes = 0x0000001C
	}

	// Add new Access Levels and prepare old ones for conversion.
	// Adam: when you update this, you MUST also update the names table GetAccessLevelName()
	public enum AccessLevel
	{
		//OldPlayer = 0,
		//OldCounselor = 1,
		//OldGameMaster = 2,
		//OldSeer = 3,
		//OldAdministrator = 4,
		Player = 100,
		Reporter = 115,
		FightBroker = 130,
		Counselor = 145,
		GameMaster = 160,
		Seer = 175,
		Administrator = 205,
		Owner = 220,
		ReadOnly = 255
	}

	public enum VisibleDamageType
	{
		None,
		Related,
		Everyone
	}

	public enum ResistanceType
	{
		Physical,
		Fire,
		Cold,
		Poison,
		Energy
	}

	public class MobileNotConnectedException : Exception
	{
		public MobileNotConnectedException(Mobile source, string message)
			: base(message)
		{
			this.Source = source.ToString();
		}
	}

	public delegate bool SkillCheckTargetHandler(Mobile from, SkillName skill, object target, double minSkill, double maxSkill);
	public delegate bool SkillCheckLocationHandler(Mobile from, SkillName skill, double minSkill, double maxSkill);

	public delegate bool SkillCheckDirectTargetHandler(Mobile from, SkillName skill, object target, double chance);
	public delegate bool SkillCheckDirectLocationHandler(Mobile from, SkillName skill, double chance);

	public delegate TimeSpan RegenRateHandler(Mobile from);

	public delegate bool AllowBeneficialHandler(Mobile from, Mobile target);
	public delegate bool AllowHarmfulHandler(Mobile from, Mobile target);

	public delegate Container CreateCorpseHandler(Mobile from, ArrayList initialContent, ArrayList equipedItems);

	public delegate void StatChangeHandler(Mobile from, StatType stat);

	public enum ApplyPoisonResult
	{
		Poisoned,
		Immune,
		HigherPoisonActive,
		Cured
	}

	/// <summary>
	/// Base class representing players, npcs, and creatures.
	/// </summary>
	public class Mobile : IEntity, IPoint3D, IHued
	{
		#region CompareTo(...)
		public int CompareTo(IEntity other)
		{
			if (other == null)
				return -1;

			return m_Serial.CompareTo(other.Serial);
		}

		public int CompareTo(Mobile other)
		{
			return this.CompareTo((IEntity)other);
		}

		public int CompareTo(object other)
		{
			if (other == null || other is IEntity)
				return this.CompareTo((IEntity)other);

			throw new ArgumentException();
		}
		#endregion

		[Flags]
		public enum MobileFlags
		{
			None = 0x00000000,
			IsTemplate = 0x00000001,	// is this a template mobile? (THIS SHOULD BE CONVERTED TO IsIntMapStorage)
			ToDelete = 0x00000002,	// is this mobile marked for deletion?
			StaffOwned = 0x00000004,	// IS this mobile owned by staff?
			IsIntMapStorage = 0x00000008,	// Adam: Is Internal Map Storage?
		}

		private void SetFlag(MobileFlags flag, bool value)
		{
			if (value)
				m_Flags |= flag;
			else
				m_Flags &= ~flag;
		}

		private bool GetFlag(MobileFlags flag)
		{
			return ((m_Flags & flag) != 0);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool StaffOwned
		{
			get { return GetFlag(MobileFlags.StaffOwned); }
			set { SetFlag(MobileFlags.StaffOwned, value); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToDelete
		{
			get { return GetFlag(MobileFlags.ToDelete); }
			set { SetFlag(MobileFlags.ToDelete, value); InvalidateProperties(); }
		}

		private static bool m_DragEffects = true;

		public static bool DragEffects
		{
			get { return m_DragEffects; }
			set { m_DragEffects = value; }
		}

		private static AllowBeneficialHandler m_AllowBeneficialHandler;
		private static AllowHarmfulHandler m_AllowHarmfulHandler;

		public static AllowBeneficialHandler AllowBeneficialHandler
		{
			get { return m_AllowBeneficialHandler; }
			set { m_AllowBeneficialHandler = value; }
		}

		public static AllowHarmfulHandler AllowHarmfulHandler
		{
			get { return m_AllowHarmfulHandler; }
			set { m_AllowHarmfulHandler = value; }
		}

		public virtual TimeSpan HitsRegenRate
		{
			get
			{
				return TimeSpan.FromSeconds(11.0);
			}
		}

		public virtual TimeSpan ManaRegenRate
		{
			get
			{
				if (m_ManaRegenRate != null)
					return m_ManaRegenRate(this);

				else
					return TimeSpan.FromSeconds(7.0);
			}
		}

		public virtual TimeSpan StamRegenRate
		{
			get
			{
				return TimeSpan.FromSeconds(7.0);
			}
		}

		private static RegenRateHandler m_ManaRegenRate;/*, m_HitsRegenRate, m_StamRegenRate;
		private static TimeSpan m_DefaultHitsRate, m_DefaultStamRate, m_DefaultManaRate;

		public static RegenRateHandler HitsRegenRateHandler
		{
			get{ return m_HitsRegenRate; }
			set{ m_HitsRegenRate = value; }
		}

		public static TimeSpan DefaultHitsRate
		{
			get{ return m_DefaultHitsRate; }
			set{ m_DefaultHitsRate = value; }
		}

		public static RegenRateHandler StamRegenRateHandler
		{
			get{ return m_StamRegenRate; }
			set{ m_StamRegenRate = value; }
		}

		public static TimeSpan DefaultStamRate
		{
			get{ return m_DefaultStamRate; }
			set{ m_DefaultStamRate = value; }
		}*/

		public static RegenRateHandler ManaRegenRateHandler
		{
			get { return m_ManaRegenRate; }
			set { m_ManaRegenRate = value; }
		}/*

		public static TimeSpan DefaultManaRate
		{
			get{ return m_DefaultManaRate; }
			set{ m_DefaultManaRate = value; }
		}

		public static TimeSpan GetHitsRegenRate( Mobile m )
		{
			if ( m_HitsRegenRate == null )
				return m_DefaultHitsRate;
			else
				return m_HitsRegenRate( m );
		}

		public static TimeSpan GetStamRegenRate( Mobile m )
		{
			if ( m_StamRegenRate == null )
				return m_DefaultStamRate;
			else
				return m_StamRegenRate( m );
		}

		public static TimeSpan GetManaRegenRate( Mobile m )
		{
			if ( m_ManaRegenRate == null )
				return m_DefaultManaRate;
			else
				return m_ManaRegenRate( m );
		}*/

		private class MovementRecord
		{
			public DateTime m_End;

			private static Queue m_InstancePool = new Queue();

			public static MovementRecord NewInstance(DateTime end)
			{
				MovementRecord r;

				if (m_InstancePool.Count > 0)
				{
					r = (MovementRecord)m_InstancePool.Dequeue();

					r.m_End = end;
				}
				else
				{
					r = new MovementRecord(end);
				}

				return r;
			}

			private MovementRecord(DateTime end)
			{
				m_End = end;
			}

			public bool Expired()
			{
				bool v = (DateTime.Now >= m_End);

				if (v)
					m_InstancePool.Enqueue(this);

				return v;
			}
		}

		public event StatChangeHandler StatChange;

		private Serial m_Serial;
		private Map m_Map;
		private Point3D m_Location;
		private Direction m_Direction;
		private Body m_Body;
		private int m_Hue;
		private Poison m_Poison;
		private Timer m_PoisonTimer;
		private BaseGuild m_Guild;
		private string m_GuildTitle;
		private bool m_Criminal;
		private string m_Name;
		private int m_Kills, m_ShortTermMurders;
		private int m_SpeechHue, m_EmoteHue, m_WhisperHue, m_YellHue;
		private string m_Language;
		private NetState m_NetState;
		private bool m_Female, m_Warmode, m_Hidden, m_Blessed;
		private int m_StatCap;
		private int m_STRBonusCap;
		private int m_Str, m_Dex, m_Int;
		private int m_Hits, m_Stam, m_Mana;
		private int m_Fame, m_Karma;
		private AccessLevel m_AccessLevel = AccessLevel.Player; //hard code to New player level!
		private Skills m_Skills;
		private ArrayList m_Items;
		private bool m_Player;
		private string m_Title;
		private string m_Profile;
		private bool m_ProfileLocked;
		private int m_LightLevel;
		private int m_TotalGold, m_TotalWeight;
		private ArrayList m_StatMods;
		private ISpell m_Spell;
		private Target m_Target;
		private Prompt m_Prompt;
		private ContextMenu m_ContextMenu;
		private ArrayList m_Aggressors, m_Aggressed;
		private Mobile m_Combatant;
		private ArrayList m_Stabled;
		private bool m_AutoPageNotify;
		private bool m_Meditating;
		private bool m_CanHearGhosts;
		private bool m_CanSwim, m_CantWalk, m_CanFly;
		private MobileFlags m_Flags;
		private bool m_DisplayGuildTitle;
		private Mobile m_GuildFealty;
		private DateTime m_NextSpellTime;
		private DateTime[] m_StuckMenuUses;
		private Timer m_ExpireCombatant;
		private Timer m_ExpireCriminal;
		private Timer m_ExpireAggrTimer;
		private Timer m_LogoutTimer;
		private Timer m_CombatTimer;
		private Timer m_ManaTimer, m_HitsTimer, m_StamTimer;
		private DateTime m_NextSkillTime;
		private DateTime m_NextActionTime; // Use, pickup, etc
		private DateTime m_NextActionMessage;
		private bool m_Paralyzed;
		private ParalyzedTimer m_ParaTimer;
		private bool m_Frozen;
		private FrozenTimer m_FrozenTimer;
		private int m_AllowedStealthSteps;
		private int m_Hunger;
		private int m_NameHue = -1;
		private Region m_Region;
		private bool m_DisarmReady, m_StunReady;
		private int m_BaseSoundID;
		private int m_VirtualArmor;
		private bool m_Squelched;
		private int m_MeleeDamageAbsorb;
		private int m_MagicDamageAbsorb;
		private int m_Followers, m_FollowersMax;
		private ArrayList m_Actions;
		private Queue<MovementRecord> m_MoveRecords;
		private int m_WarmodeChanges = 0;
		private DateTime m_NextWarmodeChange;
		private WarmodeTimer m_WarmodeTimer;
		private int m_Thirst, m_BAC;
		private int m_VirtualArmorMod;
		private VirtueInfo m_Virtues;
		private object m_Party;
		private ArrayList m_SkillMods = new ArrayList(1);
		private Body m_BodyMod;
		private DateTime m_LastStatGain;

		private bool m_HasAbilityReady;
		private DateTime m_NextAbilityTime;

		private int[] FlyIDs = new int[] { };

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextAbilityTime
		{
			get { return m_NextAbilityTime; }
			set { m_NextAbilityTime = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasAbilityReady
		{
			get { return m_HasAbilityReady; }
			set { m_HasAbilityReady = value; }
		}

		//array accessor for setting/retriveing fly tiles
		public int[] FlyArray
		{
			get { return FlyIDs; }
			set { FlyIDs = value; }
		}

		//virtual outfit and body init routines
		public virtual void InitOutfit()
		{
		}

		//name/sex
		public virtual void InitBody()
		{
		}

		public virtual void WipeLayers()
		{
			try
			{
				Item[] items = new Item[21];
				items[0] = this.FindItemOnLayer(Layer.Shoes);
				items[1] = this.FindItemOnLayer(Layer.Pants);
				items[2] = this.FindItemOnLayer(Layer.Shirt);
				items[3] = this.FindItemOnLayer(Layer.Helm);
				items[4] = this.FindItemOnLayer(Layer.Gloves);
				items[5] = this.FindItemOnLayer(Layer.Neck);
				items[6] = this.FindItemOnLayer(Layer.Waist);
				items[7] = this.FindItemOnLayer(Layer.InnerTorso);
				items[8] = this.FindItemOnLayer(Layer.MiddleTorso);
				items[9] = this.FindItemOnLayer(Layer.Arms);
				items[10] = this.FindItemOnLayer(Layer.Cloak);
				items[11] = this.FindItemOnLayer(Layer.OuterTorso);
				items[12] = this.FindItemOnLayer(Layer.OuterLegs);
				items[13] = this.FindItemOnLayer(Layer.InnerLegs);
				items[14] = this.FindItemOnLayer(Layer.Bracelet);
				items[15] = this.FindItemOnLayer(Layer.Ring);
				items[16] = this.FindItemOnLayer(Layer.Earrings);
				items[17] = this.FindItemOnLayer(Layer.OneHanded);
				items[18] = this.FindItemOnLayer(Layer.TwoHanded);
				items[19] = this.FindItemOnLayer(Layer.Hair);
				items[20] = this.FindItemOnLayer(Layer.FacialHair);
				for (int i = 0; i < items.Length; i++)
				{
					if (items[i] != null)
					{
						items[i].Delete();
					}
				}
			}
			catch (Exception exc)
			{
				System.Console.WriteLine("Send to Zen please: ");
				System.Console.WriteLine("Exception caught in Mobile.WipeLayers: " + exc.Message);
				System.Console.WriteLine(exc.StackTrace);
			}
		}

		private static TimeSpan WarmodeSpamCatch = TimeSpan.FromSeconds(0.5);
		private static TimeSpan WarmodeSpamDelay = TimeSpan.FromSeconds(2.0);
		private const int WarmodeCatchCount = 4; // Allow four warmode changes in 0.5 seconds, any more will be delay for two seconds


		//private ArrayList m_ResistMods;

		//private int[] m_Resistances;

		//public int[] Resistances{ get{ return m_Resistances; } }

		//public virtual int BasePhysicalResistance{ get{ return 0; } }
		//public virtual int BaseFireResistance{ get{ return 0; } }
		//public virtual int BaseColdResistance{ get{ return 0; } }
		//public virtual int BasePoisonResistance{ get{ return 0; } }
		//public virtual int BaseEnergyResistance{ get{ return 0; } }

		public virtual void ComputeLightLevels(out int global, out int personal)
		{
			ComputeBaseLightLevels(out global, out personal);

			if (m_Region != null)
				m_Region.AlterLightLevel(this, ref global, ref personal);
		}

		public virtual void ComputeBaseLightLevels(out int global, out int personal)
		{
			global = 0;
			personal = m_LightLevel;
		}

		public virtual void CheckLightLevels(bool forceResend)
		{
		}
		/*
				[CommandProperty( AccessLevel.Counselor )]
				public virtual int PhysicalResistance
				{
					get{ return GetResistance( ResistanceType.Physical ); }
				}

				[CommandProperty( AccessLevel.Counselor )]
				public virtual int FireResistance
				{
					get{ return GetResistance( ResistanceType.Fire ); }
				}

				[CommandProperty( AccessLevel.Counselor )]
				public virtual int ColdResistance
				{
					get{ return GetResistance( ResistanceType.Cold ); }
				}

				[CommandProperty( AccessLevel.Counselor )]
				public virtual int PoisonResistance
				{
					get{ return GetResistance( ResistanceType.Poison ); }
				}

				[CommandProperty( AccessLevel.Counselor )]
				public virtual int EnergyResistance
				{
					get{ return GetResistance( ResistanceType.Energy ); }
				}
		*/
		/*
		public virtual void UpdateResistances()
		{
			if ( m_Resistances == null )
				m_Resistances = new int[5]{ int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue };

			bool delta = false;

			for ( int i = 0; i < m_Resistances.Length; ++i )
			{
				if ( m_Resistances[i] != int.MinValue )
				{
					m_Resistances[i] = int.MinValue;
					delta = true;
				}
			}

			if ( delta )
				Delta( MobileDelta.Resistances );
		}
*/
		/*
				public virtual int GetResistance( ResistanceType type )
				{
					if ( m_Resistances == null )
						m_Resistances = new int[5]{ int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue };

					int v = (int)type;

					if ( v < 0 || v >= m_Resistances.Length )
						return 0;

					int res = m_Resistances[v];

					if ( res == int.MinValue )
					{
						ComputeResistances();
						res = m_Resistances[v];
					}

					return res;
				}
		*/
		/*
		public ArrayList ResistanceMods
		{
			get{ return m_ResistMods; }
			set{ m_ResistMods = value; }
		}*/
		/*
				public virtual void AddResistanceMod( ResistanceMod toAdd )
				{
					if ( m_ResistMods == null )
						m_ResistMods = new ArrayList( 2 );

					m_ResistMods.Add( toAdd );
					UpdateResistances();
				}

				public virtual void RemoveResistanceMod( ResistanceMod toRemove )
				{
					if ( m_ResistMods != null )
					{
						m_ResistMods.Remove( toRemove );

						if ( m_ResistMods.Count == 0 )
							m_ResistMods = null;
					}

					UpdateResistances();
				}
		*/
		private static int m_MaxPlayerResistance = 70;

		public static int MaxPlayerResistance { get { return m_MaxPlayerResistance; } set { m_MaxPlayerResistance = value; } }
		/*
				public virtual void ComputeResistances()
				{
					if ( m_Resistances == null )
						m_Resistances = new int[5]{ int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue };

					for ( int i = 0; i < m_Resistances.Length; ++i )
						m_Resistances[i] = 0;

					m_Resistances[0] += this.BasePhysicalResistance;
					m_Resistances[1] += this.BaseFireResistance;
					m_Resistances[2] += this.BaseColdResistance;
					m_Resistances[3] += this.BasePoisonResistance;
					m_Resistances[4] += this.BaseEnergyResistance;

					for ( int i = 0; m_ResistMods != null && i < m_ResistMods.Count; ++i )
					{
						ResistanceMod mod = (ResistanceMod)m_ResistMods[i];
						int v = (int)mod.Type;

						if ( v >= 0 && v < m_Resistances.Length )
							m_Resistances[v] += mod.Offset;
					}

					for ( int i = 0; i < m_Items.Count; ++i )
					{
						Item item = (Item)m_Items[i];

						if ( item.CheckPropertyConfliction( this ) )
							continue;

						m_Resistances[0] += item.PhysicalResistance;
						m_Resistances[1] += item.FireResistance;
						m_Resistances[2] += item.ColdResistance;
						m_Resistances[3] += item.PoisonResistance;
						m_Resistances[4] += item.EnergyResistance;
					}

					for ( int i = 0; i < m_Resistances.Length; ++i )
					{
						int min = GetMinResistance( (ResistanceType)i );
						int max = GetMaxResistance( (ResistanceType)i );

						if ( max < min )
							max = min;

						if ( m_Resistances[i] > max )
							m_Resistances[i] = max;
						else if ( m_Resistances[i] < min )
							m_Resistances[i] = min;
					}
				}
		*/
		public virtual int GetMinResistance(ResistanceType type)
		{
			return int.MinValue;
		}

		public virtual int GetMaxResistance(ResistanceType type)
		{
			if (m_Player)
				return m_MaxPlayerResistance;

			return int.MaxValue;
		}

		public virtual void SendPropertiesTo(Mobile from)
		{
			from.Send(PropertyList);
		}

		public virtual void OnAosSingleClick(Mobile from)
		{
			ObjectPropertyList opl = this.PropertyList;

			if (opl.Header > 0)
			{
				int hue;

				if (m_NameHue != -1)
					hue = m_NameHue;
				else if (m_AccessLevel > AccessLevel.Player)
					hue = 11;
				else
					hue = Notoriety.GetHue(Notoriety.Compute(from, this));

				from.Send(new MessageLocalized(m_Serial, Body, MessageType.Label, hue, 3, opl.Header, Name, opl.HeaderArgs));
			}
		}

		public virtual string ApplyNameSuffix(string suffix)
		{
			return suffix;
		}

		public virtual void AddNameProperties(ObjectPropertyList list)
		{
			string name = Name;

			if (name == null)
				name = String.Empty;

			string prefix = "";

			if (ShowFameTitle && (m_Player || m_Body.IsHuman) && m_Fame >= 10000)
				prefix = m_Female ? "Lady" : "Lord";

			string suffix = "";

			if (ClickTitle && Title != null && Title.Length > 0)
				suffix = Title;

			BaseGuild guild = m_Guild;

			if (guild != null && (m_Player || m_DisplayGuildTitle))
			{
				if (suffix.Length > 0)
					suffix = String.Format("{0} [{1}]", suffix, Utility.FixHtml(guild.Abbreviation));
				else
					suffix = String.Format("[{0}]", Utility.FixHtml(guild.Abbreviation));
			}

			suffix = ApplyNameSuffix(suffix);

			list.Add(1050045, "{0} \t{1}\t {2}", prefix, name, suffix); // ~1_PREFIX~~2_NAME~~3_SUFFIX~

			if (guild != null && (m_DisplayGuildTitle || (m_Player && guild.Type != GuildType.Regular)))
			{
				string type;

				if (guild.Type >= 0 && (int)guild.Type < m_GuildTypes.Length)
					type = m_GuildTypes[(int)guild.Type];
				else
					type = "";

				string title = GuildTitle;

				if (title == null)
					title = "";
				else
					title = title.Trim();

				if (title.Length > 0)
					list.Add("{0}, {1} Guild{2}", Utility.FixHtml(title), Utility.FixHtml(guild.Name), type);
				else
					list.Add(Utility.FixHtml(guild.Name));
			}
		}

		public virtual void GetProperties(ObjectPropertyList list)
		{
			AddNameProperties(list);
		}

		public virtual void GetChildProperties(ObjectPropertyList list, Item item)
		{
		}

		public virtual void GetChildNameProperties(ObjectPropertyList list, Item item)
		{
		}

		private void UpdateAggrExpire()
		{
			if (m_Deleted || (m_Aggressors.Count == 0 && m_Aggressed.Count == 0))
			{
				StopAggrExpire();
			}
			else if (m_ExpireAggrTimer == null)
			{
				m_ExpireAggrTimer = new ExpireAggressorsTimer(this);
				m_ExpireAggrTimer.Start();
			}
		}

		private void StopAggrExpire()
		{
			if (m_ExpireAggrTimer != null)
				m_ExpireAggrTimer.Stop();

			m_ExpireAggrTimer = null;
		}

		private void CheckAggrExpire()
		{
			for (int i = m_Aggressors.Count - 1; i >= 0; --i)
			{
				if (i >= m_Aggressors.Count)
					continue;

				AggressorInfo info = (AggressorInfo)m_Aggressors[i];

				if (info.Expired)
				{
					Mobile attacker = info.Attacker;
					attacker.RemoveAggressed(this);

					m_Aggressors.RemoveAt(i);
					info.Free();

					if (m_NetState != null && this.CanSee(attacker) && Utility.InUpdateRange(m_Location, attacker.m_Location))
						m_NetState.Send(new MobileIncoming(this, attacker));
				}
			}

			for (int i = m_Aggressed.Count - 1; i >= 0; --i)
			{
				if (i >= m_Aggressed.Count)
					continue;

				AggressorInfo info = (AggressorInfo)m_Aggressed[i];

				if (info.Expired)
				{
					Mobile defender = info.Defender;
					defender.RemoveAggressor(this);

					m_Aggressed.RemoveAt(i);
					info.Free();

					if (m_NetState != null && this.CanSee(defender) && Utility.InUpdateRange(m_Location, defender.m_Location))
						m_NetState.Send(new MobileIncoming(this, defender));
				}
			}

			UpdateAggrExpire();
		}

		public ArrayList Stabled { get { return m_Stabled; } set { m_Stabled = value; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public VirtueInfo Virtues { get { return m_Virtues; } set { } }

		public object Party { get { return m_Party; } set { m_Party = value; } }
		public ArrayList SkillMods { get { return m_SkillMods; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int VirtualArmorMod
		{
			get
			{
				return m_VirtualArmorMod;
			}
			set
			{
				if (m_VirtualArmorMod != value)
				{
					m_VirtualArmorMod = value;

					Delta(MobileDelta.Armor);
				}
			}
		}

		/// <summary>
		/// Overridable. Virtual event invoked when <paramref name="skill" /> changes in some way.
		/// </summary>
		public virtual void OnSkillInvalidated(Skill skill)
		{
		}

		public virtual void UpdateSkillMods()
		{
			ValidateSkillMods();

			for (int i = 0; i < m_SkillMods.Count; ++i)
			{
				SkillMod mod = (SkillMod)m_SkillMods[i];

				Skill sk = m_Skills[mod.Skill];

				if (sk != null)
					sk.Update();
			}
		}

		public virtual void ValidateSkillMods()
		{
			for (int i = 0; i < m_SkillMods.Count; )
			{
				SkillMod mod = (SkillMod)m_SkillMods[i];

				if (mod.CheckCondition())
					++i;
				else
					InternalRemoveSkillMod(mod);
			}
		}

		public virtual void AddSkillMod(SkillMod mod)
		{
			if (mod == null)
				return;

			ValidateSkillMods();

			if (!m_SkillMods.Contains(mod))
			{
				m_SkillMods.Add(mod);
				mod.Owner = this;

				Skill sk = m_Skills[mod.Skill];

				if (sk != null)
					sk.Update();
			}
		}

		public virtual void RemoveSkillMod(SkillMod mod)
		{
			if (mod == null)
				return;

			ValidateSkillMods();

			InternalRemoveSkillMod(mod);
		}

		private void InternalRemoveSkillMod(SkillMod mod)
		{
			if (m_SkillMods.Contains(mod))
			{
				m_SkillMods.Remove(mod);
				mod.Owner = null;

				Skill sk = m_Skills[mod.Skill];

				if (sk != null)
					sk.Update();
			}
		}

		private class WarmodeTimer : Timer
		{
			private Mobile m_Mobile;
			private bool m_Value;

			public bool Value
			{
				get
				{
					return m_Value;
				}
				set
				{
					m_Value = value;
				}
			}

			public WarmodeTimer(Mobile m, bool value)
				: base(WarmodeSpamDelay)
			{
				m_Mobile = m;
				m_Value = value;
			}

			protected override void OnTick()
			{
				m_Mobile.Warmode = m_Value;
				m_Mobile.m_WarmodeChanges = 0;

				m_Mobile.m_WarmodeTimer = null;
			}
		}

		/// <summary>
		/// Overridable. Virtual event invoked when a client, <paramref name="from" />, invokes a 'help request' for the Mobile. Seemingly no longer functional in newer clients.
		/// </summary>
		public virtual void OnHelpRequest(Mobile from)
		{
		}

		public void DelayChangeWarmode(bool value)
		{
			if (m_WarmodeTimer != null)
			{
				m_WarmodeTimer.Value = value;
				return;
			}

			if (m_Warmode == value)
				return;

			DateTime now = DateTime.Now, next = m_NextWarmodeChange;

			if (now > next || m_WarmodeChanges == 0)
			{
				m_WarmodeChanges = 1;
				m_NextWarmodeChange = now + WarmodeSpamCatch;
			}
			else if (m_WarmodeChanges == WarmodeCatchCount)
			{
				m_WarmodeTimer = new WarmodeTimer(this, value);
				m_WarmodeTimer.Start();

				return;
			}
			else
			{
				++m_WarmodeChanges;
			}

			Warmode = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MeleeDamageAbsorb
		{
			get
			{
				return m_MeleeDamageAbsorb;
			}
			set
			{
				m_MeleeDamageAbsorb = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MagicDamageAbsorb
		{
			get
			{
				return m_MagicDamageAbsorb;
			}
			set
			{
				m_MagicDamageAbsorb = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SkillsTotal
		{
			get
			{
				return m_Skills == null ? 0 : m_Skills.Total;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SkillsCap
		{
			get
			{
				return m_Skills == null ? 0 : m_Skills.Cap;
			}
			set
			{
				if (m_Skills != null)
					m_Skills.Cap = value;
			}
		}

		public bool InLOS(Mobile target)
		{
			if (m_Deleted || m_Map == null)
				return false;
			else if (target == this || m_AccessLevel > AccessLevel.Player)
				return true;

			return m_Map.LineOfSight(this, target);
		}

		// wea: new check to see if target is audible to another
		public bool IsAudibleTo(Mobile target)
		{
			if (m_Deleted || m_Map == null)
				return false;
			else if (target == this || m_AccessLevel > AccessLevel.Player)
				return true;

			return m_Map.LineOfSight(this, target, true);
		}

		public bool InLOS(object target)
		{
			if (m_Deleted || m_Map == null)
				return false;
			else if (target == this || m_AccessLevel > AccessLevel.Player)
				return true;
			else if (target is Item && ((Item)target).RootParent == this)
				return true;

			return m_Map.LineOfSight(this, target);
		}

		public bool InLOS(Point3D target)
		{
			if (m_Deleted || m_Map == null)
				return false;
			else if (m_AccessLevel > AccessLevel.Player)
				return true;

			return m_Map.LineOfSight(this, target);
		}

		public bool SpawnerTempMob
		{
			get { return GetFlag(MobileFlags.IsTemplate); }
			set { SetFlag(MobileFlags.IsTemplate, value); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsIntMapStorage
		{
			get { return GetFlag(MobileFlags.IsIntMapStorage); }
			set { SetFlag(MobileFlags.IsIntMapStorage, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int BaseSoundID
		{
			get
			{
				return m_BaseSoundID;
			}
			set
			{
				m_BaseSoundID = value;
			}
		}

		public DateTime NextCombatTime
		{
			get
			{
				return m_NextCombatTime;
			}
			set
			{
				m_NextCombatTime = value;
			}
		}

		public bool BeginAction(object toLock)
		{
			if (m_Actions == null)
			{
				m_Actions = new ArrayList(2);

				m_Actions.Add(toLock);

				return true;
			}
			else if (!m_Actions.Contains(toLock))
			{
				m_Actions.Add(toLock);

				return true;
			}

			return false;
		}

		public bool CanBeginAction(object toLock)
		{
			return (m_Actions == null || !m_Actions.Contains(toLock));
		}

		public void EndAction(object toLock)
		{
			if (m_Actions != null)
			{
				m_Actions.Remove(toLock);

				if (m_Actions.Count == 0)
					m_Actions = null;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int NameHue
		{
			get
			{
				return m_NameHue;
			}
			set
			{
				m_NameHue = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Hunger
		{
			get
			{
				return m_Hunger;
			}
			set
			{
				int oldValue = m_Hunger;

				if (oldValue != value)
				{
					m_Hunger = value;

					EventSink.InvokeHungerChanged(new HungerChangedEventArgs(this, oldValue));
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Thirst
		{
			get
			{
				return m_Thirst;
			}
			set
			{
				m_Thirst = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int BAC
		{
			get
			{
				return m_BAC;
			}
			set
			{
				m_BAC = value;
			}
		}

		private DateTime m_LastMoveTime;

		/// <summary>
		/// Gets or sets the number of steps this player may take when hidden before being revealed.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int AllowedStealthSteps
		{
			get
			{
				return m_AllowedStealthSteps;
			}
			set
			{
				m_AllowedStealthSteps = value;
			}
		}

		/* Logout:
		 * 
		 * When a client logs into mobile x
		 *  - if ( x is Internalized ) move x to logout location and map
		 * 
		 * When a client attached to a mobile disconnects
		 *  - LogoutTimer is started
		 *	   - Delay is taken from Region.GetLogoutDelay to allow insta-logout regions.
		 *     - OnTick : Location and map are stored, and mobile is internalized
		 * 
		 * Some things to consider:
		 *  - An internalized person getting killed (say, by poison). Where does the body go?
		 *  - Regions now have a GetLogoutDelay( Mobile m ); virtual function (see above)
		 */
		private Point3D m_LogoutLocation;
		private Map m_LogoutMap;

		public virtual TimeSpan GetLogoutDelay()
		{
			return Region.GetLogoutDelay(this);
		}

		private StatLockType m_StrLock, m_DexLock, m_IntLock;

		private Item m_Holding;

		public Item Holding
		{
			get
			{
				return m_Holding;
			}
			set
			{
				if (m_Holding != value)
				{
					if (m_Holding != null)
					{
						TotalWeight -= m_Holding.TotalWeight + m_Holding.PileWeight;

						if (m_Holding.HeldBy == this)
							m_Holding.HeldBy = null;
					}

					if (value != null && m_Holding != null)
						DropHolding();

					m_Holding = value;

					if (m_Holding != null)
					{
						TotalWeight += m_Holding.TotalWeight + m_Holding.PileWeight;

						if (m_Holding.HeldBy == null)
							m_Holding.HeldBy = this;
					}
				}
			}
		}

		public DateTime LastMoveTime
		{
			get
			{
				return m_LastMoveTime;
			}
			set
			{
				m_LastMoveTime = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Paralyzed
		{
			get
			{
				return m_Paralyzed;
			}
			set
			{
				if (m_Paralyzed != value)
				{
					m_Paralyzed = value;

					this.SendLocalizedMessage(m_Paralyzed ? 502381 : 502382);

					if (m_ParaTimer != null)
					{
						m_ParaTimer.Stop();
						m_ParaTimer = null;
					}
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DisarmReady
		{
			get
			{
				return m_DisarmReady;
			}
			set
			{
				m_DisarmReady = value;
				//SendLocalizedMessage( value ? 1019013 : 1019014 );
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool StunReady
		{
			get
			{
				return m_StunReady;
			}
			set
			{
				m_StunReady = value;
				//SendLocalizedMessage( value ? 1019011 : 1019012 );
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Frozen
		{
			get
			{
				return m_Frozen;
			}
			set
			{
				if (m_Frozen != value)
				{
					m_Frozen = value;

					if (m_FrozenTimer != null)
					{
						m_FrozenTimer.Stop();
						m_FrozenTimer = null;
					}
				}
			}
		}

		public virtual void Paralyze(TimeSpan duration)
		{
			if (!m_Paralyzed)
			{
				Paralyzed = true;

				m_ParaTimer = new ParalyzedTimer(this, duration);
				m_ParaTimer.Start();
			}
		}

		public void Freeze(TimeSpan duration)
		{
			if (!m_Frozen)
			{
				m_Frozen = true;

				m_FrozenTimer = new FrozenTimer(this, duration);
				m_FrozenTimer.Start();
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="StatLockType">lock state</see> for the <see cref="RawStr" /> property.
		/// </summary>
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public StatLockType StrLock
		{
			get
			{
				return m_StrLock;
			}
			set
			{
				if (m_StrLock != value)
				{
					m_StrLock = value;

					if (m_NetState != null)
						m_NetState.Send(new StatLockInfo(this));
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="StatLockType">lock state</see> for the <see cref="RawDex" /> property.
		/// </summary>
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public StatLockType DexLock
		{
			get
			{
				return m_DexLock;
			}
			set
			{
				if (m_DexLock != value)
				{
					m_DexLock = value;

					if (m_NetState != null)
						m_NetState.Send(new StatLockInfo(this));
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="StatLockType">lock state</see> for the <see cref="RawInt" /> property.
		/// </summary>
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public StatLockType IntLock
		{
			get
			{
				return m_IntLock;
			}
			set
			{
				if (m_IntLock != value)
				{
					m_IntLock = value;

					if (m_NetState != null)
						m_NetState.Send(new StatLockInfo(this));
				}
			}
		}

		public override string ToString()
		{
			return String.Format("0x{0:X} \"{1}\"", m_Serial.Value, Name);
		}

		public DateTime NextActionTime
		{
			get
			{
				return m_NextActionTime;
			}
			set
			{
				m_NextActionTime = value;
			}
		}

		public DateTime NextActionMessage
		{
			get
			{
				return m_NextActionMessage;
			}
			set
			{
				m_NextActionMessage = value;
			}
		}

		private static TimeSpan m_ActionMessageDelay = TimeSpan.FromSeconds(0.125);

		public static TimeSpan ActionMessageDelay
		{
			get { return m_ActionMessageDelay; }
			set { m_ActionMessageDelay = value; }
		}

		public virtual void SendSkillMessage()
		{
			if (DateTime.Now < m_NextActionMessage)
				return;

			m_NextActionMessage = DateTime.Now + m_ActionMessageDelay;

			SendLocalizedMessage(500118); // You must wait a few moments to use another skill.
		}

		public virtual void SendActionMessage()
		{
			if (DateTime.Now < m_NextActionMessage)
				return;

			m_NextActionMessage = DateTime.Now + m_ActionMessageDelay;

			SendLocalizedMessage(500119); // You must wait to perform another action.
		}

		public virtual void ClearHands()
		{
			ClearHand(FindItemOnLayer(Layer.OneHanded));
			ClearHand(FindItemOnLayer(Layer.TwoHanded));
		}

		public bool MoveMobileToIntStorage()
		{
			return MoveMobileToIntStorage(false);
		}

		public bool MoveMobileToIntStorage(bool PreserveLocation)
		{
			if (Map == null)
				return false;

			IsIntMapStorage = true;
			if (PreserveLocation == true)
				MoveToWorld(Location, Map.Internal);
			else
				Internalize();
			return true;
		}

		public bool RetrieveMobileFromIntStorage(Point3D p, Map m)
		{
			if (Deleted == true || p == Point3D.Zero)
				return false;

			IsIntMapStorage = false;
			MoveToWorld(p, m);
			return true;
		}

		public virtual void ClearHand(Item item)
		{
			if (item != null && item.Movable && !item.AllowEquipedCast(this))
			{
				Container pack = this.Backpack;

				if (pack == null)
					AddToBackpack(item);
				else
					pack.DropItem(item);
			}
		}

		private static bool m_GlobalRegenThroughPoison = true;

		public static bool GlobalRegenThroughPoison
		{
			get { return m_GlobalRegenThroughPoison; }
			set { m_GlobalRegenThroughPoison = value; }
		}

		public virtual bool RegenThroughPoison { get { return m_GlobalRegenThroughPoison; } }

		public virtual bool CanRegenHits { get { return this.Alive && (RegenThroughPoison || !this.Poisoned); } }
		public virtual bool CanRegenStam { get { return this.Alive; } }
		public virtual bool CanRegenMana { get { return this.Alive; } }

		private class ManaTimer : Timer
		{
			private Mobile m_Owner;

			public ManaTimer(Mobile m)
				: base(m.ManaRegenRate, m.ManaRegenRate)
			{
				this.Priority = TimerPriority.FiftyMS;
				m_Owner = m;
			}

			protected override void OnTick()
			{
				if (m_Owner.CanRegenMana)// m_Owner.Alive )
					m_Owner.Mana++;

				Delay = Interval = m_Owner.ManaRegenRate;
			}
		}

		private class HitsTimer : Timer
		{
			private Mobile m_Owner;

			public HitsTimer(Mobile m)
				: base(m.HitsRegenRate, m.HitsRegenRate)
			{
				this.Priority = TimerPriority.FiftyMS;
				m_Owner = m;
			}

			protected override void OnTick()
			{
				if (m_Owner.CanRegenHits)// m_Owner.Alive && !m_Owner.Poisoned )
					m_Owner.Hits++;

				Delay = Interval = m_Owner.HitsRegenRate;
			}
		}

		private class StamTimer : Timer
		{
			private Mobile m_Owner;

			public StamTimer(Mobile m)
				: base(m.StamRegenRate, m.StamRegenRate)
			{
				this.Priority = TimerPriority.FiftyMS;
				m_Owner = m;
			}

			protected override void OnTick()
			{
				if (m_Owner.CanRegenStam)// m_Owner.Alive )
					m_Owner.Stam++;

				Delay = Interval = m_Owner.StamRegenRate;
			}
		}

		private class LogoutTimer : Timer
		{
			private Mobile m_Mobile;

			public LogoutTimer(Mobile m)
				: base(TimeSpan.FromDays(1.0))
			{
				Priority = TimerPriority.OneSecond;
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				if (m_Mobile.m_Map != Map.Internal)
				{
					EventSink.InvokeLogout(new LogoutEventArgs(m_Mobile));

					m_Mobile.m_LogoutLocation = m_Mobile.m_Location;
					m_Mobile.m_LogoutMap = m_Mobile.m_Map;

					m_Mobile.Internalize();
				}
			}
		}

		private class ParalyzedTimer : Timer
		{
			private Mobile m_Mobile;

			public ParalyzedTimer(Mobile m, TimeSpan duration)
				: base(duration)
			{
				this.Priority = TimerPriority.TwentyFiveMS;
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				m_Mobile.Paralyzed = false;
			}
		}

		private class FrozenTimer : Timer
		{
			private Mobile m_Mobile;

			public FrozenTimer(Mobile m, TimeSpan duration)
				: base(duration)
			{
				this.Priority = TimerPriority.TwentyFiveMS;
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				m_Mobile.Frozen = false;
			}
		}

		private class CombatTimer : Timer
		{
			private Mobile m_Mobile;

			public CombatTimer(Mobile m)
				: base(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.01), 0)
			{
				m_Mobile = m;

				if (!m_Mobile.m_Player && m_Mobile.m_Dex <= 100)
					Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				if (DateTime.Now > m_Mobile.m_NextCombatTime)
				{
					Mobile combatant = m_Mobile.Combatant;

					// If no combatant, wrong map, one of us is a ghost, or cannot see, or deleted, then stop combat
					if (combatant == null || combatant.m_Deleted || m_Mobile.m_Deleted || combatant.m_Map != m_Mobile.m_Map || !combatant.Alive || !m_Mobile.Alive || !m_Mobile.CanSee(combatant) || combatant.IsDeadBondedPet || m_Mobile.IsDeadBondedPet)
					{
						m_Mobile.Combatant = null;
						return;
					}

					IWeapon weapon = m_Mobile.Weapon;

					if (!m_Mobile.InRange(combatant, weapon.MaxRange))
						return;

					if (m_Mobile.InLOS(combatant))
					{
						m_Mobile.RevealingAction();
						m_Mobile.m_NextCombatTime = DateTime.Now + weapon.OnSwing(m_Mobile, combatant);
					}
				}
			}
		}

		private class ExpireCombatantTimer : Timer
		{
			private Mobile m_Mobile;

			public ExpireCombatantTimer(Mobile m)
				: base(TimeSpan.FromMinutes(1.0))
			{
				this.Priority = TimerPriority.FiveSeconds;
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				m_Mobile.Combatant = null;
			}
		}

		private static TimeSpan m_ExpireCriminalDelay = TimeSpan.FromMinutes(2.0);

		public static TimeSpan ExpireCriminalDelay
		{
			get { return m_ExpireCriminalDelay; }
			set { m_ExpireCriminalDelay = value; }
		}

		private class ExpireCriminalTimer : Timer
		{
			private Mobile m_Mobile;

			public ExpireCriminalTimer(Mobile m)
				: base(m_ExpireCriminalDelay)
			{
				this.Priority = TimerPriority.FiveSeconds;
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				m_Mobile.Criminal = false;
			}
		}

		private class ExpireAggressorsTimer : Timer
		{
			private Mobile m_Mobile;

			public ExpireAggressorsTimer(Mobile m)
				: base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0))
			{
				m_Mobile = m;
				Priority = TimerPriority.FiveSeconds;
			}

			protected override void OnTick()
			{
				if (m_Mobile.Deleted || (m_Mobile.Aggressors.Count == 0 && m_Mobile.Aggressed.Count == 0))
					m_Mobile.StopAggrExpire();
				else
					m_Mobile.CheckAggrExpire();
			}
		}

		private DateTime m_NextCombatTime;

		public DateTime NextSkillTime
		{
			get
			{
				return m_NextSkillTime;
			}
			set
			{
				m_NextSkillTime = value;
			}
		}

		public ArrayList Aggressors
		{
			get
			{
				return m_Aggressors;
			}
		}

		public ArrayList Aggressed
		{
			get
			{
				return m_Aggressed;
			}
		}

		private int m_ChangingCombatant;

		public bool ChangingCombatant
		{
			get { return (m_ChangingCombatant > 0); }
		}

		public virtual void Attack(Mobile m)
		{
			if (CheckAttack(m))
				Combatant = m;
		}

		public virtual bool CheckAttack(Mobile m)
		{
			return (Utility.InUpdateRange(this, m) && CanSee(m) && InLOS(m));
		}

		/// <summary>
		/// Overridable. Gets or sets which Mobile that this Mobile is currently engaged in combat with.
		/// <seealso cref="OnCombatantChange" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual Mobile Combatant
		{
			get
			{
				return m_Combatant;
			}
			set
			{
				if (m_Deleted)
					return;

				if (m_Combatant != value && value != this)
				{
					Mobile old = m_Combatant;

					++m_ChangingCombatant;
					m_Combatant = value;

					if ((m_Combatant != null && !CanBeHarmful(m_Combatant, false)) || !Region.OnCombatantChange(this, old, m_Combatant))
					{
						m_Combatant = old;
						--m_ChangingCombatant;
						return;
					}

					if (m_NetState != null)
						m_NetState.Send(new ChangeCombatant(m_Combatant));

					if (m_Combatant == null)
					{
						if (m_ExpireCombatant != null)
							m_ExpireCombatant.Stop();

						if (m_CombatTimer != null)
							m_CombatTimer.Stop();

						m_ExpireCombatant = null;
						m_CombatTimer = null;
					}
					else
					{
						if (m_ExpireCombatant == null)
							m_ExpireCombatant = new ExpireCombatantTimer(this);

						m_ExpireCombatant.Start();

						if (m_CombatTimer == null)
							m_CombatTimer = new CombatTimer(this);

						m_CombatTimer.Start();
					}

					if (m_Combatant != null && CanBeHarmful(m_Combatant, false))
					{
						DoHarmful(m_Combatant);

						if (m_Combatant != null)
							m_Combatant.PlaySound(m_Combatant.GetAngerSound());
					}

					OnCombatantChange();
					--m_ChangingCombatant;
				}
			}
		}

		/// <summary>
		/// Overridable. Virtual event invoked after the <see cref="Combatant" /> property has changed.
		/// <seealso cref="Combatant" />
		/// </summary>
		public virtual void OnCombatantChange()
		{
		}

		public double GetDistanceToSqrt(Point3D p)
		{
			int xDelta = m_Location.m_X - p.m_X;
			int yDelta = m_Location.m_Y - p.m_Y;

			return Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));
		}

		public double GetDistanceToSqrt(Mobile m)
		{
			int xDelta = m_Location.m_X - m.m_Location.m_X;
			int yDelta = m_Location.m_Y - m.m_Location.m_Y;

			return Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));
		}

		public double GetDistanceToSqrt(IPoint2D p)
		{
			int xDelta = m_Location.m_X - p.X;
			int yDelta = m_Location.m_Y - p.Y;

			return Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));
		}

		public virtual void AggressiveAction(Mobile aggressor)
		{
			AggressiveAction(aggressor, false);
		}

		public virtual void AggressiveAction(Mobile aggressor, bool criminal)
		{
			if (aggressor == this)
				return;

			AggressiveActionEventArgs args = AggressiveActionEventArgs.Create(this, aggressor, criminal);

			EventSink.InvokeAggressiveAction(args);

			args.Free();

			if (Combatant == aggressor)
			{
				if (m_ExpireCombatant == null)
					m_ExpireCombatant = new ExpireCombatantTimer(this);
				else
					m_ExpireCombatant.Stop();

				m_ExpireCombatant.Start();
			}

			bool addAggressor = true;

			ArrayList list = m_Aggressors;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = (AggressorInfo)list[i];

				if (info.Attacker == aggressor)
				{
					info.Refresh();
					info.CriminalAggression = criminal;
					info.CanReportMurder = criminal;
					addAggressor = false;
					break;					// Adam: exit when you know your answer
				}
			}

			list = aggressor.m_Aggressors;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = (AggressorInfo)list[i];

				if (info.Attacker == this)
				{
					info.Refresh();
					addAggressor = false;
					break;					// Adam: exit when you know your answer
				}
			}

			bool addAggressed = true;

			list = m_Aggressed;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = (AggressorInfo)list[i];

				if (info.Defender == aggressor)
				{
					info.Refresh();
					addAggressed = false;
					break;					// Adam: exit when you know your answer
				}
			}

			list = aggressor.m_Aggressed;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = (AggressorInfo)list[i];

				if (info.Defender == this)
				{
					info.Refresh();
					info.CriminalAggression = criminal;
					info.CanReportMurder = criminal;
					addAggressed = false;
					break;					// Adam: exit when you know your answer
				}
			}

			bool setCombatant = false;

			if (addAggressor)
			{
				m_Aggressors.Add(AggressorInfo.Create(aggressor, this, criminal)); // new AggressorInfo( aggressor, this, criminal, true ) );

				if (this.CanSee(aggressor) && m_NetState != null)
					m_NetState.Send(new MobileIncoming(this, aggressor));

				if (Combatant == null)
					setCombatant = true;

				UpdateAggrExpire();
			}

			if (addAggressed)
			{
				aggressor.m_Aggressed.Add(AggressorInfo.Create(aggressor, this, criminal)); // new AggressorInfo( aggressor, this, criminal, false ) );

				if (this.CanSee(aggressor) && m_NetState != null)
					m_NetState.Send(new MobileIncoming(this, aggressor));

				if (Combatant == null)
					setCombatant = true;

				UpdateAggrExpire();
			}

			if (setCombatant)
				Combatant = aggressor;

			Region.OnAggressed(aggressor, this, criminal);
		}

		public void RemoveAggressed(Mobile aggressed)
		{
			if (m_Deleted)
				return;

			ArrayList list = m_Aggressed;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = (AggressorInfo)list[i];

				if (info.Defender == aggressed)
				{
					m_Aggressed.RemoveAt(i);
					info.Free();

					if (m_NetState != null && this.CanSee(aggressed))
						m_NetState.Send(new MobileIncoming(this, aggressed));

					break;
				}
			}

			UpdateAggrExpire();
		}

		public void RemoveAggressor(Mobile aggressor)
		{
			if (m_Deleted)
				return;

			ArrayList list = m_Aggressors;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = (AggressorInfo)list[i];

				if (info.Attacker == aggressor)
				{
					m_Aggressors.RemoveAt(i);
					info.Free();

					if (m_NetState != null && this.CanSee(aggressor))
						m_NetState.Send(new MobileIncoming(this, aggressor));

					break;
				}
			}

			UpdateAggrExpire();
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int TotalGold
		{
			get
			{
				return m_TotalGold;
			}
			set
			{
				if (m_TotalGold != value)
				{
					m_TotalGold = value;

					Delta(MobileDelta.Gold);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Followers
		{
			get
			{
				return m_Followers;
			}
			set
			{
				if (m_Followers != value)
				{
					m_Followers = value;

					Delta(MobileDelta.Followers);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int FollowersMax
		{
			get
			{
				return m_FollowersMax;
			}
			set
			{
				if (m_FollowersMax != value)
				{
					m_FollowersMax = value;

					Delta(MobileDelta.Followers);
				}
			}
		}

		public virtual void UpdateTotals()
		{
			if (m_Items == null)
				return;

			int oldValue = m_TotalWeight;

			m_TotalGold = 0;
			m_TotalWeight = 0;

			for (int i = 0; i < m_Items.Count; ++i)
			{
				Item item = (Item)m_Items[i];

				item.UpdateTotals();

				if (!(item is BankBox))
				{
					m_TotalGold += item.TotalGold;
					m_TotalWeight += item.TotalWeight + item.PileWeight;
				}
			}

			if (m_Holding != null)
				m_TotalWeight += m_Holding.TotalWeight + m_Holding.PileWeight;

			OnWeightChange(oldValue);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int TotalWeight
		{
			get
			{
				return m_TotalWeight;
			}
			set
			{
				int oldValue = m_TotalWeight;

				if (oldValue != value)
				{
					m_TotalWeight = value;

					Delta(MobileDelta.Weight);

					OnWeightChange(oldValue);
				}
			}
		}

		public void ClearQuestArrow()
		{
			m_QuestArrow = null;
		}

		public void ClearTarget()
		{
			m_Target = null;
		}

		private bool m_TargetLocked;

		public bool TargetLocked
		{
			get
			{
				return m_TargetLocked;
			}
			set
			{
				m_TargetLocked = value;
			}
		}

		private class SimpleTarget : Target
		{
			private TargetCallback m_Callback;

			public SimpleTarget(int range, TargetFlags flags, bool allowGround, TargetCallback callback)
				: base(range, allowGround, flags)
			{
				m_Callback = callback;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Callback != null)
					m_Callback(from, targeted);
			}
		}

		public Target BeginTarget(int range, bool allowGround, TargetFlags flags, TargetCallback callback)
		{
			Target t = new SimpleTarget(range, flags, allowGround, callback);

			this.Target = t;

			return t;
		}

		private class SimpleStateTarget : Target
		{
			private TargetStateCallback m_Callback;
			private object m_State;

			public SimpleStateTarget(int range, TargetFlags flags, bool allowGround, TargetStateCallback callback, object state)
				: base(range, allowGround, flags)
			{
				m_Callback = callback;
				m_State = state;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Callback != null)
					m_Callback(from, targeted, m_State);
			}
		}

		public Target BeginTarget(int range, bool allowGround, TargetFlags flags, TargetStateCallback callback, object state)
		{
			Target t = new SimpleStateTarget(range, flags, allowGround, callback, state);

			this.Target = t;

			return t;
		}

		public Target Target
		{
			get
			{
				return m_Target;
			}
			set
			{
				Target oldTarget = m_Target;
				Target newTarget = value;

				if (oldTarget == newTarget)
					return;

				m_Target = null;

				if (oldTarget != null && newTarget != null)
					oldTarget.Cancel(this, TargetCancelType.Overriden);

				m_Target = newTarget;

				if (newTarget != null && m_NetState != null && !m_TargetLocked)
					m_NetState.Send(newTarget.GetPacket());

				OnTargetChange();



				/*if ( m_Target != value )
				{
					if ( m_Target != null && value != null )
						m_Target.Cancel( this, TargetCancelType.Overriden );

					m_Target = value;

					if ( m_Target != null && m_NetState != null && !m_TargetLocked )
						m_NetState.Send( m_Target.GetPacket() );
					//m_NetState.Send( new TargetReq( m_Target ) );

					OnTargetChange();
				}*/
			}
		}

		/// <summary>
		/// Overridable. Virtual event invoked after the <see cref="Target">Target property</see> has changed.
		/// </summary>
		protected virtual void OnTargetChange()
		{
		}

		public ContextMenu ContextMenu
		{
			get
			{
				return m_ContextMenu;
			}
			set
			{
				m_ContextMenu = value;

				if (m_ContextMenu != null)
					Send(new DisplayContextMenu(m_ContextMenu));
			}
		}

		public virtual bool CheckContextMenuDisplay(IEntity target)
		{
			return true;
		}

		public Prompt Prompt
		{
			get
			{
				return m_Prompt;
			}
			set
			{
				Prompt oldPrompt = m_Prompt;
				Prompt newPrompt = value;

				if (oldPrompt == newPrompt)
					return;

				m_Prompt = null;

				if (oldPrompt != null && newPrompt != null)
					oldPrompt.OnCancel(this);

				m_Prompt = newPrompt;

				if (newPrompt != null)
					Send(new UnicodePrompt(newPrompt));
			}
		}

		private bool InternalOnMove(Direction d)
		{
			if (!OnMove(d))
				return false;

			MovementEventArgs e = MovementEventArgs.Create(this, d);

			EventSink.InvokeMovement(e);

			bool ret = !e.Blocked;

			e.Free();

			return ret;
		}

		/// <summary>
		/// Overridable. Event invoked before the Mobile <see cref="Move">moves</see>.
		/// </summary>
		/// <returns>True if the move is allowed, false if not.</returns>
		protected virtual bool OnMove(Direction d)
		{
			if (m_Hidden && m_AccessLevel == AccessLevel.Player)
			{
				if (m_AllowedStealthSteps-- <= 0 || (d & Direction.Running) != 0 || this.Mounted)
					RevealingAction();
			}

			return true;
		}

		//private static MobileMoving[] m_MovingPacketCache = new MobileMoving[8];
		private static Packet[] m_MovingPacketCache = new Packet[8];

		private bool m_Pushing;

		public bool Pushing
		{
			get
			{
				return m_Pushing;
			}
			set
			{
				m_Pushing = value;
			}
		}

		private static TimeSpan m_WalkFoot = TimeSpan.FromSeconds(0.4);
		private static TimeSpan m_RunFoot = TimeSpan.FromSeconds(0.2);
		private static TimeSpan m_WalkMount = TimeSpan.FromSeconds(0.2);
		private static TimeSpan m_RunMount = TimeSpan.FromSeconds(0.1);

		private DateTime m_EndQueue;

		private static ArrayList m_MoveList = new ArrayList();

		private static AccessLevel m_FwdAccessOverride = AccessLevel.GameMaster;
		private static bool m_FwdEnabled = true;
		private static bool m_FwdUOTDOverride = false;
		private static int m_FwdMaxSteps = 4;

		public static AccessLevel FwdAccessOverride { get { return m_FwdAccessOverride; } set { m_FwdAccessOverride = value; } }
		public static bool FwdEnabled { get { return m_FwdEnabled; } set { m_FwdEnabled = value; } }
		public static bool FwdUOTDOverride { get { return m_FwdUOTDOverride; } set { m_FwdUOTDOverride = value; } }
		public static int FwdMaxSteps { get { return m_FwdMaxSteps; } set { m_FwdMaxSteps = value; } }

		public virtual void ClearFastwalkStack()
		{
			if (m_MoveRecords != null && m_MoveRecords.Count > 0)
				m_MoveRecords.Clear();

			m_EndQueue = DateTime.Now;
		}

		public virtual bool CheckMovement(Direction d, out int newZ)
		{
			return Movement.Movement.CheckMovement(this, d, out newZ);
		}

		//private int m_FastWalkCount = 0;

		public virtual bool Move(Direction d)
		{
			if (m_Deleted)
				return false;

			BankBox box = FindBankNoCreate();

			if (box != null && box.Opened)
				box.Close();

			Point3D newLocation = m_Location;
			Point3D oldLocation = newLocation;

			if ((m_Direction & Direction.Mask) == (d & Direction.Mask))
			{
				// We are actually moving (not just a direction change)

				if (m_Spell != null && !m_Spell.OnCasterMoving(d))
					return false;

				if (m_Paralyzed || m_Frozen)
				{
					SendLocalizedMessage(500111); // You are frozen and can not move.

					return false;
				}

				int newZ;

				if (CheckMovement(d, out newZ))
				{
					int x = oldLocation.m_X, y = oldLocation.m_Y;
					int oldX = x, oldY = y;
					int oldZ = oldLocation.m_Z;

					switch (d & Direction.Mask)
					{
						case Direction.North:
							--y;
							break;
						case Direction.Right:
							++x;
							--y;
							break;
						case Direction.East:
							++x;
							break;
						case Direction.Down:
							++x;
							++y;
							break;
						case Direction.South:
							++y;
							break;
						case Direction.Left:
							--x;
							++y;
							break;
						case Direction.West:
							--x;
							break;
						case Direction.Up:
							--x;
							--y;
							break;
					}

					newLocation.m_X = x;
					newLocation.m_Y = y;
					newLocation.m_Z = newZ;

					m_Pushing = false;

					Map map = m_Map;

					if (map != null)
					{
						Sector oldSector = map.GetSector(oldX, oldY);
						Sector newSector = map.GetSector(x, y);
						ArrayList OnMoveOff = new ArrayList();
						ArrayList OnMoveOver = new ArrayList();

						if (oldSector != newSector)
						{
							foreach (Mobile m in oldSector.Mobiles.Values)
							{
								if (m == null)
									continue;

								if (m != this && m.X == oldX && m.Y == oldY && (m.Z + 15) > oldZ && (oldZ + 15) > m.Z)
									OnMoveOff.Add(m);
							}

							foreach (Mobile m in OnMoveOff)
								if (!m.OnMoveOff(this))
									return false;
							OnMoveOff.Clear();

							foreach (Item item in oldSector.Items.Values)
							{
								if (item == null)
									continue;

								if (item.AtWorldPoint(oldX, oldY) && (item.Z == oldZ || ((item.Z + item.ItemData.Height) > oldZ && (oldZ + 15) > item.Z)))
									OnMoveOff.Add(item);
							}

							foreach (Item item in OnMoveOff)
								if (!item.OnMoveOff(this))
									return false;
							OnMoveOff.Clear();

							foreach (Mobile m in newSector.Mobiles.Values)
							{
								if (m == null)
									continue;

								if (m.X == x && m.Y == y && (m.Z + 15) > newZ && (newZ + 15) > m.Z)
									OnMoveOver.Add(m);
							}

							foreach (Mobile m in OnMoveOver)
								if (!m.OnMoveOver(this))
									return false;
							OnMoveOver.Clear();

							foreach (Item item in newSector.Items.Values)
							{
								if (item == null)
									continue;

								if (item.AtWorldPoint(x, y) && (item.Z == newZ || ((item.Z + item.ItemData.Height) > newZ && (newZ + 15) > item.Z)))
									OnMoveOver.Add(item);
							}

							foreach (Item item in OnMoveOver)
								if (!item.OnMoveOver(this))
									return false;
							OnMoveOver.Clear();
						}
						else
						{

							foreach (Mobile m in oldSector.Mobiles.Values)
							{
								if (m == null)
									continue;

								if (m != this && m.X == oldX && m.Y == oldY && (m.Z + 15) > oldZ && (oldZ + 15) > m.Z)
									OnMoveOff.Add(m);
								else if (m.X == x && m.Y == y && (m.Z + 15) > newZ && (newZ + 15) > m.Z)
									OnMoveOver.Add(m);
							}

							for (int ix = 0, jx = 0; true; ix++, jx++)
							{
								if (ix < OnMoveOff.Count)
									if (!(OnMoveOff[ix] as Mobile).OnMoveOff(this))
										return false;

								if (jx < OnMoveOver.Count)
									if (!(OnMoveOver[jx] as Mobile).OnMoveOver(this))
										return false;

								if (ix >= OnMoveOff.Count && jx >= OnMoveOver.Count)
									break;
							}
							OnMoveOver.Clear();
							OnMoveOff.Clear();

							foreach (Item item in oldSector.Items.Values)
							{
								if (item == null)
									continue;

								if (item.AtWorldPoint(oldX, oldY) && (item.Z == oldZ || ((item.Z + item.ItemData.Height) > oldZ && (oldZ + 15) > item.Z)))
									OnMoveOff.Add(item);
								else if (item.AtWorldPoint(x, y) && (item.Z == newZ || ((item.Z + item.ItemData.Height) > newZ && (newZ + 15) > item.Z)))
									OnMoveOver.Add(item);
							}

							for (int ix = 0, jx = 0; true; ix++, jx++)
							{
								if (ix < OnMoveOff.Count)
									if (!(OnMoveOff[ix] as Item).OnMoveOff(this))
										return false;

								if (jx < OnMoveOver.Count)
									if (!(OnMoveOver[jx] as Item).OnMoveOver(this))
										return false;

								if (ix >= OnMoveOff.Count && jx >= OnMoveOver.Count)
									break;
							}
							OnMoveOver.Clear();
							OnMoveOff.Clear();
						}

						//if( !Region.CanMove( this, d, newLocation, oldLocation, m_Map ) )
						//	return false;
					}
					else
					{
						return false;
					}

					if (!InternalOnMove(d))
						return false;

					if (m_FwdEnabled && m_NetState != null && m_AccessLevel < m_FwdAccessOverride && (!m_FwdUOTDOverride || !m_NetState.IsUOTDClient))
					{
						if (m_MoveRecords == null)
							m_MoveRecords = new Queue<MovementRecord>(6);

						while (m_MoveRecords.Count > 0)
						{
							MovementRecord r = m_MoveRecords.Peek();

							if (r.Expired())
								m_MoveRecords.Dequeue();
							else
								break;
						}

						if (m_MoveRecords.Count >= m_FwdMaxSteps)
						{
							FastWalkEventArgs fw = new FastWalkEventArgs(m_NetState);
							EventSink.InvokeFastWalk(fw);

							if (fw.Blocked)
								return false;
						}

						TimeSpan delay = ComputeMovementSpeed(d);

						/*if ( Mounted )
                            delay = (d & Direction.Running) != 0 ? m_RunMount : m_WalkMount;
                        else
							delay = ( d & Direction.Running ) != 0 ? m_RunFoot : m_WalkFoot;*/

						DateTime end;

						if (m_MoveRecords.Count > 0)
							end = m_EndQueue + delay;
						else
							end = DateTime.Now + delay;

						m_MoveRecords.Enqueue(MovementRecord.NewInstance(end));

						m_EndQueue = end;
					}

					m_LastMoveTime = DateTime.Now;
				}
				else
				{
					return false;
				}

				DisruptiveAction();
			}

			if (m_NetState != null)
				m_NetState.Send(MovementAck.Instantiate(m_NetState.Sequence, this));//new MovementAck( m_NetState.Sequence, this ) );

			SetLocation(newLocation, false);
			SetDirection(d);

			if (m_Map != null)
			{
				IPooledEnumerable eable = m_Map.GetObjectsInRange(m_Location, Core.GlobalMaxUpdateRange);

				foreach (object o in eable)
				{
					if (o == this)
						continue;

					if (o is Mobile)
					{
						m_MoveList.Add(o);
					}
					else if (o is Item)
					{
						Item item = (Item)o;

						if (item.HandlesOnMovement)
							m_MoveList.Add(item);
					}
				}

				eable.Free();

				Packet[] cache = m_MovingPacketCache;

				for (int i = 0; i < cache.Length; ++i)
					Packet.Release(ref cache[i]);

				for (int i = 0; i < m_MoveList.Count; ++i)
				{
					object o = m_MoveList[i];

					if (o is Mobile)
					{
						Mobile m = (Mobile)m_MoveList[i];
						NetState ns = m.NetState;

						if (ns != null && Utility.InUpdateRange(m_Location, m.m_Location) && m.CanSee(this))
						{
							int noto = Notoriety.Compute(m, this);
							Packet p = cache[noto];

							if (p == null)
								cache[noto] = p = Packet.Acquire(new MobileMoving(this, noto));

							ns.Send(p);
						}

						m.OnMovement(this, oldLocation);
					}
					else if (o is Item)
					{
						((Item)o).OnMovement(this, oldLocation);
					}
				}

				for (int i = 0; i < cache.Length; ++i)
					Packet.Release(ref cache[i]);

				if (m_MoveList.Count > 0)
					m_MoveList.Clear();
			}

			OnAfterMove(oldLocation);
			return true;
		}

		public virtual void OnAfterMove(Point3D oldLocation)
		{
		}

		public TimeSpan ComputeMovementSpeed()
		{
			return ComputeMovementSpeed(this.Direction, false);
		}
		public virtual TimeSpan ComputeMovementSpeed(Direction dir)
		{
			return ComputeMovementSpeed(dir, true);
		}
		public virtual TimeSpan ComputeMovementSpeed(Direction dir, bool checkTurning)
		{
			TimeSpan delay;

			if (Mounted)
				delay = (dir & Direction.Running) != 0 ? m_RunMount : m_WalkMount;
			else
				delay = (dir & Direction.Running) != 0 ? m_RunFoot : m_WalkFoot;

			return delay;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when a Mobile <paramref name="m" /> moves off this Mobile.
		/// </summary>
		/// <returns>True if the move is allowed, false if not.</returns>
		public virtual bool OnMoveOff(Mobile m)
		{
			return true;
		}

		public virtual bool IsDeadBondedPet { get { return false; } }

		/// <summary>
		/// Overridable. Event invoked when a Mobile <paramref name="m" /> moves over this Mobile.
		/// </summary>
		/// <returns>True if the move is allowed, false if not.</returns>
		public virtual bool OnMoveOver(Mobile m)
		{
			if (m_Map == null || m_Deleted)
				return true;

			if ((m_Map.Rules & MapRules.FreeMovement) == 0)
			{
				if (!Alive || !m.Alive || IsDeadBondedPet || m.IsDeadBondedPet)
					return true;
				else if (m_Hidden && m_AccessLevel > AccessLevel.Player)
					return true;

				if (!m.m_Pushing)
				{
					m.m_Pushing = true;

					int number;

					if (m.AccessLevel > AccessLevel.Player)
					{
						number = m_Hidden ? 1019041 : 1019040;
					}
					else
					{
						if (m.Stam == m.StamMax)
						{
							number = m_Hidden ? 1019043 : 1019042;
							m.Stam -= 10;

							m.RevealingAction();
						}
						else
						{
							return false;
						}
					}

					m.SendLocalizedMessage(number);
				}
			}

			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile sees another Mobile, <paramref name="m" />, move.
		/// </summary>
		public virtual void OnMovement(Mobile m, Point3D oldLocation)
		{
		}

		public ISpell Spell
		{
			get
			{
				return m_Spell;
			}
			set
			{
				if (m_Spell != null && value != null)
					Console.WriteLine("Warning: Spell has been overwritten");

				m_Spell = value;
			}
		}

		[CommandProperty(AccessLevel.Administrator)]
		public bool AutoPageNotify
		{
			get
			{
				return m_AutoPageNotify;
			}
			set
			{
				m_AutoPageNotify = value;
			}
		}

		public virtual void CriminalAction(bool message)
		{
			Criminal = true;

			m_Region.OnCriminalAction(this, message);
		}

		public bool CanUseStuckMenu()
		{
			if (m_StuckMenuUses == null)
			{
				return true;
			}
			else
			{
				for (int i = 0; i < m_StuckMenuUses.Length; ++i)
				{
					if ((DateTime.Now - m_StuckMenuUses[i]) > TimeSpan.FromDays(1.0))
					{
						return true;
					}
				}

				return false;
			}
		}

		public virtual bool IsSnoop(Mobile from)
		{
			return (from != this);
		}

		/// <summary>
		/// Overridable. Any call to <see cref="Resurrect" /> will silently fail if this method returns false.
		/// <seealso cref="Resurrect" />
		/// </summary>
		public virtual bool CheckResurrect()
		{
			return true;
		}

		/// <summary>
		/// Overridable. Event invoked before the Mobile is <see cref="Resurrect">resurrected</see>.
		/// <seealso cref="Resurrect" />
		/// </summary>
		public virtual void OnBeforeResurrect()
		{
		}

		/// <summary>
		/// Overridable. Event invoked after the Mobile is <see cref="Resurrect">resurrected</see>.
		/// <seealso cref="Resurrect" />
		/// </summary>
		public virtual void OnAfterResurrect()
		{
		}

		public virtual void Resurrect()
		{
			if (!Alive)
			{
				if (!Region.OnResurrect(this))
					return;

				if (!CheckResurrect())
					return;

				OnBeforeResurrect();

				BankBox box = FindBankNoCreate();

				if (box != null && box.Opened)
					box.Close();

				Poison = null;

				Warmode = false;

				Hits = 10;
				Stam = StamMax;
				Mana = 0;

				BodyMod = 0;
				Body = m_Female ? 0x191 : 0x190;

				ProcessDeltaQueue();

				for (int i = m_Items.Count - 1; i >= 0; --i)
				{
					if (i >= m_Items.Count)
						continue;

					Item item = (Item)m_Items[i];

					if (item.ItemID == 0x204E)
						item.Delete();
				}

				this.SendIncomingPacket();
				this.SendIncomingPacket();

				OnAfterResurrect();

				//Send( new DeathStatus( false ) );
			}
		}

		private IAccount m_Account;

		// Changed old hackish "Administrator + 1" to new ReadOnly access level.
		[CommandProperty(AccessLevel.Counselor, AccessLevel.ReadOnly)]
		public IAccount Account
		{
			get
			{
				return m_Account;
			}
			set
			{
				m_Account = value;
			}
		}

		private bool m_Deleted;

		public bool Deleted
		{
			get
			{
				return m_Deleted;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int VirtualArmor
		{
			get
			{
				return m_VirtualArmor;
			}
			set
			{
				if (m_VirtualArmor != value)
				{
					m_VirtualArmor = value;

					Delta(MobileDelta.Armor);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual double ArmorRating
		{
			get
			{
				return 0.0;
			}
		}

		public void DropHolding()
		{
			Item holding = m_Holding;

			if (holding != null)
			{
				if (!holding.Deleted && holding.Map == Map.Internal)
					AddToBackpack(holding);

				Holding = null;
				holding.ClearBounce();
			}
		}

		public virtual void Delete()
		{
			if (m_Deleted)
				return;
			else if (!World.OnDelete(this))
				return;

			if (m_NetState != null)
				m_NetState.CancelAllTrades();

			if (m_NetState != null)
				m_NetState.Dispose();

			DropHolding();

			Region.InternalExit(this);

			OnDelete();

			for (int i = m_Items.Count - 1; i >= 0; --i)
				if (i < m_Items.Count)
					((Item)m_Items[i]).OnParentDeleted(this);

			SendRemovePacket();

			if (m_Guild != null)
				m_Guild.OnDelete(this);

			m_Deleted = true;

			if (m_Map != null)
			{
				m_Map.OnLeave(this);
				m_Map = null;
			}

			m_Hair = null;
			m_Beard = null;
			m_MountItem = null;

			World.RemoveMobile(this);

			OnAfterDelete();

			FreeCache();
		}

		/// <summary>
		/// Overridable. Virtual event invoked before the Mobile is deleted.
		/// </summary>
		public virtual void OnDelete()
		{
		}

		/// <summary>
		/// Overridable. Returns true if the player is alive, false if otherwise. By default, this is computed by: <c>!Deleted &amp;&amp; (!Player || !Body.IsGhost)</c>
		/// </summary>
		[CommandProperty(AccessLevel.Counselor)]
		public virtual bool Alive
		{
			get
			{	// Adam: Even though you may be tempted, do not add an IsDeadBondedPet to this test
				//	A dead bonded pet is still considered 'Alive'
				return !m_Deleted && (!m_Player || !m_Body.IsGhost);
			}
		}

		public virtual bool CheckSpellCast(ISpell spell)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile casts a <paramref name="spell" />.
		/// </summary>
		/// <param name="spell"></param>
		public virtual void OnSpellCast(ISpell spell)
		{
		}

		/// <summary>
		/// Overridable. Virtual event invoked after <see cref="TotalWeight" /> changes.
		/// </summary>
		public virtual void OnWeightChange(int oldValue)
		{
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the <see cref="Skill.Base" /> or <see cref="Skill.BaseFixedPoint" /> property of <paramref name="skill" /> changes.
		/// </summary>
		public virtual void OnSkillChange(SkillName skill, double oldBase)
		{
		}

		/// <summary>
		/// Overridable. Invoked after the mobile is deleted. When overriden, be sure to call the base method.
		/// </summary>
		public virtual void OnAfterDelete()
		{
			StopAggrExpire();

			CheckAggrExpire();

			if (m_PoisonTimer != null)
				m_PoisonTimer.Stop();

			if (m_HitsTimer != null)
				m_HitsTimer.Stop();

			if (m_StamTimer != null)
				m_StamTimer.Stop();

			if (m_ManaTimer != null)
				m_ManaTimer.Stop();

			if (m_CombatTimer != null)
				m_CombatTimer.Stop();

			if (m_ExpireCombatant != null)
				m_ExpireCombatant.Stop();

			if (m_LogoutTimer != null)
				m_LogoutTimer.Stop();

			if (m_ExpireCriminal != null)
				m_ExpireCriminal.Stop();

			if (m_WarmodeTimer != null)
				m_WarmodeTimer.Stop();

			if (m_ParaTimer != null)
				m_ParaTimer.Stop();

			if (m_FrozenTimer != null)
				m_FrozenTimer.Stop();

			if (m_AutoManifestTimer != null)
				m_AutoManifestTimer.Stop();
		}

		public virtual bool AllowSkillUse(SkillName name)
		{
			return true;
		}

		public virtual bool UseSkill(SkillName name)
		{
			return Skills.UseSkill(this, name);
		}

		public virtual bool UseSkill(int skillID)
		{
			return Skills.UseSkill(this, skillID);
		}

		private static CreateCorpseHandler m_CreateCorpse;

		public static CreateCorpseHandler CreateCorpseHandler
		{
			get { return m_CreateCorpse; }
			set { m_CreateCorpse = value; }
		}

		//plasma: default corpse/bones delay values

		/// <summary>
		/// Returns the length of time for the corpse to decay into bones
		/// </summary>
		/// <returns></returns>
		public virtual TimeSpan CorpseDecayTime() { return TimeSpan.FromMinutes(7.0); }

		/// <summary>
		/// Returns the length of time for the bones to decay
		/// </summary>
		/// <returns></returns>
		public virtual TimeSpan BoneDecayTime() { return TimeSpan.FromMinutes(7.0); }


		public virtual DeathMoveResult GetParentMoveResultFor(Item item)
		{
			return item.OnParentDeath(this);
		}

		public virtual DeathMoveResult GetInventoryMoveResultFor(Item item)
		{
			return item.OnInventoryDeath(this);
		}

		public virtual bool RetainPackLocsOnDeath { get { return Core.AOS; } }

		public virtual void Kill()
		{
			if (!CanBeDamaged())
				return;
			else if (!Alive || IsDeadBondedPet)
				return;
			else if (m_Deleted)
				return;
			else if (!Region.OnDeath(this))
				return;
			else if (!OnBeforeDeath())
				return;

			BankBox box = FindBankNoCreate();

			if (box != null && box.Opened)
				box.Close();

			if (m_NetState != null)
				m_NetState.CancelAllTrades();

			if (m_Spell != null)
				m_Spell.OnCasterKilled();
			//m_Spell.Disturb( DisturbType.Kill );

			if (m_Target != null)
				m_Target.Cancel(this, TargetCancelType.Canceled);

			DisruptiveAction();

			Warmode = false;

			DropHolding();

			Hits = 0;
			Stam = 0;
			Mana = 0;

			Poison = null;
			Combatant = null;

			if (Paralyzed)
			{
				Paralyzed = false;

				if (m_ParaTimer != null)
					m_ParaTimer.Stop();
			}

			if (Frozen)
			{
				Frozen = false;

				if (m_FrozenTimer != null)
					m_FrozenTimer.Stop();
			}

			ArrayList content = new ArrayList();
			ArrayList equip = new ArrayList();
			ArrayList moveToPack = new ArrayList();

			ArrayList itemsCopy = new ArrayList(m_Items);

			Container pack = this.Backpack;

			for (int i = 0; i < itemsCopy.Count; ++i)
			{
				Item item = (Item)itemsCopy[i];

				if (item == pack)
					continue;

				DeathMoveResult res = GetParentMoveResultFor(item);

				switch (res)
				{
					case DeathMoveResult.MoveToCorpse:
						{
							content.Add(item);
							equip.Add(item);
							break;
						}
					case DeathMoveResult.MoveToBackpack:
						{
							moveToPack.Add(item);
							break;
						}
				}
			}

			if (pack != null)
			{
				ArrayList packCopy = new ArrayList(pack.Items);

				for (int i = 0; i < packCopy.Count; ++i)
				{
					Item item = (Item)packCopy[i];

					DeathMoveResult res = GetInventoryMoveResultFor(item);

					if (res == DeathMoveResult.MoveToCorpse)
						content.Add(item);
					else
						moveToPack.Add(item);
				}

				for (int i = 0; i < moveToPack.Count; ++i)
				{
					Item item = (Item)moveToPack[i];

					if (RetainPackLocsOnDeath && item.Parent == pack)
						continue;

					pack.DropItem(item);
				}
			}

			Container c = (m_CreateCorpse == null ? null : m_CreateCorpse(this, content, equip));

			/*m_Corpse = c;

			for ( int i = 0; c != null && i < content.Count; ++i )
				c.DropItem( (Item)content[i] );

			if ( c != null )
				c.MoveToWorld( this.Location, this.Map );*/

			if (m_Map != null)
			{
				Packet animPacket = null;//new DeathAnimation( this, c );
				Packet remPacket = null;//this.RemovePacket;

				IPooledEnumerable eable = m_Map.GetClientsInRange(m_Location);

				foreach (NetState state in eable)
				{
					if (state != m_NetState)
					{
						if (animPacket == null)
							animPacket = Packet.Acquire(new DeathAnimation(this, c));

						state.Send(animPacket);

						if (!state.Mobile.CanSee(this))
						{
							if (remPacket == null)
								remPacket = this.RemovePacket;

							state.Send(remPacket);
						}
					}
				}

				Packet.Release(animPacket);

				eable.Free();
			}

			OnDeath(c);
		}

		private Container m_Corpse;

		[CommandProperty(AccessLevel.GameMaster)]
		public Container Corpse
		{
			get
			{
				return m_Corpse;
			}
			set
			{
				m_Corpse = value;
			}
		}

		/// <summary>
		/// Overridable. Event invoked before the Mobile is <see cref="Kill">killed</see>.
		/// <seealso cref="Kill" />
		/// <seealso cref="OnDeath" />
		/// </summary>
		/// <returns>True to continue with death, false to override it.</returns>
		public virtual bool OnBeforeDeath()
		{
			return true;
		}

		/// <summary>
		/// Overridable. Event invoked after the Mobile is <see cref="Kill">killed</see>. Primarily, this method is responsible for deleting an NPC or turning a PC into a ghost.
		/// <seealso cref="Kill" />
		/// <seealso cref="OnBeforeDeath" />
		/// </summary>
		public virtual void OnDeath(Container c)
		{
			int sound = this.GetDeathSound();

			if (sound >= 0)
				Effects.PlaySound(this, this.Map, sound);

			if (!m_Player)
			{
				Delete();
			}
			else
			{
				Send(DeathStatus.Instantiate(true));

				Warmode = false;

				BodyMod = 0;
				Body = this.Female ? 0x193 : 0x192;

				Item deathShroud = new Item(0x204E);

				deathShroud.Movable = false;
				deathShroud.Layer = Layer.OuterTorso;

				AddItem(deathShroud);

				m_Items.Remove(deathShroud);
				m_Items.Insert(0, deathShroud);

				Poison = null;
				Combatant = null;

				Hits = 0;
				Stam = 0;
				Mana = 0;

				EventSink.InvokePlayerDeath(new PlayerDeathEventArgs(this));

				ProcessDeltaQueue();

				Send(DeathStatus.Instantiate(false));

				CheckStatTimers();
			}
		}

		public virtual int GetAngerSound()
		{
			if (m_BaseSoundID != 0)
				return m_BaseSoundID;

			return -1;
		}

		public virtual int GetIdleSound()
		{
			if (m_BaseSoundID != 0)
				return m_BaseSoundID + 1;

			return -1;
		}

		public virtual int GetAttackSound()
		{
			if (m_BaseSoundID != 0)
				return m_BaseSoundID + 2;

			return -1;
		}

		public virtual int GetHurtSound()
		{
			if (m_BaseSoundID != 0)
				return m_BaseSoundID + 3;

			return -1;
		}

		public virtual int GetDeathSound()
		{
			if (m_BaseSoundID != 0)
			{
				return m_BaseSoundID + 4;
			}
			else if (m_Body.IsHuman)
			{
				return Utility.Random(m_Female ? 0x314 : 0x423, m_Female ? 4 : 5);
			}
			else
			{
				return -1;
			}
		}

		private static char[] m_GhostChars = new char[2] { 'o', 'O' };

		public static char[] GhostChars { get { return m_GhostChars; } set { m_GhostChars = value; } }

		private static bool m_NoSpeechLOS;

		public static bool NoSpeechLOS { get { return m_NoSpeechLOS; } set { m_NoSpeechLOS = value; } }

		private static TimeSpan m_AutoManifestTimeout = TimeSpan.FromSeconds(5.0);

		public static TimeSpan AutoManifestTimeout { get { return m_AutoManifestTimeout; } set { m_AutoManifestTimeout = value; } }

		private Timer m_AutoManifestTimer;

		private class AutoManifestTimer : Timer
		{
			private Mobile m_Mobile;

			public AutoManifestTimer(Mobile m, TimeSpan delay)
				: base(delay)
			{
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				if (!m_Mobile.Alive)
					m_Mobile.Warmode = false;
			}
		}

		public virtual bool CheckTarget(Mobile from, Target targ, object targeted)
		{
			return true;
		}

		private static bool m_InsuranceEnabled;

		public static bool InsuranceEnabled
		{
			get { return m_InsuranceEnabled; }
			set { m_InsuranceEnabled = value; }
		}

		public virtual void Use(Item item)
		{
			if (item == null || item.Deleted)
				return;

			DisruptiveAction();

			if (m_Spell != null && !m_Spell.OnCasterUsingObject(item))
				return;

			object root = item.RootParent;
			bool okay = false;

			if (!Utility.InUpdateRange(this, item.GetWorldLocation()))
				item.OnDoubleClickOutOfRange(this);
			else if (!CanSee(item))
				item.OnDoubleClickCantSee(this);
			else if (!item.IsAccessibleTo(this))
			{
				Region reg = Region.Find(item.GetWorldLocation(), item.Map);

				if (reg == null || !reg.SendInaccessibleMessage(item, this))
					item.OnDoubleClickNotAccessible(this);
			}
			else if (!CheckAlive(false))
				item.OnDoubleClickDead(this);
			else if (item.InSecureTrade)
				item.OnDoubleClickSecureTrade(this);
			else if (!AllowItemUse(item))
				okay = false;
			else if (!item.CheckItemUse(this, item))
				okay = false;
			else if (root != null && root is Mobile && ((Mobile)root).IsSnoop(this))
				item.OnSnoop(this);
			else if (m_Region.OnDoubleClick(this, item))
				okay = true;

			if (okay)
			{
				if (!item.Deleted)
					item.OnItemUsed(this, item);

				if (!item.Deleted)
					item.OnDoubleClick(this);
			}
		}

		public virtual void Use(Mobile m)
		{
			if (m == null || m.Deleted)
				return;

			DisruptiveAction();

			if (m_Spell != null && !m_Spell.OnCasterUsingObject(m))
				return;

			if (!Utility.InUpdateRange(this, m))
				m.OnDoubleClickOutOfRange(this);
			else if (!CanSee(m))
				m.OnDoubleClickCantSee(this);
			else if (!CheckAlive(false))
				m.OnDoubleClickDead(this);
			else if (m_Region.OnDoubleClick(this, m) && !m.Deleted)
				m.OnDoubleClick(this);
		}

		public virtual bool IsOwner(Mobile from)
		{
			return false;
		}

		public virtual void Lift(Item item, int amount, out bool rejected, out LRReason reject)
		{
			rejected = true;
			reject = LRReason.Inspecific;

			if (item == null)
				return;

			Mobile from = this;
			NetState state = m_NetState;

			if (from.AccessLevel >= AccessLevel.GameMaster || DateTime.Now >= from.NextActionTime)
			{
				if (from.CheckAlive())
				{
					from.DisruptiveAction();

					if (from.Holding != null)
					{
						reject = LRReason.AreHolding;
					}
					else if (from.AccessLevel < AccessLevel.GameMaster && !from.InRange(item.GetWorldLocation(), 2))
					{
						reject = LRReason.OutOfRange;
					}
					else if (!from.CanSee(item) || !from.InLOS(item))
					{
						reject = LRReason.OutOfSight;
					}
					else if (!item.VerifyMove(from))
					{
						reject = LRReason.CannotLift;
					}
					else if (item.InSecureTrade || !item.IsAccessibleTo(from))
					{
						reject = LRReason.CannotLift;
					}
					//					else if ( !item.CheckLift( from, item ) )
					//					{
					//						reject = LRReason.Inspecific;
					//					}
					else if (!item.CheckLift(from, item, ref reject))
					{
					}
					else
					{
						object root = item.RootParent;

						if (root != null && root is Mobile && !((Mobile)root).CheckNonlocalLift(from, item))
						{	// if CheckNonlocalLift fails and you are the owner of this NPC, stealing is not message we want
							if (((Mobile)root).IsOwner(from))
								reject = LRReason.Inspecific;
							else
								reject = LRReason.TryToSteal;
						}
						else if (!from.OnDragLift(item) || !item.OnDragLift(from))
						{
							reject = LRReason.Inspecific;
						}
						else if (!from.CheckAlive())
						{
							reject = LRReason.Inspecific;
						}
						else
						{
							item.SetLastMoved();

							if (amount == 0)
								amount = 1;

							if (amount > item.Amount)
								amount = item.Amount;

							int oldAmount = item.Amount;
							item.Amount = amount;

							if (amount < oldAmount)
								item.Dupe(oldAmount - amount);

							Map map = from.Map;

							if (Mobile.DragEffects && map != null && (root == null || root is Item))
							{
								IPooledEnumerable eable = map.GetClientsInRange(from.Location);
								Packet p = null;

								foreach (NetState ns in eable)
								{
									if (ns.Mobile != from && ns.Mobile.CanSee(from))
									{
										if (p == null)
										{
											IEntity src;

											if (root == null)
												src = new Entity(Serial.Zero, item.Location, map);
											else
												src = new Entity(((Item)root).Serial, ((Item)root).Location, map);

											p = Packet.Acquire(new DragEffect(src, from, item.ItemID, item.Hue, amount));
										}

										ns.Send(p);
									}
								}

								Packet.Release(p);

								eable.Free();
							}

							Point3D fixLoc = item.Location;
							Map fixMap = item.Map;
							bool shouldFix = (item.Parent == null);

							item.RecordBounce();
							item.OnItemLifted(from, item);
							item.Internalize();

							from.Holding = item;

							int liftSound = item.GetLiftSound(from);

							if (liftSound != -1)
								from.Send(new PlaySound(liftSound, from));

							from.NextActionTime = DateTime.Now + TimeSpan.FromSeconds(0.5);

							if (fixMap != null && shouldFix)
								fixMap.FixColumn(fixLoc.m_X, fixLoc.m_Y);

							reject = LRReason.Inspecific;
							rejected = false;
						}
					}
				}
				else
				{
					reject = LRReason.Inspecific;
				}
			}
			else
			{
				SendActionMessage();
				reject = LRReason.Inspecific;
			}

			if (rejected && state != null)
			{
				state.Send(new LiftRej(reject));

				if (item.Parent is Item)
				{
					if (state.IsPost6017)
					{
						state.Send(new ContainerContentUpdate6017(item));
					}
					else
					{
						state.Send(new ContainerContentUpdate(item));
					}
				}
				else if (item.Parent is Mobile)
					state.Send(new EquipUpdate(item));
				else
					item.SendInfoTo(state);

				if (ObjectPropertyList.Enabled && item.Parent != null)
					state.Send(item.OPLPacket);
			}
		}

		public virtual void SendDropEffect(Item item)
		{
			if (Mobile.DragEffects)
			{
				Map map = m_Map;
				object root = item.RootParent;

				if (map != null && (root == null || root is Item))
				{
					IPooledEnumerable eable = map.GetClientsInRange(m_Location);
					Packet p = null;

					foreach (NetState ns in eable)
					{
						if (ns.Mobile != this && ns.Mobile.CanSee(this))
						{
							if (p == null)
							{
								IEntity trg;

								if (root == null)
									trg = new Entity(Serial.Zero, item.Location, map);
								else
									trg = new Entity(((Item)root).Serial, ((Item)root).Location, map);

								p = Packet.Acquire(new DragEffect(this, trg, item.ItemID, item.Hue, item.Amount));
							}

							ns.Send(p);
						}
					}

					Packet.Release(p);

					eable.Free();
				}
			}
		}

		public virtual bool Drop(Item to, Point3D loc)
		{
			Mobile from = this;
			Item item = from.Holding;

			if (item == null)
				return false;

			from.Holding = null;
			bool bounced = true;

			item.SetLastMoved();

			if (to == null || !item.DropToItem(from, to, loc))
				item.Bounce(from);
			else
				bounced = false;

			item.ClearBounce();

			if (!bounced)
				SendDropEffect(item);

			return !bounced;
		}

		public virtual bool Drop(Point3D loc)
		{
			Mobile from = this;
			Item item = from.Holding;

			if (item == null)
				return false;

			from.Holding = null;
			bool bounced = true;

			item.SetLastMoved();

			if (!item.DropToWorld(from, loc))
				item.Bounce(from);
			else
				bounced = false;

			item.ClearBounce();

			if (!bounced)
				SendDropEffect(item);

			return !bounced;
		}

		public virtual bool Drop(Mobile to, Point3D loc)
		{
			Mobile from = this;
			Item item = from.Holding;

			if (item == null)
				return false;

			from.Holding = null;
			bool bounced = true;

			item.SetLastMoved();

			if (to == null || !item.DropToMobile(from, to, loc))
				item.Bounce(from);
			else
				bounced = false;

			item.ClearBounce();

			if (!bounced)
				SendDropEffect(item);

			return !bounced;
		}

		private static object m_GhostMutateContext = new object();

		public virtual bool MutateSpeech(ArrayList hears, ref string text, ref object context)
		{
			if (Alive)
				return false;

			StringBuilder sb = new StringBuilder(text.Length, text.Length);

			for (int i = 0; i < text.Length; ++i)
			{
				if (text[i] != ' ')
					sb.Append(m_GhostChars[Utility.Random(m_GhostChars.Length)]);
				else
					sb.Append(' ');
			}

			text = sb.ToString();
			context = m_GhostMutateContext;
			return true;
		}

		public virtual void Manifest(TimeSpan delay)
		{
			Warmode = true;

			if (m_AutoManifestTimer == null)
				m_AutoManifestTimer = new AutoManifestTimer(this, delay);
			else
				m_AutoManifestTimer.Stop();

			m_AutoManifestTimer.Start();
		}

		public virtual bool CheckSpeechManifest()
		{
			if (Alive)
				return false;

			TimeSpan delay = m_AutoManifestTimeout;

			if (delay > TimeSpan.Zero && (!Warmode || m_AutoManifestTimer != null))
			{
				Manifest(delay);
				return true;
			}

			return false;
		}

		public virtual bool CheckHearsMutatedSpeech(Mobile m, object context)
		{
			if (context == m_GhostMutateContext)
				return (m.Alive && !m.CanHearGhosts);

			return true;
		}

		private void AddSpeechItemsFrom(ArrayList list, Container cont)
		{
			for (int i = 0; i < cont.Items.Count; ++i)
			{
				Item item = (Item)cont.Items[i];

				if (item.HandlesOnSpeech)
					list.Add(item);

				if (item is Container)
					AddSpeechItemsFrom(list, (Container)item);
			}
		}

		private class LocationComparer : IComparer
		{
			private static LocationComparer m_Instance;

			public static LocationComparer GetInstance(IPoint3D relativeTo)
			{
				if (m_Instance == null)
					m_Instance = new LocationComparer(relativeTo);
				else
					m_Instance.m_RelativeTo = relativeTo;

				return m_Instance;
			}

			private IPoint3D m_RelativeTo;

			public IPoint3D RelativeTo
			{
				get { return m_RelativeTo; }
				set { m_RelativeTo = value; }
			}

			public LocationComparer(IPoint3D relativeTo)
			{
				m_RelativeTo = relativeTo;
			}

			private int GetDistance(IPoint3D p)
			{
				int x = m_RelativeTo.X - p.X;
				int y = m_RelativeTo.Y - p.Y;
				int z = m_RelativeTo.Z - p.Z;

				x *= 11;
				y *= 11;

				return (x * x) + (y * y) + (z * z);
			}

			public int Compare(object x, object y)
			{
				IPoint3D a = x as IPoint3D;
				IPoint3D b = y as IPoint3D;

				return GetDistance(a) - GetDistance(b);
			}
		}

		public IPooledEnumerable GetItemsInRange(int range)
		{
			Map map = m_Map;

			if (map == null)
				return Map.NullEnumerable.Instance;

			return map.GetItemsInRange(m_Location, range);
		}

		public IPooledEnumerable GetObjectsInRange(int range)
		{
			Map map = m_Map;

			if (map == null)
				return Map.NullEnumerable.Instance;

			return map.GetObjectsInRange(m_Location, range);
		}

		public IPooledEnumerable GetMobilesInRange(int range)
		{
			Map map = m_Map;

			if (map == null)
				return Map.NullEnumerable.Instance;

			return map.GetMobilesInRange(m_Location, range);
		}

		public IPooledEnumerable GetClientsInRange(int range)
		{
			Map map = m_Map;

			if (map == null)
				return Map.NullEnumerable.Instance;

			return map.GetClientsInRange(m_Location, range);
		}

		private static ArrayList m_Hears;
		private static ArrayList m_OnSpeech;

		public virtual void SendGuildChat(string text)
		{
		}

		public virtual void SendAlliedChat(string text)
		{
		}

		public virtual void DoSpeech(string text, int[] keywords, MessageType type, int hue)
		{
			if (m_Deleted || Commands.Handle(this, text))
				return;

			int range = 15;

			switch (type)
			{
				case MessageType.Regular: m_SpeechHue = hue; break;
				case MessageType.Emote: m_EmoteHue = hue; break;
				case MessageType.Whisper: m_WhisperHue = hue; range = 1; break;
				case MessageType.Yell: m_YellHue = hue; range = 18; break;
				case MessageType.Guild:
					{
						SendGuildChat(text);
						return;
					}
				case MessageType.Alliance:
					{
						SendAlliedChat(text);
						return;
					}
				default: type = MessageType.Regular; break;
			}

			SpeechEventArgs regArgs = new SpeechEventArgs(this, text, type, hue, keywords);

			EventSink.InvokeSpeech(regArgs);
			m_Region.OnSpeech(regArgs);
			OnSaid(regArgs);

			if (regArgs.Blocked)
				return;

			text = regArgs.Speech;

			if (text == null || text.Length == 0)
				return;

			if (m_Hears == null)
				m_Hears = new ArrayList();
			else if (m_Hears.Count > 0)
				m_Hears.Clear();

			if (m_OnSpeech == null)
				m_OnSpeech = new ArrayList();
			else if (m_OnSpeech.Count > 0)
				m_OnSpeech.Clear();

			ArrayList hears = m_Hears;
			ArrayList onSpeech = m_OnSpeech;

			if (m_Map != null)
			{
				IPooledEnumerable eable = m_Map.GetObjectsInRange(m_Location, range);

				foreach (object o in eable)
				{
					if (o is Mobile)
					{
						Mobile heard = (Mobile)o;

						// wea: InLOS -> IsAudibleTo
						if (heard.CanSee(this) && (m_NoSpeechLOS || !heard.Player || heard.IsAudibleTo(this)))
						{
							if (heard.m_NetState != null)
								hears.Add(heard);

							if (heard.HandlesOnSpeech(this))
								onSpeech.Add(heard);

							for (int i = 0; i < heard.Items.Count; ++i)
							{
								Item item = (Item)heard.Items[i];

								if (item.HandlesOnSpeech)
									onSpeech.Add(item);

								if (item is Container)
									AddSpeechItemsFrom(onSpeech, (Container)item);
							}
						}
					}
					else if (o is Item)
					{
						if (((Item)o).HandlesOnSpeech)
							onSpeech.Add(o);

						if (o is Container)
							AddSpeechItemsFrom(onSpeech, (Container)o);
					}
				}

				//eable.Free();

				object mutateContext = null;
				string mutatedText = text;
				SpeechEventArgs mutatedArgs = null;

				if (MutateSpeech(hears, ref mutatedText, ref mutateContext))
					mutatedArgs = new SpeechEventArgs(this, mutatedText, type, hue, new int[0]);

				CheckSpeechManifest();

				ProcessDelta();

				Packet regp = null;
				Packet mutp = null;

				for (int i = 0; i < hears.Count; ++i)
				{
					Mobile heard = (Mobile)hears[i];

					if (mutatedArgs == null || !CheckHearsMutatedSpeech(heard, mutateContext))
					{
						heard.OnSpeech(regArgs);

						NetState ns = heard.NetState;

						if (ns != null)
						{
							if (regp == null)
								regp = Packet.Acquire(new UnicodeMessage(m_Serial, Body, type, hue, 3, m_Language, Name, text));

							ns.Send(regp);
						}
					}
					else
					{
						//heard.OnSpeech( mutatedArgs );

						NetState ns = heard.NetState;

						if (ns != null)
						{
							if (mutp == null)
								mutp = Packet.Acquire(new UnicodeMessage(m_Serial, Body, type, hue, 3, m_Language, Name, mutatedText));

							ns.Send(mutp);
						}
					}
				}

				Packet.Release(regp);
				Packet.Release(mutp);

				if (onSpeech.Count > 1)
					onSpeech.Sort(LocationComparer.GetInstance(this));

				for (int i = 0; i < onSpeech.Count; ++i)
				{
					object obj = onSpeech[i];

					if (obj is Mobile)
					{
						Mobile heard = (Mobile)obj;

						if (mutatedArgs == null || !CheckHearsMutatedSpeech(heard, mutateContext))
							heard.OnSpeech(regArgs);
						else
							heard.OnSpeech(mutatedArgs);
					}
					else
					{
						Item item = (Item)obj;

						item.OnSpeech(regArgs);
					}
				}
			}
		}

		private static VisibleDamageType m_VisibleDamageType;

		public static VisibleDamageType VisibleDamageType
		{
			get { return m_VisibleDamageType; }
			set { m_VisibleDamageType = value; }
		}

		private ArrayList m_DamageEntries;

		//Pix: added so that PlayerMobiles can clear their damage entries on death
		// (this is needed to make sure that after we distribute points in the kin system
		//  on death, we don't count damage from before the last death (if the next happens
		//  soon enough after))
		protected void ClearDamageEntries()
		{
			if (m_DamageEntries.Count > 0)
			{
				m_DamageEntries.Clear();
			}
		}
		public ArrayList DamageEntries
		{
			get { return m_DamageEntries; }
		}

		public static Mobile GetDamagerFrom(DamageEntry de)
		{
			return (de == null ? null : de.Damager);
		}

		public Mobile FindMostRecentDamager(bool allowSelf)
		{
			return GetDamagerFrom(FindMostRecentDamageEntry(allowSelf));
		}

		public DamageEntry FindMostRecentDamageEntry(bool allowSelf)
		{
			for (int i = m_DamageEntries.Count - 1; i >= 0; --i)
			{
				if (i >= m_DamageEntries.Count)
					continue;

				DamageEntry de = (DamageEntry)m_DamageEntries[i];

				if (de.HasExpired)
					m_DamageEntries.RemoveAt(i);
				else if (allowSelf || de.Damager != this)
					return de;
			}

			return null;
		}

		public Mobile FindLeastRecentDamager(bool allowSelf)
		{
			return GetDamagerFrom(FindLeastRecentDamageEntry(allowSelf));
		}

		public DamageEntry FindLeastRecentDamageEntry(bool allowSelf)
		{
			for (int i = 0; i < m_DamageEntries.Count; ++i)
			{
				if (i < 0)
					continue;

				DamageEntry de = (DamageEntry)m_DamageEntries[i];

				if (de.HasExpired)
				{
					m_DamageEntries.RemoveAt(i);
					--i;
				}
				else if (allowSelf || de.Damager != this)
				{
					return de;
				}
			}

			return null;
		}

		public Mobile FindMostTotalDamger(bool allowSelf)
		{
			return GetDamagerFrom(FindMostTotalDamageEntry(allowSelf));
		}

		public DamageEntry FindMostTotalDamageEntry(bool allowSelf)
		{
			DamageEntry mostTotal = null;

			for (int i = m_DamageEntries.Count - 1; i >= 0; --i)
			{
				if (i >= m_DamageEntries.Count)
					continue;

				DamageEntry de = (DamageEntry)m_DamageEntries[i];

				if (de.HasExpired)
					m_DamageEntries.RemoveAt(i);
				else if ((allowSelf || de.Damager != this) && (mostTotal == null || de.DamageGiven > mostTotal.DamageGiven))
					mostTotal = de;
			}

			return mostTotal;
		}

		public Mobile FindLeastTotalDamger(bool allowSelf)
		{
			return GetDamagerFrom(FindLeastTotalDamageEntry(allowSelf));
		}

		public DamageEntry FindLeastTotalDamageEntry(bool allowSelf)
		{
			DamageEntry mostTotal = null;

			for (int i = m_DamageEntries.Count - 1; i >= 0; --i)
			{
				if (i >= m_DamageEntries.Count)
					continue;

				DamageEntry de = (DamageEntry)m_DamageEntries[i];

				if (de.HasExpired)
					m_DamageEntries.RemoveAt(i);
				else if ((allowSelf || de.Damager != this) && (mostTotal == null || de.DamageGiven < mostTotal.DamageGiven))
					mostTotal = de;
			}

			return mostTotal;
		}

		public DamageEntry FindDamageEntryFor(Mobile m)
		{
			for (int i = m_DamageEntries.Count - 1; i >= 0; --i)
			{
				if (i >= m_DamageEntries.Count)
					continue;

				DamageEntry de = (DamageEntry)m_DamageEntries[i];

				if (de.HasExpired)
					m_DamageEntries.RemoveAt(i);
				else if (de.Damager == m)
					return de;
			}

			return null;
		}

		public virtual Mobile GetDamageMaster(Mobile damagee)
		{
			return null;
		}

		//Pix: Added this so DamageEntries can have special timeouts (for mobs like the Harrower)
		public virtual double DamageEntryExpireTimeSeconds
		{
			get { return 120.0; }
		}

		public virtual DamageEntry RegisterDamage(int amount, Mobile from)
		{
			DamageEntry de = FindDamageEntryFor(from);

			if (de == null)
			{
				de = new DamageEntry(from);
				de.ExpireDelay = TimeSpan.FromSeconds(DamageEntryExpireTimeSeconds);
			}

			de.DamageGiven += amount;
			de.LastDamage = DateTime.Now;

			m_DamageEntries.Remove(de);
			m_DamageEntries.Add(de);

			Mobile master = from.GetDamageMaster(this);

			if (master != null)
			{
				ArrayList list = de.Responsible;

				if (list == null)
					de.Responsible = list = new ArrayList();

				DamageEntry resp = null;

				for (int i = 0; i < list.Count; ++i)
				{
					DamageEntry check = (DamageEntry)list[i];

					if (check.Damager == master)
					{
						resp = check;
						break;
					}
				}

				if (resp == null)
					list.Add(resp = new DamageEntry(master));

				resp.DamageGiven += amount;
				resp.LastDamage = DateTime.Now;
			}

			return de;
		}

		private Mobile m_LastKiller;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile LastKiller
		{
			get { return m_LastKiller; }
			set { m_LastKiller = value; }
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile is <see cref="Damage">damaged</see>. It is called before <see cref="Hits">hit points</see> are lowered or the Mobile is <see cref="Kill">killed</see>.
		/// <seealso cref="Damage" />
		/// <seealso cref="Hits" />
		/// <seealso cref="Kill" />
		/// </summary>
		public virtual void OnDamage(int amount, Mobile from, bool willKill)
		{
		}

		public virtual void Damage(int amount)
		{
			Damage(amount, null);
		}

		public virtual bool CanBeDamaged()
		{
			return !m_Blessed;
		}

		public virtual void Damage(int amount, Mobile from)
		{
			if (!CanBeDamaged())
				return;

			if (!Region.OnDamage(this, ref amount))
				return;

			if (amount > 0)
			{
				int oldHits = Hits;
				int newHits = oldHits - amount;

				if (m_Spell != null)
					m_Spell.OnCasterHurt();

				//if ( m_Spell != null && m_Spell.State == SpellState.Casting )
				//	m_Spell.Disturb( DisturbType.Hurt, false, true );

				if (from != null)
					RegisterDamage(amount, from);

				DisruptiveAction();

				Paralyzed = false;

				switch (m_VisibleDamageType)
				{
					case VisibleDamageType.Related:
						{
							NetState ourState = m_NetState, theirState = (from == null ? null : from.m_NetState);

							if (ourState == null)
							{
								Mobile master = GetDamageMaster(from);

								if (master != null)
									ourState = master.m_NetState;
							}

							if (theirState == null && from != null)
							{
								Mobile master = from.GetDamageMaster(this);

								if (master != null)
									theirState = master.m_NetState;
							}

							if (amount > 0 && (ourState != null || theirState != null))
							{
								Packet p = null;// = new DamagePacket( this, amount );

								if (ourState != null)
								{
									bool newPacket = (ourState.Version != null && ourState.Version >= DamagePacket.Version);

									if (newPacket)
										p = Packet.Acquire(new DamagePacket(this, amount));
									else
										p = Packet.Acquire(new DamagePacketOld(this, amount));

									ourState.Send(p);
								}

								if (theirState != null && theirState != ourState)
								{
									bool newPacket = (theirState.Version != null && theirState.Version >= DamagePacket.Version);

									if (newPacket && (p == null || !(p is DamagePacket)))
									{
										Packet.Release(p);
										p = Packet.Acquire(new DamagePacket(this, amount));
									}
									else if (!newPacket && (p == null || !(p is DamagePacketOld)))
									{
										Packet.Release(p);
										p = Packet.Acquire(new DamagePacketOld(this, amount));
									}

									theirState.Send(p);
								}

								Packet.Release(p);
							}

							break;
						}
					case VisibleDamageType.Everyone:
						{
							SendDamageToAll(amount);
							break;
						}
				}

				OnDamage(amount, from, newHits < 0);

				if (newHits < 0)
				{
					m_LastKiller = from;

					Hits = 0;

					if (oldHits >= 0)
						Kill();
				}
				else
				{
					Hits = newHits;
				}
			}
		}

		public virtual void SendDamageToAll(int amount)
		{
			if (amount < 0)
				return;

			Map map = m_Map;

			if (map == null)
				return;

			IPooledEnumerable eable = map.GetClientsInRange(m_Location);

			Packet pNew = null;
			Packet pOld = null;

			foreach (NetState ns in eable)
			{
				if (ns.Mobile.CanSee(this))
				{
					bool newPacket = (ns.Version != null && ns.Version >= DamagePacket.Version);
					Packet p;

					if (newPacket)
					{
						if (pNew == null)
							pNew = Packet.Acquire(new DamagePacket(this, amount));

						p = pNew;
					}
					else
					{
						if (pOld == null)
							pOld = Packet.Acquire(new DamagePacketOld(this, amount));

						p = pOld;
					}

					ns.Send(p);
				}
			}

			Packet.Release(pNew);
			Packet.Release(pOld);

			eable.Free();
		}

		public void Heal(int amount)
		{
			if (!Alive || IsDeadBondedPet)
				return;

			if (!Region.OnHeal(this, ref amount))
				return;

			if ((Hits + amount) > HitsMax)
			{
				amount = HitsMax - Hits;
			}

			Hits += amount;

			if (amount > 0 && m_NetState != null)
				m_NetState.Send(new MessageLocalizedAffix(Serial.MinusOne, -1, MessageType.Label, 0x3B2, 3, 1008158, "", AffixType.Append | AffixType.System, amount.ToString(), ""));
		}

		public void UsedStuckMenu()
		{
			if (m_StuckMenuUses == null)
			{
				m_StuckMenuUses = new DateTime[2];
			}

			for (int i = 0; i < m_StuckMenuUses.Length; ++i)
			{
				if ((DateTime.Now - m_StuckMenuUses[i]) > TimeSpan.FromDays(1.0))
				{
					m_StuckMenuUses[i] = DateTime.Now;
					return;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Squelched
		{
			get
			{
				return m_Squelched;
			}
			set
			{
				m_Squelched = value;
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 31:
					{
						m_STRBonusCap = reader.ReadInt();
						goto case 30;
					}
				case 30:
					{
						int size = reader.ReadInt32();
						FlyIDs = new int[size];

						for (int i = 0; i < size; i++)
						{
							FlyIDs[i] = reader.ReadInt();
						}
						goto case 29;
					}
				case 29:
					{
						m_CanFly = reader.ReadBool();
						goto case 28;
					}

				case 28:
					{
						m_LastStatGain = reader.ReadDeltaTime();

						goto case 27;
					}
				case 27:
					{
						m_Flags = (MobileFlags)reader.ReadInt32();

						goto case 26;
					}
				case 26:
				case 25:
				case 24:
					{
						m_Corpse = reader.ReadItem() as Container;

						goto case 23;
					}
				case 23:
					{
						m_CreationTime = reader.ReadDateTime();

						goto case 22;
					}
				case 22: // Just removed followers
				case 21:
					{
						m_Stabled = reader.ReadMobileList();

						goto case 20;
					}
				case 20:
					{
						m_CantWalk = reader.ReadBool();

						goto case 19;
					}
				case 19: // Just removed variables
				case 18:
					{
						m_Virtues = new VirtueInfo(reader);

						goto case 17;
					}
				case 17:
					{
						m_Thirst = reader.ReadInt32();
						m_BAC = reader.ReadInt32();

						goto case 16;
					}
				case 16:
					{
						m_ShortTermMurders = reader.ReadInt32();

						if (version <= 24)
						{
							reader.ReadDateTime();
							reader.ReadDateTime();
						}

						goto case 15;
					}
				case 15:
					{
						if (version < 22)
							reader.ReadInt(); // followers

						m_FollowersMax = reader.ReadInt32();

						goto case 14;
					}
				case 14:
					{
						m_MagicDamageAbsorb = reader.ReadInt32();

						goto case 13;
					}
				case 13:
					{
						m_GuildFealty = reader.ReadMobile();

						goto case 12;
					}
				case 12:
					{
						m_Guild = reader.ReadGuild();

						goto case 11;
					}
				case 11:
					{
						m_DisplayGuildTitle = reader.ReadBool();

						goto case 10;
					}
				case 10:
					{
						m_CanSwim = reader.ReadBool();

						goto case 9;
					}
				case 9:
					{
						m_Squelched = reader.ReadBool();

						goto case 8;
					}
				case 8:
					{
						m_Holding = reader.ReadItem();

						goto case 7;
					}
				case 7:
					{
						m_VirtualArmor = reader.ReadInt32();

						goto case 6;
					}
				case 6:
					{
						m_BaseSoundID = reader.ReadInt32();

						goto case 5;
					}
				case 5:
					{
						m_DisarmReady = reader.ReadBool();
						m_StunReady = reader.ReadBool();

						goto case 4;
					}
				case 4:
					{
						if (version <= 25)
						{
							Poison.Deserialize(reader);

							/*if ( m_Poison != null )
							{
								m_PoisonTimer = new PoisonTimer( this );
								m_PoisonTimer.Start();
							}*/
						}

						goto case 3;
					}
				case 3:
					{
						m_StatCap = reader.ReadInt32();

						goto case 2;
					}
				case 2:
					{
						m_NameHue = reader.ReadInt32();

						goto case 1;
					}
				case 1:
					{
						m_Hunger = reader.ReadInt32();

						goto case 0;
					}
				case 0:
					{
						if (version < 21)
							m_Stabled = new ArrayList();

						if (version < 18)
							m_Virtues = new VirtueInfo();

						if (version < 11)
							m_DisplayGuildTitle = true;

						if (version < 3)
							m_StatCap = 225;

						if (version < 15)
						{
							m_Followers = 0;
							m_FollowersMax = 5;
						}

						m_Location = reader.ReadPoint3D();
						m_Body = new Body(reader.ReadInt32());
						m_Name = reader.ReadString();
						m_GuildTitle = reader.ReadString();
						m_Criminal = reader.ReadBool();
						m_Kills = reader.ReadInt32();
						m_SpeechHue = reader.ReadInt32();
						m_EmoteHue = reader.ReadInt32();
						m_WhisperHue = reader.ReadInt32();
						m_YellHue = reader.ReadInt32();
						m_Language = reader.ReadString();
						m_Female = reader.ReadBool();
						m_Warmode = reader.ReadBool();
						m_Hidden = reader.ReadBool();
						m_Direction = (Direction)reader.ReadByte();
						m_Hue = reader.ReadInt32();
						m_Str = reader.ReadInt32();
						m_Dex = reader.ReadInt32();
						m_Int = reader.ReadInt32();
						m_Hits = reader.ReadInt32();
						m_Stam = reader.ReadInt32();
						m_Mana = reader.ReadInt32();
						m_Map = reader.ReadMap();
						m_Blessed = reader.ReadBool();
						m_Fame = reader.ReadInt32();
						m_Karma = reader.ReadInt32();
						m_AccessLevel = (AccessLevel)reader.ReadByte();

						// Convert old bonus caps to 'no cap'
						if (version < 31)
							m_STRBonusCap = 0;

						// Convert old access levels to new access levels
						if (version < 31)
						{
							switch (m_AccessLevel)
							{
								case (AccessLevel)0: //OldPlayer = 0,
									{
										m_AccessLevel = AccessLevel.Player;
										break;
									}
								case (AccessLevel)1: //OldCounselor = 1,
									{
										m_AccessLevel = AccessLevel.Counselor;
										break;
									}
								case (AccessLevel)2: //OldGameMaster = 2,
									{
										m_AccessLevel = AccessLevel.GameMaster;
										break;
									}
								case (AccessLevel)3: //OldSeer = 3,
									{
										m_AccessLevel = AccessLevel.Seer;
										break;
									}
								case (AccessLevel)4: //OldAdministrator = 4,
									{
										m_AccessLevel = AccessLevel.Administrator;
										break;
									}
							}
						}

						m_Skills = new Skills(this, reader);

						int itemCount = reader.ReadInt32();

						m_Items = new ArrayList(itemCount);

						for (int i = 0; i < itemCount; ++i)
						{
							Item item = reader.ReadItem();

							if (item != null)
								m_Items.Add(item);
						}

						m_Player = reader.ReadBool();
						m_Title = reader.ReadString();
						m_Profile = reader.ReadString();
						m_ProfileLocked = reader.ReadBool();
						if (version <= 18)
						{
							/*m_LightLevel =*/
							reader.ReadInt();
							/*m_TotalGold =*/
							reader.ReadInt();
							/*m_TotalWeight =*/
							reader.ReadInt();
						}
						m_AutoPageNotify = reader.ReadBool();

						m_LogoutLocation = reader.ReadPoint3D();
						m_LogoutMap = reader.ReadMap();

						m_StrLock = (StatLockType)reader.ReadByte();
						m_DexLock = (StatLockType)reader.ReadByte();
						m_IntLock = (StatLockType)reader.ReadByte();

						m_StatMods = new ArrayList();

						if (reader.ReadBool())
						{
							m_StuckMenuUses = new DateTime[reader.ReadInt32()];

							for (int i = 0; i < m_StuckMenuUses.Length; ++i)
							{
								m_StuckMenuUses[i] = reader.ReadDateTime();
							}
						}
						else
						{
							m_StuckMenuUses = null;
						}

						if (m_Player && m_Map != Map.Internal)
						{
							m_LogoutLocation = m_Location;
							m_LogoutMap = m_Map;

							m_Map = Map.Internal;
						}

						if (m_Map != null)
							m_Map.OnEnter(this);

						if (m_Criminal)
						{
							if (m_ExpireCriminal == null)
								m_ExpireCriminal = new ExpireCriminalTimer(this);

							m_ExpireCriminal.Start();
						}

						if (ShouldCheckStatTimers)
							CheckStatTimers();

						if (!m_Player && m_Dex <= 100 && m_CombatTimer != null)
							m_CombatTimer.Priority = TimerPriority.FiftyMS;
						else if (m_CombatTimer != null)
							m_CombatTimer.Priority = TimerPriority.EveryTick;

						m_Region = Region.Find(m_Location, m_Map);

						m_Region.InternalEnter(this);

						//UpdateResistances();

						break;
					}
			}

			//Pix: special logic to ensure the DefensiveSpell lock in m_Actions exists if it should
			//Note protection is a different beast since there are timers that control
			// BeginAction/EndAction -- protection effects aren't serialized.
			if (MagicDamageAbsorb > 0 || MeleeDamageAbsorb > 0)
			{
				BeginAction(typeof(DefensiveSpell));
			}
		}

		public virtual bool ShouldCheckStatTimers { get { return true; } }

		public virtual void CheckStatTimers()
		{
			if (m_Deleted)
				return;

			if (Hits < HitsMax)
			{
				if (CanRegenHits)
				{
					if (m_HitsTimer == null)
						m_HitsTimer = new HitsTimer(this);

					m_HitsTimer.Start();
				}
				else if (m_HitsTimer != null)
				{
					m_HitsTimer.Stop();
				}
			}
			else
			{
				Hits = HitsMax;
			}

			if (Stam < StamMax)
			{
				if (CanRegenStam)
				{
					if (m_StamTimer == null)
						m_StamTimer = new StamTimer(this);

					m_StamTimer.Start();
				}
				else if (m_StamTimer != null)
				{
					m_StamTimer.Stop();
				}
			}
			else
			{
				Stam = StamMax;
			}

			if (Mana < ManaMax)
			{
				if (CanRegenMana)
				{
					if (m_ManaTimer == null)
						m_ManaTimer = new ManaTimer(this);

					m_ManaTimer.Start();
				}
				else if (m_ManaTimer != null)
				{
					m_ManaTimer.Stop();
				}
			}
			else
			{
				Mana = ManaMax;
			}
		}

		private DateTime m_CreationTime;

		public DateTime CreationTime
		{
			get
			{
				return m_CreationTime;
			}
			set
			{
				m_CreationTime = value;
			}
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.Write((int)31); // version

			writer.Write(m_STRBonusCap);

			//write flytile array
			writer.WriteInt32(FlyIDs.Length);

			for (int i = 0; i < FlyIDs.Length; i++)
			{
				writer.Write(FlyIDs[i]);
			}

			writer.Write(m_CanFly);

			writer.WriteDeltaTime(m_LastStatGain);

			writer.WriteInt32((int)m_Flags);

			writer.Write(m_Corpse);

			writer.Write(m_CreationTime);

			writer.WriteMobileList(m_Stabled, true);

			writer.Write(m_CantWalk);

			VirtueInfo.Serialize(writer, m_Virtues);

			writer.WriteInt32(m_Thirst);
			writer.WriteInt32(m_BAC);

			writer.WriteInt32(m_ShortTermMurders);
			//writer.Write( m_ShortTermElapse );
			//writer.Write( m_LongTermElapse );

			//writer.Write( m_Followers );
			writer.WriteInt32(m_FollowersMax);

			writer.WriteInt32(m_MagicDamageAbsorb);

			writer.Write(m_GuildFealty);

			writer.Write(m_Guild);

			writer.Write(m_DisplayGuildTitle);

			writer.Write(m_CanSwim);

			writer.Write(m_Squelched);

			writer.Write(m_Holding);

			writer.WriteInt32(m_VirtualArmor);

			writer.WriteInt32(m_BaseSoundID);

			writer.Write(m_DisarmReady);
			writer.Write(m_StunReady);

			//Poison.Serialize( m_Poison, writer );

			writer.WriteInt32(m_StatCap);

			writer.WriteInt32(m_NameHue);

			writer.WriteInt32(m_Hunger);

			writer.Write(m_Location);
			writer.WriteInt32((int)m_Body);
			writer.Write(m_Name);
			writer.Write(m_GuildTitle);
			writer.Write(m_Criminal);
			writer.WriteInt32(m_Kills);
			writer.WriteInt32(m_SpeechHue);
			writer.WriteInt32(m_EmoteHue);
			writer.WriteInt32(m_WhisperHue);
			writer.WriteInt32(m_YellHue);
			writer.Write(m_Language);
			writer.Write(m_Female);
			writer.Write(m_Warmode);
			writer.Write(m_Hidden);
			writer.Write((byte)m_Direction);
			writer.WriteInt32(m_Hue);
			writer.WriteInt32(m_Str);
			writer.WriteInt32(m_Dex);
			writer.WriteInt32(m_Int);
			writer.WriteInt32(m_Hits);
			writer.WriteInt32(m_Stam);
			writer.WriteInt32(m_Mana);

			writer.Write(m_Map);

			writer.Write(m_Blessed);
			writer.WriteInt32(m_Fame);
			writer.WriteInt32(m_Karma);
			writer.Write((byte)m_AccessLevel);
			m_Skills.Serialize(writer);

			writer.WriteInt32(m_Items.Count);

			for (int i = 0; i < m_Items.Count; ++i)
				writer.Write((Item)m_Items[i]);

			writer.Write(m_Player);
			writer.Write(m_Title);
			writer.Write(m_Profile);
			writer.Write(m_ProfileLocked);
			//writer.Write( m_LightLevel );
			//writer.Write( m_TotalGold );
			//writer.Write( m_TotalWeight );
			writer.Write(m_AutoPageNotify);

			writer.Write(m_LogoutLocation);
			writer.Write(m_LogoutMap);

			writer.Write((byte)m_StrLock);
			writer.Write((byte)m_DexLock);
			writer.Write((byte)m_IntLock);

			if (m_StuckMenuUses != null)
			{
				writer.Write(true);

				writer.WriteInt32(m_StuckMenuUses.Length);

				for (int i = 0; i < m_StuckMenuUses.Length; ++i)
				{
					writer.Write(m_StuckMenuUses[i]);
				}
			}
			else
			{
				writer.Write(false);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int LightLevel
		{
			get
			{
				return m_LightLevel;
			}
			set
			{
				if (m_LightLevel != value)
				{
					m_LightLevel = value;

					CheckLightLevels(false);

					/*if ( m_NetState != null )
						m_NetState.Send( new PersonalLightLevel( this ) );*/
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public string Profile
		{
			get
			{
				return m_Profile;
			}
			set
			{
				m_Profile = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool ProfileLocked
		{
			get
			{
				return m_ProfileLocked;
			}
			set
			{
				m_ProfileLocked = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public bool Player
		{
			get
			{
				return m_Player;
			}
			set
			{
				m_Player = value;
				InvalidateProperties();

				if (!m_Player && m_Dex <= 100 && m_CombatTimer != null)
					m_CombatTimer.Priority = TimerPriority.FiftyMS;
				else if (m_CombatTimer != null)
					m_CombatTimer.Priority = TimerPriority.EveryTick;

				CheckStatTimers();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Title
		{
			get
			{
				return m_Title;
			}
			set
			{
				m_Title = value;
				InvalidateProperties();
			}
		}

		public static string GetAccessLevelName(AccessLevel level)
		{
			switch (level)
			{
				//case AccessLevel.OldPlayer:
				//case AccessLevel.OldCounselor:
				//case AccessLevel.OldGameMaster:
				//case AccessLevel.OldSeer:
				//case AccessLevel.OldAdministrator: 
				//return "an obsolete access level";
				case AccessLevel.Player: return "a player";
				case AccessLevel.Reporter: return "a reporter";
				case AccessLevel.FightBroker: return "a fight broker";
				case AccessLevel.Counselor: return "a counselor";
				case AccessLevel.GameMaster: return "a game master";
				case AccessLevel.Seer: return "a seer";
				case AccessLevel.Administrator: return "an administrator";
				case AccessLevel.Owner: return "an owner";

				default:
				case AccessLevel.ReadOnly:
					return "an invalid access level";
			}
		}

		public virtual bool CanPaperdollBeOpenedBy(Mobile from)
		{
			return (Body.IsHuman || Body.IsGhost || IsBodyMod);
		}

		public virtual void GetChildContextMenuEntries(Mobile from, ArrayList list, Item item)
		{
		}

		public virtual void GetContextMenuEntries(Mobile from, ArrayList list)
		{
			if (m_Deleted)
				return;

			if (CanPaperdollBeOpenedBy(from))
				list.Add(new PaperdollEntry(this));

			if (from == this && Backpack != null && CanSee(Backpack) && CheckAlive(false))
				list.Add(new OpenBackpackEntry(this));
		}

		public void Internalize()
		{
			Map = Map.Internal;
		}

		public ArrayList Items
		{
			get
			{
				return m_Items;
			}
		}

		/// <summary>
		/// Overridable. Virtual event invoked when <paramref name="item" /> is <see cref="AddItem">added</see> from the Mobile, such as when it is equiped.
		/// <seealso cref="Items" />
		/// <seealso cref="OnItemRemoved" />
		/// </summary>
		public virtual void OnItemAdded(Item item)
		{
		}

		/// <summary>
		/// Overridable. Virtual event invoked when <paramref name="item" /> is <see cref="RemoveItem">removed</see> from the Mobile.
		/// <seealso cref="Items" />
		/// <seealso cref="OnItemAdded" />
		/// </summary>
		public virtual void OnItemRemoved(Item item)
		{
		}

		/// <summary>
		/// Overridable. Virtual event invoked when <paramref name="item" /> is becomes a child of the Mobile; it's worn or contained at some level of the Mobile's <see cref="Mobile.Backpack">backpack</see> or <see cref="Mobile.BankBox">bank box</see>
		/// <seealso cref="OnSubItemRemoved" />
		/// <seealso cref="OnItemAdded" />
		/// </summary>
		public virtual void OnSubItemAdded(Item item)
		{
		}

		/// <summary>
		/// Overridable. Virtual event invoked when <paramref name="item" /> is removed from the Mobile, its <see cref="Mobile.Backpack">backpack</see>, or its <see cref="Mobile.BankBox">bank box</see>.
		/// <seealso cref="OnSubItemAdded" />
		/// <seealso cref="OnItemRemoved" />
		/// </summary>
		public virtual void OnSubItemRemoved(Item item)
		{
		}

		public virtual void OnItemBounceCleared(Item item)
		{
		}

		public virtual void OnSubItemBounceCleared(Item item)
		{
		}

		public void AddItem(Item item, LootType type)
		{
			item.LootType = type;
			AddItem(item);
		}

		public void AddItem(Item item)
		{
			if (item == null || item.Deleted)
				return;

			if (item.Parent == this)
				return;
			else if (item.Parent is Mobile)
				((Mobile)item.Parent).RemoveItem(item);
			else if (item.Parent is Item)
				((Item)item.Parent).RemoveItem(item);
			else
				item.SendRemovePacket();

			item.Parent = this;
			item.Map = m_Map;

			m_Items.Add(item);

			if (!(item is BankBox))
			{
				TotalWeight += item.TotalWeight + item.PileWeight;
				TotalGold += item.TotalGold;
			}

			item.Delta(ItemDelta.Update);

			item.OnAdded(this);
			OnItemAdded(item);

			//if ( item.PhysicalResistance != 0 || item.FireResistance != 0 || item.ColdResistance != 0 ||
			//item.PoisonResistance != 0 || item.EnergyResistance != 0 )
			//UpdateResistances();
		}

		private static IWeapon m_DefaultWeapon;

		public static IWeapon DefaultWeapon
		{
			get
			{
				return m_DefaultWeapon;
			}
			set
			{
				m_DefaultWeapon = value;
			}
		}

		public Item RequestItem(Type type)
		{
			// see if they are carrying one of these
			foreach (Item ix in Items)
				if (ix.GetType() == type)
					return ix;

			return null;
		}

		public virtual bool ProcessItem(Item item)
		{
			return false;
		}

		public void RemoveItem(Item item)
		{
			if (item == null || m_Items == null)
				return;

			if (m_Items.Contains(item))
			{
				item.SendRemovePacket();

				int oldCount = m_Items.Count;

				m_Items.Remove(item);

				if (!(item is BankBox))
				{
					TotalWeight -= item.TotalWeight + item.PileWeight;
					TotalGold -= item.TotalGold;
				}

				item.Parent = null;

				item.OnRemoved(this);
				OnItemRemoved(item);

				//if ( item.PhysicalResistance != 0 || item.FireResistance != 0 || item.ColdResistance != 0 ||
				//item.PoisonResistance != 0 || item.EnergyResistance != 0 )
				//UpdateResistances();
			}
		}

		public virtual void Animate(int action, int frameCount, int repeatCount, bool forward, bool repeat, int delay)
		{
			Map map = m_Map;

			if (map != null)
			{
				ProcessDelta();

				Packet p = null;

				IPooledEnumerable eable = map.GetClientsInRange(m_Location);

				foreach (NetState state in eable)
				{
					if (state.Mobile.CanSee(this))
					{
						state.Mobile.ProcessDelta();

						if (p == null)
							p = Packet.Acquire(new MobileAnimation(this, action, frameCount, repeatCount, forward, repeat, delay));

						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void SendSound(int soundID)
		{
			if (soundID != -1 && m_NetState != null)
				Send(new PlaySound(soundID, this));
		}

		public void SendSound(int soundID, IPoint3D p)
		{
			if (soundID != -1 && m_NetState != null)
				Send(new PlaySound(soundID, p));
		}

		public virtual void PlaySound(int soundID)
		{
			if (soundID == -1)
				return;

			if (m_Map != null)
			{
				Packet p = null;

				IPooledEnumerable eable = m_Map.GetClientsInRange(m_Location);

				foreach (NetState state in eable)
				{
					if (state.Mobile.CanSee(this))
					{
						if (p == null)
							p = Packet.Acquire(new PlaySound(soundID, this));

						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}

		}

		[CommandProperty(AccessLevel.Counselor)]
		public Skills Skills
		{
			get
			{
				return m_Skills;
			}
			set
			{
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public AccessLevel AccessLevel
		{
			get
			{
				return m_AccessLevel;
			}
			set
			{
				AccessLevel oldValue = m_AccessLevel;

				// Don't allow changing access level to Owner.
				if (value > AccessLevel.Administrator) return;

				if (oldValue != value)
				{
					m_AccessLevel = value;

					Delta(MobileDelta.Noto);
					InvalidateProperties();

					SendMessage("Your access level has been changed. You are now {0}.", GetAccessLevelName(value));

					ClearScreen();
					SendEverything();

					OnAccessLevelChanged(oldValue);
				}
			}
		}

		public virtual void OnAccessLevelChanged(AccessLevel oldLevel)
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Fame
		{
			get
			{
				return m_Fame;
			}
			set
			{
				int oldValue = m_Fame;

				if (oldValue != value)
				{
					m_Fame = value;

					if (ShowFameTitle && (m_Player || m_Body.IsHuman) && (oldValue >= 10000) != (value >= 10000))
						InvalidateProperties();

					OnFameChange(oldValue);
				}
			}
		}

		public virtual void OnFameChange(int oldValue)
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Karma
		{
			get
			{
				return m_Karma;
			}
			set
			{
				int old = m_Karma;

				if (old != value)
				{
					m_Karma = value;
					OnKarmaChange(old);
				}
			}
		}

		public virtual void OnKarmaChange(int oldValue)
		{
		}

		// Mobile did something which should unhide him
		public virtual void RevealingAction()
		{
			if (m_Hidden && m_AccessLevel == AccessLevel.Player)
				Hidden = false;

			DisruptiveAction(); // Anything that unhides you will also distrupt meditation
		}

		public void SayTo(Mobile to, bool ascii, string text)
		{
			PrivateOverheadMessage(MessageType.Regular, m_SpeechHue, ascii, text, to.NetState);
		}

		public void SayTo(Mobile to, string text)
		{
			SayTo(to, false, text);
		}

		public void SayTo(Mobile to, string format, params object[] args)
		{
			SayTo(to, false, String.Format(format, args));
		}

		public void SayTo(Mobile to, bool ascii, string format, params object[] args)
		{
			SayTo(to, ascii, String.Format(format, args));
		}

		public void SayTo(Mobile to, int number)
		{
			to.Send(new MessageLocalized(m_Serial, Body, MessageType.Regular, m_SpeechHue, 3, number, Name, ""));
		}

		public void SayTo(Mobile to, int number, string args)
		{
			to.Send(new MessageLocalized(m_Serial, Body, MessageType.Regular, m_SpeechHue, 3, number, Name, args));
		}

		public void Say(bool ascii, string text)
		{
			PublicOverheadMessage(MessageType.Regular, m_SpeechHue, ascii, text);
		}

		public void Say(string text)
		{
			PublicOverheadMessage(MessageType.Regular, m_SpeechHue, false, text);
		}

		public void Say(string format, params object[] args)
		{
			Say(String.Format(format, args));
		}

		public void Say(int number, AffixType type, string affix, string args)
		{
			PublicOverheadMessage(MessageType.Regular, m_SpeechHue, number, type, affix, args);
		}

		public void Say(int number)
		{
			Say(number, "");
		}

		public void Say(int number, string args)
		{
			PublicOverheadMessage(MessageType.Regular, m_SpeechHue, number, args);
		}

		public void Emote(string text)
		{
			PublicOverheadMessage(MessageType.Emote, m_EmoteHue, false, text);
		}

		public void Emote(string format, params object[] args)
		{
			Emote(String.Format(format, args));
		}

		public void Emote(int number)
		{
			Emote(number, "");
		}

		public void Emote(int number, string args)
		{
			PublicOverheadMessage(MessageType.Emote, m_EmoteHue, number, args);
		}

		public void Whisper(string text)
		{
			PublicOverheadMessage(MessageType.Whisper, m_WhisperHue, false, text);
		}

		public void Whisper(string format, params object[] args)
		{
			Whisper(String.Format(format, args));
		}

		public void Whisper(int number)
		{
			Whisper(number, "");
		}

		public void Whisper(int number, string args)
		{
			PublicOverheadMessage(MessageType.Whisper, m_WhisperHue, number, args);
		}

		public void Yell(string text)
		{
			PublicOverheadMessage(MessageType.Yell, m_YellHue, false, text);
		}

		public void Yell(string format, params object[] args)
		{
			Yell(String.Format(format, args));
		}

		public void Yell(int number)
		{
			Yell(number, "");
		}

		public void Yell(int number, string args)
		{
			PublicOverheadMessage(MessageType.Yell, m_YellHue, number, args);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Blessed
		{
			get
			{
				return m_Blessed;
			}
			set
			{
				if (m_Blessed != value)
				{
					m_Blessed = value;
					Delta(MobileDelta.Flags);
				}
			}
		}

		public void SendRemovePacket()
		{
			SendRemovePacket(true);
		}

		public void SendRemovePacket(bool everyone)
		{
			if (m_Map != null)
			{
				Packet p = null;

				IPooledEnumerable eable = m_Map.GetClientsInRange(m_Location);

				foreach (NetState state in eable)
				{
					if (state != m_NetState && (everyone || !state.Mobile.CanSee(this)))
					{
						if (p == null)
							p = this.RemovePacket;

						state.Send(p);
					}
				}

				eable.Free();
			}
		}

		public void ClearScreen()
		{
			NetState ns = m_NetState;

			if (m_Map != null && ns != null)
			{
				IPooledEnumerable eable = m_Map.GetObjectsInRange(m_Location, Core.GlobalMaxUpdateRange);

				foreach (object o in eable)
				{
					if (o is Mobile)
					{
						Mobile m = (Mobile)o;

						if (m != this && Utility.InUpdateRange(m_Location, m.m_Location))
							ns.Send(m.RemovePacket);
					}
					else if (o is Item)
					{
						Item item = (Item)o;

						if (InRange(item.Location, item.GetUpdateRange(this)))
							ns.Send(item.RemovePacket);
					}
				}

				eable.Free();
			}
		}

		public bool Send(Packet p)
		{
			return Send(p, false);
		}

		public bool Send(Packet p, bool throwOnOffline)
		{
			if (m_NetState != null)
			{
				m_NetState.Send(p);
				return true;
			}
			else if (throwOnOffline)
			{
				throw new MobileNotConnectedException(this, "Packet could not be sent.");
			}
			else
			{
				return false;
			}
		}

		public bool SendHuePicker(HuePicker p)
		{
			return SendHuePicker(p, false);
		}

		public bool SendHuePicker(HuePicker p, bool throwOnOffline)
		{
			if (m_NetState != null)
			{
				p.SendTo(m_NetState);
				return true;
			}
			else if (throwOnOffline)
			{
				throw new MobileNotConnectedException(this, "Hue picker could not be sent.");
			}
			else
			{
				return false;
			}
		}

		// wea: Correctly remove gumps from the NetState object on CloseGump() (integrated RunUO fix).
		public Gump FindGump(Type type)
		{
			NetState ns = m_NetState;

			if (ns != null)
			{
				foreach (Gump gump in ns.Gumps)
				{
					if (type.IsAssignableFrom(gump.GetType()))
					{
						return gump;
					}
				}
			}

			return null;
		}

		public bool CloseGump(Type type)
		{
			if (m_NetState != null)
			{
				Gump gump = FindGump(type);
				if (gump != null)
				{
					m_NetState.Send(new CloseGump(gump.TypeID, 0));
					m_NetState.RemoveGump(gump);
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// CloseGumps - fix for newer clients not sending back closegump calls...
		/// </summary>
		/// <param name="type">type of gump to close</param>
		/// <returns></returns>
		public int CloseGumps(Type type)
		{
			int numberRemoved = 0;
			NetState ns = m_NetState;

			if (ns != null)
			{
				List<Gump> gumps = (List<Gump>)ns.Gumps;
				List<Gump> toremove = new List<Gump>();

				for (int i = 0; i < gumps.Count; ++i)
				{
					if (gumps[i].GetType() == type)
					{
						ns.Send(new CloseGump(gumps[i].TypeID, 0));
						toremove.Add(gumps[i]);
					}
				}

				for (int i = 0; i < toremove.Count; ++i)
				{
					try
					{
						gumps.Remove(toremove[i]);
						numberRemoved++;
					}
					catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
				}
			}

			return numberRemoved;
		}

		public bool CloseAllGumps()
		{
			NetState ns = m_NetState;

			if (ns != null)
			{
				//GumpCollection gumps = ns.Gumps;
				List<Gump> gumps = new List<Gump>(ns.Gumps);

				for (int i = 0; i < gumps.Count; ++i)
				{
					//send all gumps a button response 0 as well (0 is always cancel)
					gumps[i].OnResponse(ns, new RelayInfo(0, null, null));
					ns.Send(new CloseGump(gumps[i].TypeID, 0));
					ns.RemoveGump(gumps[i]);
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		public bool HasGump(Type type)
		{
			return HasGump(type, false);
		}

		public bool HasGump(Type type, bool throwOnOffline)
		{
			NetState ns = m_NetState;

			if (ns != null)
			{
				bool contains = false;
				//GumpCollection gumps = ns.Gumps;
				List<Gump> gumps = new List<Gump>(ns.Gumps);

				for (int i = 0; !contains && i < gumps.Count; ++i)
					contains = (gumps[i].GetType() == type);

				return contains;
			}
			else if (throwOnOffline)
			{
				throw new MobileNotConnectedException(this, "Mobile is not connected.");
			}
			else
			{
				return false;
			}
		}

		public bool SendGump(Gump g)
		{
			return SendGump(g, false);
		}

		public bool SendGump(Gump g, bool throwOnOffline)
		{
			if (m_NetState != null)
			{
				g.SendTo(m_NetState);
				return true;
			}
			else if (throwOnOffline)
			{
				throw new MobileNotConnectedException(this, "Gump could not be sent.");
			}
			else
			{
				return false;
			}
		}

		public bool SendMenu(IMenu m)
		{
			return SendMenu(m, false);
		}

		public bool SendMenu(IMenu m, bool throwOnOffline)
		{
			if (m_NetState != null)
			{
				m.SendTo(m_NetState);
				return true;
			}
			else if (throwOnOffline)
			{
				throw new MobileNotConnectedException(this, "Menu could not be sent.");
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Overridable. Event invoked before the Mobile says something.
		/// <seealso cref="DoSpeech" />
		/// </summary>
		public virtual void OnSaid(SpeechEventArgs e)
		{
			if (m_Squelched)
			{
				this.SendLocalizedMessage(500168); // You can not say anything, you have been squelched.
				e.Blocked = true;
			}

			if (!e.Blocked)
				RevealingAction();
		}

		public virtual bool HandlesOnSpeech(Mobile from)
		{
			return false;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile hears speech. This event will only be invoked if <see cref="HandlesOnSpeech" /> returns true.
		/// <seealso cref="DoSpeech" />
		/// </summary>
		public virtual void OnSpeech(SpeechEventArgs e)
		{
		}

		public void SendEverything()
		{
			NetState ns = m_NetState;

			if (m_Map != null && ns != null)
			{
				IPooledEnumerable eable = m_Map.GetObjectsInRange(m_Location, Core.GlobalMaxUpdateRange);

				foreach (object o in eable)
				{
					if (o is Item)
					{
						Item item = (Item)o;

						if (CanSee(item) && InRange(item.Location, item.GetUpdateRange(this)))
							item.SendInfoTo(ns);
					}
					else if (o is Mobile)
					{
						Mobile m = (Mobile)o;

						if (CanSee(m) && Utility.InUpdateRange(m_Location, m.m_Location))
						{
							ns.Send(new MobileIncoming(this, m));

							if (m.IsDeadBondedPet)
								ns.Send(new BondedStatus(0, m.m_Serial, 1));

							if (ObjectPropertyList.Enabled)
							{
								ns.Send(m.OPLPacket);

								//foreach ( Item item in m.m_Items )
								//	ns.Send( item.OPLPacket );
							}
						}
					}
				}

				eable.Free();
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Map Map
		{
			get
			{
				return m_Map;
			}
			set
			{
				if (m_Deleted)
					return;

				if (m_Map != value)
				{
					if (m_NetState != null)
						m_NetState.ValidateAllTrades();

					Map oldMap = m_Map;

					if (m_Map != null)
					{
						m_Map.OnLeave(this);

						ClearScreen();
						SendRemovePacket();
					}

					for (int i = 0; i < m_Items.Count; ++i)
						((Item)m_Items[i]).Map = value;

					m_Map = value;

					if (m_Map != null)
						m_Map.OnEnter(this);

					m_Region.InternalExit(this);

					NetState ns = m_NetState;

					if (m_Map != null)
					{
						Region old = m_Region;
						m_Region = Region.Find(m_Location, m_Map);
						OnRegionChange(old, m_Region);
						m_Region.InternalEnter(this);

						if (ns != null && m_Map != null)
						{
							ns.Sequence = 0;
							ns.Send(new MapChange(this));
							ns.Send(new MapPatches());
							ns.Send(SeasonChange.Instantiate(GetSeason(), true));
							ns.Send(new MobileUpdate(this));
							ClearFastwalkStack();
						}
					}

					if (ns != null)
					{
						if (m_Map != null)
							Send(new ServerChange(this, m_Map));

						ns.Sequence = 0;
						ClearFastwalkStack();

						Send(new MobileIncoming(this, this));
						Send(new MobileUpdate(this));
						CheckLightLevels(true);
						Send(new MobileUpdate(this));
					}

					SendEverything();
					SendIncomingPacket();

					if (ns != null)
					{
						ns.Sequence = 0;
						ClearFastwalkStack();

						Send(new MobileIncoming(this, this));
						Send(SupportedFeatures.Instantiate(ns.Account));
						Send(new MobileUpdate(this));
						Send(new MobileAttributes(this));
					}

					OnMapChange(oldMap);
				}
			}
		}

		public void ForceRegionReEnter(bool Exit)
		{//forces to find the region again and enter it
			if (m_Deleted)
				return;

			Region n = Region.Find(m_Location, m_Map);
			if (n != m_Region)
			{
				if (Exit)
					m_Region.InternalExit(this);
				m_Region = n;
				OnRegionChange(n, m_Region);
				m_Region.InternalEnter(this);
				CheckLightLevels(false);
			}
		}

		/// <summary>
		/// Overridable. Virtual event invoked when <see cref="Map" /> changes.
		/// </summary>
		protected virtual void OnMapChange(Map oldMap)
		{
		}

		public virtual bool CanBeBeneficial(Mobile target)
		{
			return CanBeBeneficial(target, true, false);
		}

		public virtual bool CanBeBeneficial(Mobile target, bool message)
		{
			return CanBeBeneficial(target, message, false);
		}

		public virtual bool CanBeBeneficial(Mobile target, bool message, bool allowDead)
		{
			if (target == null)
				return false;

			if (m_Deleted || target.m_Deleted || !Alive || IsDeadBondedPet || (!allowDead && (!target.Alive || IsDeadBondedPet)))
			{
				if (message)
					SendLocalizedMessage(1001017); // You can not perform beneficial acts on your target.

				return false;
			}

			if (target == this)
				return true;

			if ( /*m_Player &&*/ !Region.AllowBenificial(this, target))
			{
				// TODO: Pets
				//if ( !(target.m_Player || target.Body.IsHuman || target.Body.IsAnimal) )
				//{
				if (message)
					SendLocalizedMessage(1001017); // You can not perform beneficial acts on your target.

				return false;
				//}
			}

			return true;
		}

		public virtual bool IsBeneficialCriminal(Mobile target)
		{
			if (this == target)
				return false;

			int n = Notoriety.Compute(this, target);

			return (n == Notoriety.Criminal || n == Notoriety.Murderer);
		}

		/// <summary>
		/// Overridable. Event invoked when the Mobile <see cref="DoBeneficial">does a beneficial action</see>.
		/// </summary>
		public virtual void OnBeneficialAction(Mobile target, bool isCriminal)
		{
			if (isCriminal)
				CriminalAction(false);
		}

		public virtual void DoBeneficial(Mobile target)
		{
			if (target == null)
				return;

			OnBeneficialAction(target, IsBeneficialCriminal(target));

			Region.OnBenificialAction(this, target);
			target.Region.OnGotBenificialAction(this, target);
		}

		public virtual bool BeneficialCheck(Mobile target)
		{
			if (CanBeBeneficial(target, true))
			{
				DoBeneficial(target);
				return true;
			}

			return false;
		}

		public virtual bool CanBeHarmful(Mobile target)
		{
			return CanBeHarmful(target, true);
		}

		public virtual bool CanBeHarmful(Mobile target, bool message)
		{
			return CanBeHarmful(target, message, false);
		}

		// wea: added overloaded version to handle instances where damage can be dealt 
		// after the incurrer's death

		public virtual bool CanBeHarmful(Mobile target, bool message, bool ignoreOurBlessedness)
		{
			return CanBeHarmful(target, message, ignoreOurBlessedness, false);
		}

		public virtual bool CanBeHarmful(Mobile target, bool message, bool ignoreOurBlessedness, bool ignoreOurDeadness)
		{
			if (target == null)
				return false;

			if (m_Deleted || (!ignoreOurDeadness && !Alive) || (!ignoreOurBlessedness && m_Blessed) || target.m_Deleted || target.m_Blessed || IsDeadBondedPet || !target.Alive || target.IsDeadBondedPet)
			{
				if (message)
					SendLocalizedMessage(1001018); // You can not perform negative acts on your target.

				return false;
			}

			if (target == this)
				return true;

			// TODO: Pets
			if ( /*m_Player &&*/ !Region.AllowHarmful(this, target))//(target.m_Player || target.Body.IsHuman) && !Region.AllowHarmful( this, target )  )
			{
				if (message)
					SendLocalizedMessage(1001018); // You can not perform negative acts on your target.

				return false;
			}

			return true;
		}

		public virtual int Luck
		{
			get { return 0; }
		}

		public virtual bool IsHarmfulCriminal(Mobile target)
		{
			if (this == target)
				return false;

			return (Notoriety.Compute(this, target) == Notoriety.Innocent);
		}

		/// <summary>
		/// Overridable. Event invoked when the Mobile <see cref="DoHarmful">does a harmful action</see>.
		/// </summary>
		public virtual void OnHarmfulAction(Mobile target, bool isCriminal)
		{
			if (isCriminal)
				CriminalAction(false);
		}

		public virtual void DoHarmful(Mobile target)
		{
			DoHarmful(target, false);
		}

		public virtual void DoHarmful(Mobile target, bool indirect)
		{
			if (target == null)
				return;

			bool isCriminal = IsHarmfulCriminal(target);

			OnHarmfulAction(target, isCriminal);
			target.AggressiveAction(this, isCriminal);

			Region.OnDidHarmful(this, target);
			target.Region.OnGotHarmful(this, target);

			if (!indirect)
				Combatant = target;

			if (m_ExpireCombatant == null)
				m_ExpireCombatant = new ExpireCombatantTimer(this);
			else
				m_ExpireCombatant.Stop();

			m_ExpireCombatant.Start();
		}

		public virtual bool HarmfulCheck(Mobile target)
		{
			if (CanBeHarmful(target))
			{
				DoHarmful(target);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets <see cref="System.Collections.ArrayList">a list</see> of all <see cref="StatMod">StatMod's</see> currently active for the Mobile.
		/// </summary>
		public ArrayList StatMods { get { return m_StatMods; } }

		protected void RemoveSkillModsOfType(Type type)
		{
			ArrayList al = new ArrayList();
			foreach (object o in m_SkillMods)
			{
				if (o.GetType() == type)
				{
					al.Add(o);
				}
			}
			foreach (object o in al)
			{
				m_SkillMods.Remove(o);
			}
		}

		public void ClearStatMods()
		{
			while (m_StatMods.Count > 0)
			{
				StatMod check = (StatMod)m_StatMods[0];

				m_StatMods.RemoveAt(0);
				CheckStatTimers();
				Delta(MobileDelta.Stat | GetStatDelta(check.Type));
			}

			if (StatChange != null)
				StatChange(this, StatType.All);
		}

		public bool RemoveStatMod(string name)
		{
			for (int i = 0; i < m_StatMods.Count; ++i)
			{
				StatMod check = (StatMod)m_StatMods[i];

				if (check.Name == name)
				{
					m_StatMods.RemoveAt(i);
					CheckStatTimers();
					Delta(MobileDelta.Stat | GetStatDelta(check.Type));
					if (StatChange != null)
						StatChange(this, check.Type);
					return true;
				}
			}

			return false;
		}

		public StatMod GetStatMod(string name)
		{
			for (int i = 0; i < m_StatMods.Count; ++i)
			{
				StatMod check = (StatMod)m_StatMods[i];

				if (check.Name == name)
					return check;
			}

			return null;
		}

		public void AddStatMod(StatMod mod)
		{
			for (int i = 0; i < m_StatMods.Count; ++i)
			{
				StatMod check = (StatMod)m_StatMods[i];

				if (check.Name == mod.Name)
				{
					Delta(MobileDelta.Stat | GetStatDelta(check.Type));
					m_StatMods.RemoveAt(i);
					break;
				}
			}

			m_StatMods.Add(mod);
			Delta(MobileDelta.Stat | GetStatDelta(mod.Type));
			CheckStatTimers();

			if (StatChange != null)
				StatChange(this, mod.Type);
		}

		private MobileDelta GetStatDelta(StatType type)
		{
			MobileDelta delta = 0;

			if ((type & StatType.Str) != 0)
				delta |= MobileDelta.Hits;

			if ((type & StatType.Dex) != 0)
				delta |= MobileDelta.Stam;

			if ((type & StatType.Int) != 0)
				delta |= MobileDelta.Mana;

			return delta;
		}

		/// <summary>
		/// Computes the total modified offset for the specified stat type. Expired <see cref="StatMod" /> instances are removed.
		/// </summary>
		public double GetStatOffset(StatType type)
		{
			double offset = 0;

			for (int i = 0; i < m_StatMods.Count; ++i)
			{
				StatMod mod = (StatMod)m_StatMods[i];

				if (mod.HasElapsed())
				{
					m_StatMods.RemoveAt(i);
					Delta(MobileDelta.Stat | GetStatDelta(mod.Type));
					CheckStatTimers();
					if (StatChange != null)
						StatChange(this, mod.Type);

					--i;
				}
				else if ((mod.Type & type) != 0)
				{
					offset += mod.Offset;
				}
			}

			return offset;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the <see cref="RawStr" /> changes.
		/// <seealso cref="RawStr" />
		/// <seealso cref="OnRawStatChange" />
		/// </summary>
		public virtual void OnRawStrChange(int oldValue)
		{
		}

		/// <summary>
		/// Overridable. Virtual event invoked when <see cref="RawDex" /> changes.
		/// <seealso cref="RawDex" />
		/// <seealso cref="OnRawStatChange" />
		/// </summary>
		public virtual void OnRawDexChange(int oldValue)
		{
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the <see cref="RawInt" /> changes.
		/// <seealso cref="RawInt" />
		/// <seealso cref="OnRawStatChange" />
		/// </summary>
		public virtual void OnRawIntChange(int oldValue)
		{
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the <see cref="RawStr" />, <see cref="RawDex" />, or <see cref="RawInt" /> changes.
		/// <seealso cref="OnRawStrChange" />
		/// <seealso cref="OnRawDexChange" />
		/// <seealso cref="OnRawIntChange" />
		/// </summary>
		public virtual void OnRawStatChange(StatType stat, int oldValue)
		{
			if (StatChange != null)
				StatChange(this, stat);
		}

		/// <summary>
		/// Gets or sets the base, unmodified, strength of the Mobile. Ranges from 1 to 65000, inclusive.
		/// <seealso cref="Str" />
		/// <seealso cref="StatMod" />
		/// <seealso cref="OnRawStrChange" />
		/// <seealso cref="OnRawStatChange" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int RawStr
		{
			get
			{
				return m_Str;
			}
			set
			{
				if (value < 1) value = 1;
				else if (value > 65000) value = 65000;

				if (m_Str != value)
				{
					int oldValue = m_Str;

					m_Str = value;
					Delta(MobileDelta.Stat | MobileDelta.Hits);

					if (Hits < HitsMax)
					{
						if (m_HitsTimer == null)
							m_HitsTimer = new HitsTimer(this);

						m_HitsTimer.Start();
					}
					else if (Hits > HitsMax)
					{
						Hits = HitsMax;
					}

					OnRawStrChange(oldValue);
					OnRawStatChange(StatType.Str, oldValue);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int STRBonusCap
		{
			get { return m_STRBonusCap; }
			set { m_STRBonusCap = value; }
		}

		/// <summary>
		/// Gets or sets the effective strength of the Mobile. This is the sum of the <see cref="RawStr" /> plus any additional modifiers. Any attempts to set this value when under the influence of a <see cref="StatMod" /> will result in no change. It ranges from 1 to 65000, inclusive.
		/// <seealso cref="RawStr" />
		/// <seealso cref="StatMod" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int Str
		{
			get
			{
				int value = m_Str + (int)GetStatOffset(StatType.Str);

				if (value < 1) value = 1;
				else if (value > 65000) value = 65000;

				return value;
			}
			set
			{
				if (m_StatMods.Count == 0)
					RawStr = value;
			}
		}

		public virtual int StrMax
		{
			get
			{
				return 100;
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets or sets the base, unmodified, dexterity of the Mobile. Ranges from 1 to 65000, inclusive.
		/// <seealso cref="Dex" />
		/// <seealso cref="StatMod" />
		/// <seealso cref="OnRawDexChange" />
		/// <seealso cref="OnRawStatChange" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int RawDex
		{
			get
			{
				return m_Dex;
			}
			set
			{
				if (value < 1) value = 1;
				else if (value > 65000) value = 65000;

				if (m_Dex != value)
				{
					int oldValue = m_Dex;

					m_Dex = value;
					Delta(MobileDelta.Stat | MobileDelta.Stam);

					if (Stam < StamMax)
					{
						if (m_StamTimer == null)
							m_StamTimer = new StamTimer(this);

						m_StamTimer.Start();
					}
					else if (Stam > StamMax)
					{
						Stam = StamMax;
					}

					OnRawDexChange(oldValue);
					OnRawStatChange(StatType.Dex, oldValue);
				}
			}
		}

		/// <summary>
		/// Gets or sets the effective dexterity of the Mobile. This is the sum of the <see cref="RawDex" /> plus any additional modifiers. Any attempts to set this value when under the influence of a <see cref="StatMod" /> will result in no change. It ranges from 1 to 65000, inclusive.
		/// <seealso cref="RawDex" />
		/// <seealso cref="StatMod" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int Dex
		{
			get
			{
				int value = m_Dex + (int)GetStatOffset(StatType.Dex);

				if (value < 1) value = 1;
				else if (value > 65000) value = 65000;

				return value;
			}
			set
			{
				if (m_StatMods.Count == 0)
					RawDex = value;
			}
		}

		public virtual int DexMax
		{
			get
			{
				return 100;
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets or sets the base, unmodified, intelligence of the Mobile. Ranges from 1 to 65000, inclusive.
		/// <seealso cref="Int" />
		/// <seealso cref="StatMod" />
		/// <seealso cref="OnRawIntChange" />
		/// <seealso cref="OnRawStatChange" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int RawInt
		{
			get
			{
				return m_Int;
			}
			set
			{
				if (value < 1) value = 1;
				else if (value > 65000) value = 65000;

				if (m_Int != value)
				{
					int oldValue = m_Int;

					m_Int = value;
					Delta(MobileDelta.Stat | MobileDelta.Mana);

					if (Mana < ManaMax)
					{
						if (m_ManaTimer == null)
							m_ManaTimer = new ManaTimer(this);

						m_ManaTimer.Start();
					}
					else if (Mana > ManaMax)
					{
						Mana = ManaMax;
					}

					OnRawIntChange(oldValue);
					OnRawStatChange(StatType.Int, oldValue);
				}
			}
		}

		/// <summary>
		/// Gets or sets the effective intelligence of the Mobile. This is the sum of the <see cref="RawInt" /> plus any additional modifiers. Any attempts to set this value when under the influence of a <see cref="StatMod" /> will result in no change. It ranges from 1 to 65000, inclusive.
		/// <seealso cref="RawInt" />
		/// <seealso cref="StatMod" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int Int
		{
			get
			{
				int value = m_Int + (int)GetStatOffset(StatType.Int);

				if (value < 1) value = 1;
				else if (value > 65000) value = 65000;

				return value;
			}
			set
			{
				if (m_StatMods.Count == 0)
					RawInt = value;
			}
		}

		public virtual int IntMax
		{
			get
			{
				return 100;
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets or sets the current hit point of the Mobile. This value ranges from 0 to <see cref="HitsMax" />, inclusive. When set to the value of <see cref="HitsMax" />, the <see cref="AggressorInfo.CanReportMurder">CanReportMurder</see> flag of all aggressors is reset to false, and the list of damage entries is cleared.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int Hits
		{
			get
			{
				return m_Hits;
			}
			set
			{
				if (m_Deleted)
					return;

				if (value < 0)
				{
					value = 0;
				}
				else if (value >= HitsMax)
				{
					value = HitsMax;

					if (m_HitsTimer != null)
						m_HitsTimer.Stop();

					for (int i = 0; i < m_Aggressors.Count; i++)//reset reports on full HP
						((AggressorInfo)m_Aggressors[i]).CanReportMurder = false;

					if (m_DamageEntries.Count > 0)
						m_DamageEntries.Clear(); // reset damage entries on full HP
				}

				if (value < HitsMax)
				{
					if (CanRegenHits)
					{
						if (m_HitsTimer == null)
							m_HitsTimer = new HitsTimer(this);

						m_HitsTimer.Start();
					}
					else if (m_HitsTimer != null)
					{
						m_HitsTimer.Stop();
					}
				}

				if (m_Hits != value)
				{
					m_Hits = value;
					Delta(MobileDelta.Hits);
				}
			}
		}

		/// <summary>
		/// Overridable. Gets the maximum hit point of the Mobile. By default, this returns: <c>50 + (<see cref="Str" /> / 2)</c>
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int HitsMax
		{
			get
			{
				return 50 + (Str / 2);
			}
		}

		/// <summary>
		/// Gets or sets the current stamina of the Mobile. This value ranges from 0 to <see cref="StamMax" />, inclusive.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int Stam
		{
			get
			{
				return m_Stam;
			}
			set
			{
				if (m_Deleted)
					return;

				if (value < 0)
				{
					value = 0;
				}
				else if (value >= StamMax)
				{
					value = StamMax;

					if (m_StamTimer != null)
						m_StamTimer.Stop();
				}

				if (value < StamMax)
				{
					if (CanRegenStam)
					{
						if (m_StamTimer == null)
							m_StamTimer = new StamTimer(this);

						m_StamTimer.Start();
					}
					else if (m_StamTimer != null)
					{
						m_StamTimer.Stop();
					}
				}

				if (m_Stam != value)
				{
					m_Stam = value;
					Delta(MobileDelta.Stam);
				}
			}
		}

		/// <summary>
		/// Overridable. Gets the maximum stamina of the Mobile. By default, this returns: <c><see cref="Dex" /></c>
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int StamMax
		{
			get
			{
				return Dex;
			}
		}

		/// <summary>
		/// Gets or sets the current stamina of the Mobile. This value ranges from 0 to <see cref="ManaMax" />, inclusive.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int Mana
		{
			get
			{
				return m_Mana;
			}
			set
			{
				if (m_Deleted)
					return;

				if (value < 0)
				{
					value = 0;
				}
				else if (value >= ManaMax)
				{
					value = ManaMax;

					if (m_ManaTimer != null)
						m_ManaTimer.Stop();

					if (Meditating)
					{
						Meditating = false;
						SendLocalizedMessage(501846); // You are at peace.
					}
				}

				if (value < ManaMax)
				{
					if (CanRegenMana)
					{
						if (m_ManaTimer == null)
							m_ManaTimer = new ManaTimer(this);

						m_ManaTimer.Start();
					}
					else if (m_ManaTimer != null)
					{
						m_ManaTimer.Stop();
					}
				}

				if (m_Mana != value)
				{
					m_Mana = value;
					Delta(MobileDelta.Mana);
				}
			}
		}

		/// <summary>
		/// Overridable. Gets the maximum mana of the Mobile. By default, this returns: <c><see cref="Int" /></c>
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ManaMax
		{
			get
			{
				return Int;
			}
		}

		public virtual int HuedItemID
		{
			get
			{
				return (m_Female ? 0x2107 : 0x2106);
			}
		}

		private int m_HueMod = -1;

		[Hue, CommandProperty(AccessLevel.GameMaster)]
		public int HueMod
		{
			get
			{
				return m_HueMod;
			}
			set
			{
				if (m_HueMod != value)
				{
					m_HueMod = value;

					Delta(MobileDelta.Hue);
				}
			}
		}

		[Hue, CommandProperty(AccessLevel.GameMaster)]
		public virtual int Hue
		{
			get
			{
				if (m_HueMod != -1)
					return m_HueMod;

				return m_Hue;
			}
			set
			{
				int oldHue = m_Hue;

				if (oldHue != value)
				{
					m_Hue = value;

					Delta(MobileDelta.Hue);
				}
			}
		}


		public void SetDirection(Direction dir)
		{
			m_Direction = dir;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Direction Direction
		{
			get
			{
				return m_Direction;
			}
			set
			{
				if (m_Direction != value)
				{
					m_Direction = value;

					Delta(MobileDelta.Direction);
					//ProcessDelta();
				}
			}
		}

		public virtual int GetSeason()
		{
			if (m_Map != null)
				return m_Map.Season;

			return 1;
		}

		public virtual int GetPacketFlags()
		{
			int flags = 0x0;

			if (m_Female)
				flags |= 0x02;

			if (m_Poison != null)
				flags |= 0x04;

			if (m_Blessed || m_YellowHealthbar)
				flags |= 0x08;

			if (m_Warmode)
				flags |= 0x40;

			if (m_Hidden)
				flags |= 0x80;

			return flags;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Female
		{
			get
			{
				return m_Female;
			}
			set
			{
				if (m_Female != value)
				{
					m_Female = value;
					Delta(MobileDelta.Flags);
					OnGenderChanged(!m_Female);
				}
			}
		}

		public virtual void OnGenderChanged(bool oldFemale)
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Warmode
		{
			get
			{
				return m_Warmode;
			}
			set
			{
				if (m_Deleted)
					return;

				if (m_Warmode != value)
				{
					if (m_AutoManifestTimer != null)
					{
						m_AutoManifestTimer.Stop();
						m_AutoManifestTimer = null;
					}

					m_Warmode = value;
					Delta(MobileDelta.Flags);

					if (m_NetState != null)
						Send(SetWarMode.Instantiate(value));

					if (!m_Warmode)
						Combatant = null;

					if (!Alive)
					{
						if (value)
							Delta(MobileDelta.GhostUpdate);
						else
							SendRemovePacket(false);
					}
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Hidden
		{
			get
			{
				return m_Hidden;
			}
			set
			{
				if (m_Hidden != value)
				{
					m_AllowedStealthSteps = 0;

					m_Hidden = value;
					//Delta( MobileDelta.Flags );

					if (m_Map != null)
					{
						Packet p = null;

						IPooledEnumerable eable = m_Map.GetClientsInRange(m_Location);

						foreach (NetState state in eable)
						{
							if (!state.Mobile.CanSee(this))
							{
								if (p == null)
									p = this.RemovePacket;

								state.Send(p);
							}
							else
							{
								state.Send(new MobileIncoming(state.Mobile, this));

								if (IsDeadBondedPet)
									state.Send(new BondedStatus(0, m_Serial, 1));

								if (ObjectPropertyList.Enabled)
								{
									state.Send(OPLPacket);

									//foreach ( Item item in m_Items )
									//	state.Send( item.OPLPacket );
								}
							}
						}

						eable.Free();
					}
				}
			}
		}

		public virtual void OnConnected()
		{
		}

		public virtual void OnDisconnected()
		{
		}

		public virtual void OnNetStateChanged()
		{
		}

		public NetState NetState
		{
			get
			{
				if (m_NetState != null && m_NetState.Socket == null)
					NetState = null;

				return m_NetState;
			}
			set
			{
				if (m_NetState != value)
				{
					if (m_Map != null)
						m_Map.OnClientChange(m_NetState, value, this);

					if (m_Target != null)
						m_Target.Cancel(this, TargetCancelType.Disconnected);

					if (m_QuestArrow != null)
						QuestArrow = null;

					if (m_Spell != null)
						m_Spell.OnConnectionChanged();

					//if ( m_Spell != null )
					//	m_Spell.FinishSequence();

					if (m_NetState != null)
						m_NetState.CancelAllTrades();

					try
					{
						CloseAllGumps();
					}
					catch (Exception e)
					{
						Console.WriteLine("Send to Zen wierd gump magic error");
						Console.WriteLine(e.Message);
						Console.WriteLine(e.StackTrace);
					}

					BankBox box = FindBankNoCreate();

					if (box != null && box.Opened)
						box.Close();

					// REMOVED:
					//m_Actions.Clear();

					m_NetState = value;

					if (m_NetState == null)
					{
						OnDisconnected();
						EventSink.InvokeDisconnected(new DisconnectedEventArgs(this));

						// Disconnected, start the logout timer

						if (m_LogoutTimer == null)
							m_LogoutTimer = new LogoutTimer(this);
						else
							m_LogoutTimer.Stop();

						m_LogoutTimer.Delay = GetLogoutDelay();
						m_LogoutTimer.Start();
					}
					else
					{
						OnConnected();
						EventSink.InvokeConnected(new ConnectedEventArgs(this));

						// Connected, stop the logout timer and if needed, move to the world

						if (m_LogoutTimer != null)
							m_LogoutTimer.Stop();

						m_LogoutTimer = null;

						if (m_Map == Map.Internal && m_LogoutMap != null)
						{
							Map = m_LogoutMap;
							Location = m_LogoutLocation;
						}
					}

					for (int i = m_Items.Count - 1; i >= 0; --i)
					{
						if (i >= m_Items.Count)
							continue;

						Item item = (Item)m_Items[i];

						if (item is SecureTradeContainer)
						{
							for (int j = item.Items.Count - 1; j >= 0; --j)
							{
								if (j < item.Items.Count)
								{
									((Item)item.Items[j]).OnSecureTrade(this, this, this, false);
									AddToBackpack((Item)item.Items[j]);
								}
							}

							item.Delete();
						}
					}

					DropHolding();
					OnNetStateChanged();
				}
			}
		}

		public virtual bool CanSee(object o)
		{
			if (o is Item)
			{
				return CanSee((Item)o);
			}
			else if (o is Mobile)
			{
				return CanSee((Mobile)o);
			}
			else
			{
				return true;
			}
		}

		public virtual bool CanSee(Item item)
		{
			if (m_Map == Map.Internal)
				return false;
			else if (item.Map == Map.Internal)
				return false;

			if (item.Parent != null)
			{
				if (item.Parent is Item)
				{
					if (!CanSee((Item)item.Parent))
						return false;
				}
				else if (item.Parent is Mobile)
				{
					if (!CanSee((Mobile)item.Parent))
						return false;
				}
			}

			if (item is BankBox)
			{
				BankBox box = item as BankBox;

				if (box != null && m_AccessLevel <= AccessLevel.Counselor && (box.Owner != this || !box.Opened))
					return false;
			}
			else if (item is SecureTradeContainer)
			{
				SecureTrade trade = ((SecureTradeContainer)item).Trade;

				if (trade != null && trade.From.Mobile != this && trade.To.Mobile != this)
					return false;
			}

			return !item.Deleted && item.Map == m_Map && (item.Visible || m_AccessLevel > AccessLevel.Counselor);
		}

		public virtual bool CanSee(Mobile m)
		{
			if (m_Deleted || m.m_Deleted || m_Map == Map.Internal || m.m_Map == Map.Internal)
				return false;

			return this == m || (
				m.m_Map == m_Map &&
				(!m.Hidden || m_AccessLevel > m.AccessLevel) &&
				(m.Alive || !Alive || m_AccessLevel > AccessLevel.Player || m.Warmode));
		}

		public virtual bool CanBeRenamedBy(Mobile from)
		{
			// Counselors cannot rename players
			if (from.m_AccessLevel == AccessLevel.Counselor && this.Player == true)
				return false;

			return (from.m_AccessLevel > m_AccessLevel);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Language
		{
			get
			{
				return m_Language;
			}
			set
			{
				m_Language = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SpeechHue
		{
			get
			{
				return m_SpeechHue;
			}
			set
			{
				m_SpeechHue = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int EmoteHue
		{
			get
			{
				return m_EmoteHue;
			}
			set
			{
				m_EmoteHue = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int WhisperHue
		{
			get
			{
				return m_WhisperHue;
			}
			set
			{
				m_WhisperHue = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int YellHue
		{
			get
			{
				return m_YellHue;
			}
			set
			{
				m_YellHue = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string GuildTitle
		{
			get
			{
				return m_GuildTitle;
			}
			set
			{
				string old = m_GuildTitle;

				if (old != value)
				{
					m_GuildTitle = value;

					if (m_Guild != null && !m_Guild.Disbanded && m_GuildTitle != null)
						this.SendLocalizedMessage(1018026, true, m_GuildTitle); // Your guild title has changed :

					InvalidateProperties();

					OnGuildTitleChange(old);
				}
			}
		}

		public virtual void OnGuildTitleChange(string oldTitle)
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DisplayGuildTitle
		{
			get
			{
				return m_DisplayGuildTitle;
			}
			set
			{
				m_DisplayGuildTitle = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile GuildFealty
		{
			get
			{
				return m_GuildFealty;
			}
			set
			{
				m_GuildFealty = value;
			}
		}

		private string m_NameMod;

		[CommandProperty(AccessLevel.GameMaster)]
		public string NameMod
		{
			get
			{
				return m_NameMod;
			}
			set
			{
				if (m_NameMod != value)
				{
					m_NameMod = value;
					Delta(MobileDelta.Name);
					InvalidateProperties();
				}
			}
		}

		private bool m_YellowHealthbar;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool YellowHealthbar
		{
			get
			{
				return m_YellowHealthbar;
			}
			set
			{
				m_YellowHealthbar = value;
				Delta(MobileDelta.Flags);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string RawName
		{
			get { return m_Name; }
			set { Name = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Name
		{
			get
			{
				if (m_NameMod != null)
					return m_NameMod;

				return m_Name;
			}
			set
			{
				if (m_Name != value) // I'm leaving out the && m_NameMod == null
				{
					m_Name = value;
					Delta(MobileDelta.Name);
					InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastStatGain
		{
			get
			{
				return m_LastStatGain;
			}
			set
			{
				m_LastStatGain = value;
			}
		}

		public BaseGuild Guild
		{
			get
			{
				return m_Guild;
			}
			set
			{
				BaseGuild old = m_Guild;

				if (old != value)
				{
					if (value == null)
						GuildTitle = null;

					m_Guild = value;

					Delta(MobileDelta.Noto);
					InvalidateProperties();

					OnGuildChange(old);
				}
			}
		}

		public virtual void OnGuildChange(BaseGuild oldGuild)
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Poison Poison
		{
			get
			{
				return m_Poison;
			}
			set
			{
				/*if ( m_Poison != value && (m_Poison == null || value == null || m_Poison.Level < value.Level) )
				{*/
				m_Poison = value;
				Delta(MobileDelta.Flags);

				if (m_PoisonTimer != null)
				{
					m_PoisonTimer.Stop();
					m_PoisonTimer = null;
				}

				if (m_Poison != null)
				{
					m_PoisonTimer = m_Poison.ConstructTimer(this);

					if (m_PoisonTimer != null)
						m_PoisonTimer.Start();
				}

				CheckStatTimers();
				/*}*/
			}
		}

		/// <summary>
		/// Overridable. Event invoked when a call to <see cref="ApplyPoison" /> failed because <see cref="CheckPoisonImmunity" /> returned false: the Mobile was resistant to the poison. By default, this broadcasts an overhead message: * The poison seems to have no effect. *
		/// <seealso cref="CheckPoisonImmunity" />
		/// <seealso cref="ApplyPoison" />
		/// <seealso cref="Poison" />
		/// </summary>
		public virtual void OnPoisonImmunity(Mobile from, Poison poison)
		{
			this.PublicOverheadMessage(MessageType.Emote, 0x3B2, 1005534); // * The poison seems to have no effect. *
		}

		/// <summary>
		/// Overridable. Virtual event invoked when a call to <see cref="ApplyPoison" /> failed because <see cref="CheckHigherPoison" /> returned false: the Mobile was already poisoned by an equal or greater strength poison.
		/// <seealso cref="CheckHigherPoison" />
		/// <seealso cref="ApplyPoison" />
		/// <seealso cref="Poison" />
		/// </summary>
		public virtual void OnHigherPoison(Mobile from, Poison poison)
		{
		}

		/// <summary>
		/// Overridable. Event invoked when a call to <see cref="ApplyPoison" /> succeeded. By default, this broadcasts an overhead message varying by the level of the poison. Example: * Zippy begins to spasm uncontrollably. *
		/// <seealso cref="ApplyPoison" />
		/// <seealso cref="Poison" />
		/// </summary>
		public virtual void OnPoisoned(Mobile from, Poison poison, Poison oldPoison)
		{
			if (poison != null)
			{
				this.LocalOverheadMessage(MessageType.Regular, 0x22, 1042857 + (poison.Level * 2));
				this.NonlocalOverheadMessage(MessageType.Regular, 0x22, 1042858 + (poison.Level * 2), Name);
			}
		}

		/// <summary>
		/// Overridable. Called from <see cref="ApplyPoison" />, this method checks if the Mobile is immune to some <see cref="Poison" />. If true, <see cref="OnPoisonImmunity" /> will be invoked and <see cref="ApplyPoisonResult.Immune" /> is returned.
		/// <seealso cref="OnPoisonImmunity" />
		/// <seealso cref="ApplyPoison" />
		/// <seealso cref="Poison" />
		/// </summary>
		public virtual bool CheckPoisonImmunity(Mobile from, Poison poison)
		{
			return false;
		}

		/// <summary>
		/// Overridable. Called from <see cref="ApplyPoison" />, this method checks if the Mobile is already poisoned by some <see cref="Poison" /> of equal or greater strength. If true, <see cref="OnHigherPoison" /> will be invoked and <see cref="ApplyPoisonResult.HigherPoisonActive" /> is returned.
		/// <seealso cref="OnHigherPoison" />
		/// <seealso cref="ApplyPoison" />
		/// <seealso cref="Poison" />
		/// </summary>
		public virtual bool CheckHigherPoison(Mobile from, Poison poison)
		{
			return (m_Poison != null && m_Poison.Level >= poison.Level);
		}

		/// <summary>
		/// Overridable. Attempts to apply poison to the Mobile. Checks are made such that no <see cref="CheckHigherPoison">higher poison is active</see> and that the Mobile is not <see cref="CheckPoisonImmunity">immune to the poison</see>. Provided those assertions are true, the <paramref name="poison" /> is applied and <see cref="OnPoisoned" /> is invoked.
		/// <seealso cref="Poison" />
		/// <seealso cref="CurePoison" />
		/// </summary>
		/// <returns>One of four possible values:
		/// <list type="table">
		/// <item>
		/// <term><see cref="ApplyPoisonResult.Cured">Cured</see></term>
		/// <description>The <paramref name="poison" /> parameter was null and so <see cref="CurePoison" /> was invoked.</description>
		/// </item>
		/// <item>
		/// <term><see cref="ApplyPoisonResult.HigherPoisonActive">HigherPoisonActive</see></term>
		/// <description>The call to <see cref="CheckHigherPoison" /> returned false.</description>
		/// </item>
		/// <item>
		/// <term><see cref="ApplyPoisonResult.Immune">Immune</see></term>
		/// <description>The call to <see cref="CheckPoisonImmunity" /> returned false.</description>
		/// </item>
		/// <item>
		/// <term><see cref="ApplyPoisonResult.Poisoned">Poisoned</see></term>
		/// <description>The <paramref name="poison" /> was successfully applied.</description>
		/// </item>
		/// </list>
		/// </returns>
		public virtual ApplyPoisonResult ApplyPoison(Mobile from, Poison poison)
		{
			if (poison == null)
			{
				CurePoison(from);
				return ApplyPoisonResult.Cured;
			}

			if (CheckHigherPoison(from, poison))
			{
				OnHigherPoison(from, poison);
				return ApplyPoisonResult.HigherPoisonActive;
			}

			if (CheckPoisonImmunity(from, poison))
			{
				OnPoisonImmunity(from, poison);
				return ApplyPoisonResult.Immune;
			}

			Poison oldPoison = m_Poison;
			this.Poison = poison;

			OnPoisoned(from, poison, oldPoison);

			return ApplyPoisonResult.Poisoned;
		}

		/// <summary>
		/// Overridable. Called from <see cref="CurePoison" />, this method checks to see that the Mobile can be cured of <see cref="Poison" />
		/// <seealso cref="CurePoison" />
		/// <seealso cref="Poison" />
		/// </summary>
		public virtual bool CheckCure(Mobile from)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when a call to <see cref="CurePoison" /> succeeded.
		/// <seealso cref="CurePoison" />
		/// <seealso cref="CheckCure" />
		/// <seealso cref="Poison" />
		/// </summary>
		public virtual void OnCured(Mobile from, Poison oldPoison)
		{
		}

		/// <summary>
		/// Overridable. Virtual event invoked when a call to <see cref="CurePoison" /> failed.
		/// <seealso cref="CurePoison" />
		/// <seealso cref="CheckCure" />
		/// <seealso cref="Poison" />
		/// </summary>
		public virtual void OnFailedCure(Mobile from)
		{
		}

		/// <summary>
		/// Overridable. Attempts to cure any poison that is currently active.
		/// </summary>
		/// <returns>True if poison was cured, false if otherwise.</returns>
		public virtual bool CurePoison(Mobile from)
		{
			if (CheckCure(from))
			{
				Poison oldPoison = m_Poison;
				this.Poison = null;

				OnCured(from, oldPoison);

				return true;
			}

			OnFailedCure(from);

			return false;
		}

		public virtual void OnBeforeSpawn(Point3D location, Map m)
		{
		}

		public virtual void OnAfterSpawn()
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Poisoned
		{
			get
			{
				return (m_Poison != null);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsBodyMod
		{
			get
			{
				return (m_BodyMod.BodyID != 0);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Body BodyMod
		{
			get
			{
				return m_BodyMod;
			}
			set
			{
				if (m_BodyMod != value)
				{
					m_BodyMod = value;

					Delta(MobileDelta.Body);
					InvalidateProperties();

					CheckStatTimers();
				}
			}
		}

		private static int[] m_InvalidBodies = new int[]
			{
				32,
				95,
				156,
				197,
				198,
		};

		[Body, CommandProperty(AccessLevel.GameMaster)]
		public Body Body
		{
			get
			{
				if (IsBodyMod)
					return m_BodyMod;

				return m_Body;
			}
			set
			{
				if (m_Body != value && !IsBodyMod)
				{
					m_Body = SafeBody(value);

					Delta(MobileDelta.Body);
					InvalidateProperties();

					CheckStatTimers();
				}
			}
		}

		public virtual int SafeBody(int body)
		{
			int delta = -1;

			for (int i = 0; delta < 0 && i < m_InvalidBodies.Length; ++i)
				delta = (m_InvalidBodies[i] - body);

			if (delta != 0)
				return body;

			return 0;
		}

		[Body, CommandProperty(AccessLevel.GameMaster)]
		public int BodyValue
		{
			get
			{
				return Body.BodyID;
			}
			set
			{
				Body = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public Serial Serial
		{
			get
			{
				return m_Serial;
			}
		}

		Point3D IEntity.Location
		{
			get
			{
				return m_Location;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Point3D Location
		{
			get
			{
				return m_Location;
			}
			set
			{
				SetLocation(value, true);
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Point3D LogoutLocation
		{
			get
			{
				return m_LogoutLocation;
			}
			set
			{
				m_LogoutLocation = value;
			}
		}

		public Map LogoutMap
		{
			get
			{
				return m_LogoutMap;
			}
			set
			{
				m_LogoutMap = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Region Region
		{
			get
			{
				return m_Region;
			}
		}

		public void FreeCache()
		{
			Packet.Release(ref m_RemovePacket);
			Packet.Release(ref m_PropertyList);
			Packet.Release(ref m_OPLPacket);
		}

		private Packet m_RemovePacket;

		public Packet RemovePacket
		{
			get
			{
				if (m_RemovePacket == null)
				{
					m_RemovePacket = new RemoveMobile(this);
					m_RemovePacket.SetStatic();
				}

				return m_RemovePacket;
			}
		}

		private Packet m_OPLPacket;

		public Packet OPLPacket
		{
			get
			{
				if (m_OPLPacket == null)
					m_OPLPacket = new OPLInfo(PropertyList);

				return m_OPLPacket;
			}
		}

		private ObjectPropertyList m_PropertyList;

		public ObjectPropertyList PropertyList
		{
			get
			{
				if (m_PropertyList == null)
				{
					m_PropertyList = new ObjectPropertyList(this);

					GetProperties(m_PropertyList);

					m_PropertyList.Terminate();
				}

				return m_PropertyList;
			}
		}

		public void ClearProperties()
		{
			Packet.Release(ref m_PropertyList);
			Packet.Release(ref m_OPLPacket);
		}

		public void InvalidateProperties()
		{
			if (!Core.AOS)
				return;

			if (m_Map != null && m_Map != Map.Internal && !World.Loading)
			{
				ObjectPropertyList oldList = m_PropertyList;
				Packet.Release(ref m_PropertyList);
				ObjectPropertyList newList = PropertyList;

				if (oldList == null || oldList.Hash != newList.Hash)
				{
					Packet.Release(ref m_OPLPacket);
					Delta(MobileDelta.Properties);
				}
			}
			else
			{
				ClearProperties();
			}
		}

		private int m_SolidHueOverride = -1;

		[CommandProperty(AccessLevel.GameMaster)]
		public int SolidHueOverride
		{
			get { return m_SolidHueOverride; }
			set { if (m_SolidHueOverride == value) return; m_SolidHueOverride = value; Delta(MobileDelta.Hue | MobileDelta.Body); }
		}

		public virtual void MoveToWorld(Point3D newLocation, Map map)
		{
			if (m_Map == map)
			{
				SetLocation(newLocation, true);
				return;
			}

			BankBox box = FindBankNoCreate();

			if (box != null && box.Opened)
				box.Close();

			Point3D oldLocation = m_Location;
			Map oldMap = m_Map;

			Region oldRegion = m_Region;

			if (oldMap != null)
			{
				oldMap.OnLeave(this);

				ClearScreen();
				SendRemovePacket();
			}

			for (int i = 0; i < m_Items.Count; ++i)
				((Item)m_Items[i]).Map = map;

			m_Map = map;

			m_Region.InternalExit(this);

			m_Location = newLocation;

			NetState ns = m_NetState;

			if (m_Map != null)
			{
				m_Map.OnEnter(this);

				m_Region = Region.Find(m_Location, m_Map);
				OnRegionChange(oldRegion, m_Region);

				m_Region.InternalEnter(this);

				if (ns != null && m_Map != null)
				{
					ns.Sequence = 0;
					ns.Send(new MapChange(this));
					ns.Send(new MapPatches());
					ns.Send(SeasonChange.Instantiate(GetSeason(), true));
					ns.Send(new MobileUpdate(this));
					ClearFastwalkStack();
				}
			}

			if (ns != null)
			{
				if (m_Map != null)
					Send(new ServerChange(this, m_Map));

				ns.Sequence = 0;
				ClearFastwalkStack();

				Send(new MobileIncoming(this, this));
				Send(new MobileUpdate(this));
				CheckLightLevels(true);
				Send(new MobileUpdate(this));
			}

			SendEverything();
			SendIncomingPacket();

			if (ns != null)
			{
				m_NetState.Sequence = 0;
				ClearFastwalkStack();

				Send(new MobileIncoming(this, this));
				Send(SupportedFeatures.Instantiate(ns.Account));
				Send(new MobileUpdate(this));
				Send(new MobileAttributes(this));
			}

			OnMapChange(oldMap);
			OnLocationChange(oldLocation);

			m_Region.OnLocationChanged(this, oldLocation);
		}

		public virtual void SetLocation(Point3D newLocation, bool isTeleport)
		{
			if (m_Deleted)
				return;

			Point3D oldLocation = m_Location;
			Region oldRegion = m_Region;

			if (oldLocation != newLocation)
			{
				m_Location = newLocation;

				BankBox box = FindBankNoCreate();

				if (box != null && box.Opened)
					box.Close();

				if (m_NetState != null)
					m_NetState.ValidateAllTrades();

				if (m_Map != null)
					m_Map.OnMove(oldLocation, this);

				if (isTeleport && m_NetState != null)
				{
					m_NetState.Sequence = 0;
					m_NetState.Send(new MobileUpdate(this));
					ClearFastwalkStack();
				}

				Map map = m_Map;

				if (map != null)
				{
					// First, send a remove message to everyone who can no longer see us. (inOldRange && !inNewRange)
					Packet removeThis = null;

					IPooledEnumerable eable = map.GetClientsInRange(oldLocation);

					foreach (NetState ns in eable)
					{
						if (ns != m_NetState && !Utility.InUpdateRange(newLocation, ns.Mobile.Location))
						{
							if (removeThis == null)
								removeThis = this.RemovePacket;

							ns.Send(removeThis);
						}
					}

					eable.Free();

					NetState ourState = m_NetState;

					// Check to see if we are attached to a client
					if (ourState != null)
					{
						eable = map.GetObjectsInRange(newLocation, Core.GlobalMaxUpdateRange);

						// We are attached to a client, so it's a bit more complex. We need to send new items and people to ourself, and ourself to other clients
						foreach (object o in eable)
						{
							if (o is Item)
							{
								Item item = (Item)o;

								int range = item.GetUpdateRange(this);
								Point3D loc = item.Location;

								if (!Utility.InRange(oldLocation, loc, range) && Utility.InRange(newLocation, loc, range) && CanSee(item))
									item.SendInfoTo(ourState);
							}
							else if (o != this && o is Mobile)
							{
								Mobile m = (Mobile)o;

								if (!Utility.InUpdateRange(newLocation, m.m_Location))
									continue;

								bool inOldRange = Utility.InUpdateRange(oldLocation, m.m_Location);

								if ((isTeleport || !inOldRange) && m.m_NetState != null && m.CanSee(this))
								{
									m.m_NetState.Send(new MobileIncoming(m, this));

									if (IsDeadBondedPet)
										m.m_NetState.Send(new BondedStatus(0, m_Serial, 1));

									if (ObjectPropertyList.Enabled)
									{
										m.m_NetState.Send(OPLPacket);

										//foreach ( Item item in m_Items )
										//	m.m_NetState.Send( item.OPLPacket );
									}
								}

								if (!inOldRange && CanSee(m))
								{
									ourState.Send(new MobileIncoming(this, m));

									if (m.IsDeadBondedPet)
										ourState.Send(new BondedStatus(0, m.m_Serial, 1));

									if (ObjectPropertyList.Enabled)
									{
										ourState.Send(m.OPLPacket);

										//foreach ( Item item in m.m_Items )
										//	ourState.Send( item.OPLPacket );
									}
								}
							}
						}

						eable.Free();
					}
					else
					{
						eable = map.GetClientsInRange(newLocation);

						// We're not attached to a client, so simply send an Incoming
						foreach (NetState ns in eable)
						{
							if ((isTeleport || !Utility.InUpdateRange(oldLocation, ns.Mobile.Location)) && ns.Mobile.CanSee(this))
							{
								ns.Send(new MobileIncoming(ns.Mobile, this));

								if (IsDeadBondedPet)
									ns.Send(new BondedStatus(0, m_Serial, 1));

								if (ObjectPropertyList.Enabled)
								{
									ns.Send(OPLPacket);

									//foreach ( Item item in m_Items )
									//	ns.Send( item.OPLPacket );
								}
							}
						}

						eable.Free();
					}
				}

				m_Region = Region.Find(m_Location, m_Map);

				if (oldRegion != m_Region)
				{
					oldRegion.InternalExit(this);
					m_Region.InternalEnter(this);
					OnRegionChange(oldRegion, m_Region);
				}

				OnLocationChange(oldLocation);

				CheckLightLevels(false);

				m_Region.OnLocationChanged(this, oldLocation);
			}
		}

		/// <summary>
		/// Overridable. Virtual event invoked when <see cref="Location" /> changes.
		/// </summary>
		protected virtual void OnLocationChange(Point3D oldLocation)
		{
		}

		private Item m_Hair, m_Beard;

		[CommandProperty(AccessLevel.GameMaster)]
		public Item Hair
		{
			get
			{
				if (m_Hair != null && !m_Hair.Deleted && m_Hair.Parent == this)
					return m_Hair;

				return m_Hair = FindItemOnLayer(Layer.Hair);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Item Beard
		{
			get
			{
				if (m_Beard != null && !m_Beard.Deleted && m_Beard.Parent == this)
					return m_Beard;

				return m_Beard = FindItemOnLayer(Layer.FacialHair);
			}
		}

		public bool HasFreeHand()
		{
			return FindItemOnLayer(Layer.TwoHanded) == null;
		}

		private IWeapon m_Weapon;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual IWeapon Weapon
		{
			get
			{
				Item item = m_Weapon as Item;

				if (item != null && !item.Deleted && item.Parent == this && CanSee(item))
					return m_Weapon;

				m_Weapon = null;

				item = FindItemOnLayer(Layer.OneHanded);

				if (item == null)
					item = FindItemOnLayer(Layer.TwoHanded);

				if (item is IWeapon)
					return (m_Weapon = (IWeapon)item);
				else
					return GetDefaultWeapon();
			}
		}

		public virtual IWeapon GetDefaultWeapon()
		{
			return m_DefaultWeapon;
		}

		private BankBox m_BankBox;

		[CommandProperty(AccessLevel.GameMaster)]
		public BankBox BankBox
		{
			get
			{
				if (m_BankBox != null && !m_BankBox.Deleted && m_BankBox.Parent == this)
					return m_BankBox;

				m_BankBox = FindItemOnLayer(Layer.Bank) as BankBox;

				if (m_BankBox == null)
					AddItem(m_BankBox = new BankBox(this));

				return m_BankBox;
			}
		}

		public BankBox FindBankNoCreate()
		{
			if (m_BankBox != null && !m_BankBox.Deleted && m_BankBox.Parent == this)
				return m_BankBox;

			m_BankBox = FindItemOnLayer(Layer.Bank) as BankBox;

			return m_BankBox;
		}

		private Container m_Backpack;

		[CommandProperty(AccessLevel.GameMaster)]
		public Container Backpack
		{
			get
			{
				if (m_Backpack != null && !m_Backpack.Deleted && m_Backpack.Parent == this)
					return m_Backpack;

				return (m_Backpack = (FindItemOnLayer(Layer.Backpack) as Container));
			}
		}

		public virtual bool KeepsItemsOnDeath { get { return m_AccessLevel > AccessLevel.Player; } }

		public Item FindItemOnLayer(Layer layer)
		{
			ArrayList eq = m_Items;
			int count = eq.Count;

			for (int i = 0; i < count; ++i)
			{
				Item item = (Item)eq[i];

				if (!item.Deleted && item.Layer == layer)
				{
					return item;
				}
			}

			return null;
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int X
		{
			get { return m_Location.m_X; }
			set { Location = new Point3D(value, m_Location.m_Y, m_Location.m_Z); }
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Y
		{
			get { return m_Location.m_Y; }
			set { Location = new Point3D(m_Location.m_X, value, m_Location.m_Z); }
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Z
		{
			get { return m_Location.m_Z; }
			set { Location = new Point3D(m_Location.m_X, m_Location.m_Y, value); }
		}

		public void MovingEffect(IEntity to, int itemID, int speed, int duration, bool fixedDirection, bool explodes, int hue, int renderMode)
		{
			Effects.SendMovingEffect(this, to, itemID, speed, duration, fixedDirection, explodes, hue, renderMode);
		}

		public void MovingEffect(IEntity to, int itemID, int speed, int duration, bool fixedDirection, bool explodes)
		{
			Effects.SendMovingEffect(this, to, itemID, speed, duration, fixedDirection, explodes, 0, 0);
		}

		public void MovingParticles(IEntity to, int itemID, int speed, int duration, bool fixedDirection, bool explodes, int hue, int renderMode, int effect, int explodeEffect, int explodeSound, EffectLayer layer, int unknown)
		{
			Effects.SendMovingParticles(this, to, itemID, speed, duration, fixedDirection, explodes, hue, renderMode, effect, explodeEffect, explodeSound, layer, unknown);
		}

		public void MovingParticles(IEntity to, int itemID, int speed, int duration, bool fixedDirection, bool explodes, int hue, int renderMode, int effect, int explodeEffect, int explodeSound, int unknown)
		{
			Effects.SendMovingParticles(this, to, itemID, speed, duration, fixedDirection, explodes, hue, renderMode, effect, explodeEffect, explodeSound, (EffectLayer)255, unknown);
		}

		public void MovingParticles(IEntity to, int itemID, int speed, int duration, bool fixedDirection, bool explodes, int effect, int explodeEffect, int explodeSound, int unknown)
		{
			Effects.SendMovingParticles(this, to, itemID, speed, duration, fixedDirection, explodes, effect, explodeEffect, explodeSound, unknown);
		}

		public void MovingParticles(IEntity to, int itemID, int speed, int duration, bool fixedDirection, bool explodes, int effect, int explodeEffect, int explodeSound)
		{
			Effects.SendMovingParticles(this, to, itemID, speed, duration, fixedDirection, explodes, 0, 0, effect, explodeEffect, explodeSound, 0);
		}

		public void FixedEffect(int itemID, int speed, int duration, int hue, int renderMode)
		{
			Effects.SendTargetEffect(this, itemID, speed, duration, hue, renderMode);
		}

		public void FixedEffect(int itemID, int speed, int duration)
		{
			Effects.SendTargetEffect(this, itemID, speed, duration, 0, 0);
		}

		public void FixedParticles(int itemID, int speed, int duration, int effect, int hue, int renderMode, EffectLayer layer, int unknown)
		{
			Effects.SendTargetParticles(this, itemID, speed, duration, hue, renderMode, effect, layer, unknown);
		}

		public void FixedParticles(int itemID, int speed, int duration, int effect, int hue, int renderMode, EffectLayer layer)
		{
			Effects.SendTargetParticles(this, itemID, speed, duration, hue, renderMode, effect, layer, 0);
		}

		public void FixedParticles(int itemID, int speed, int duration, int effect, EffectLayer layer, int unknown)
		{
			Effects.SendTargetParticles(this, itemID, speed, duration, 0, 0, effect, layer, unknown);
		}

		public void FixedParticles(int itemID, int speed, int duration, int effect, EffectLayer layer)
		{
			Effects.SendTargetParticles(this, itemID, speed, duration, 0, 0, effect, layer, 0);
		}

		public void BoltEffect(int hue)
		{
			Effects.SendBoltEffect(this, true, hue);
		}

		public void SendIncomingPacket()
		{
			if (m_Map != null)
			{
				IPooledEnumerable eable = m_Map.GetClientsInRange(m_Location);

				foreach (NetState state in eable)
				{
					if (state.Mobile.CanSee(this))
					{
						state.Send(new MobileIncoming(state.Mobile, this));

						if (IsDeadBondedPet)
							state.Send(new BondedStatus(0, m_Serial, 1));

						if (ObjectPropertyList.Enabled)
						{
							state.Send(OPLPacket);

							//foreach ( Item item in m_Items )
							//	state.Send( item.OPLPacket );
						}
					}
				}

				eable.Free();
			}
		}

		public bool PlaceInBackpack(Item item)
		{
			if (item.Deleted)
				return false;

			Container pack = this.Backpack;

			return pack != null && pack.TryDropItem(this, item, false);
		}

		public bool AddToBackpack(Item item)
		{
			if (item.Deleted)
				return false;

			if (!PlaceInBackpack(item))
			{
				Point3D loc = m_Location;
				Map map = m_Map;

				if ((map == null || map == Map.Internal) && m_LogoutMap != null)
				{
					loc = m_LogoutLocation;
					map = m_LogoutMap;
				}

				item.MoveToWorld(loc, map);
				return false;
			}

			return true;
		}

		//		public virtual bool CheckLift( Mobile from, Item item )
		//		{
		//			return true;
		//        }
		public virtual bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{
			return true;
		}

		public virtual bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (from == this || (from.AccessLevel > this.AccessLevel && from.AccessLevel >= AccessLevel.GameMaster))
				return true;

			return false;
		}

		public bool HasTrade
		{
			get
			{
				if (m_NetState != null)
					return m_NetState.Trades.Count > 0;

				return false;
			}
		}

		public virtual bool CheckTrade(Mobile to, Item item, SecureTradeContainer cont, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Event invoked when a Mobile (<paramref name="from" />) drops an <see cref="Item"><paramref name="dropped" /></see> onto the Mobile.
		/// </summary>
		public virtual bool OnDragDrop(Mobile from, Item dropped)
		{
			if (from == this)
			{
				Container pack = this.Backpack;

				if (pack != null)
					return dropped.DropToItem(from, pack, new Point3D(-1, -1, 0));

				return false;
			}
			else if (from.Player && this.Player && from.Alive && this.Alive && from.InRange(Location, 2))
			{
				NetState ourState = m_NetState;
				NetState theirState = from.m_NetState;

				if (ourState != null && theirState != null)
				{
					SecureTradeContainer cont = theirState.FindTradeContainer(this);

					if (!from.CheckTrade(this, dropped, cont, true, true, 0, 0))
						return false;

					if (cont == null)
						cont = theirState.AddTrade(ourState);

					cont.DropItem(dropped);

					return true;
				}

				return false;
			}
			else
			{
				return false;
			}
		}

		public virtual bool CheckEquip(Item item)
		{
			for (int i = 0; i < m_Items.Count; ++i)
				if (((Item)m_Items[i]).CheckConflictingLayer(this, item, item.Layer) || item.CheckConflictingLayer(this, (Item)m_Items[i], ((Item)m_Items[i]).Layer))
					return false;

			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile attempts to wear <paramref name="item" />.
		/// </summary>
		/// <returns>True if the request is accepted, false if otherwise.</returns>
		public virtual bool OnEquip(Item item)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile attempts to lift <paramref name="item" />.
		/// </summary>
		/// <returns>True if the lift is allowed, false if otherwise.</returns>
		/// <example>
		/// The following example demonstrates usage. It will disallow any attempts to pick up a pick axe if the Mobile does not have enough strength.
		/// <code>
		/// public override bool OnDragLift( Item item )
		/// {
		///		if ( item is Pickaxe &amp;&amp; this.Str &lt; 60 )
		///		{
		///			SendMessage( "That is too heavy for you to lift." );
		///			return false;
		///		}
		///		
		///		return base.OnDragLift( item );
		/// }</code>
		/// </example>
		public virtual bool OnDragLift(Item item)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> into a <see cref="Container"><paramref name="container" /></see>.
		/// </summary>
		/// <returns>True if the drop is allowed, false if otherwise.</returns>
		public virtual bool OnDroppedItemInto(Item item, Container container, Point3D loc)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> directly onto another <see cref="Item" />, <paramref name="target" />. This is the case of stacking items.
		/// </summary>
		/// <returns>True if the drop is allowed, false if otherwise.</returns>
		public virtual bool OnDroppedItemOnto(Item item, Item target)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> into another <see cref="Item" />, <paramref name="target" />. The target item is most likely a <see cref="Container" />.
		/// </summary>
		/// <returns>True if the drop is allowed, false if otherwise.</returns>
		public virtual bool OnDroppedItemToItem(Item item, Item target, Point3D loc)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile attempts to give <paramref name="item" /> to a Mobile (<paramref name="target" />).
		/// </summary>
		/// <returns>True if the drop is allowed, false if otherwise.</returns>
		public virtual bool OnDroppedItemToMobile(Item item, Mobile target)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> to the world at a <see cref="Point3D"><paramref name="location" /></see>.
		/// </summary>
		/// <returns>True if the drop is allowed, false if otherwise.</returns>
		public virtual bool OnDroppedItemToWorld(Item item, Point3D location)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event when <paramref name="from" /> successfully uses <paramref name="item" /> while it's on this Mobile.
		/// <seealso cref="Item.OnItemUsed" />
		/// </summary>
		public virtual void OnItemUsed(Mobile from, Item item)
		{
		}

		public virtual bool CheckNonlocalDrop(Mobile from, Item item, Item target)
		{
			if (from == this || (from.AccessLevel > this.AccessLevel && from.AccessLevel >= AccessLevel.GameMaster))
				return true;

			return false;
		}

		public virtual bool CheckItemUse(Mobile from, Item item)
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when <paramref name="from" /> successfully lifts <paramref name="item" /> from this Mobile.
		/// <seealso cref="Item.OnItemLifted" />
		/// </summary>
		public virtual void OnItemLifted(Mobile from, Item item)
		{
		}

		public virtual bool AllowItemUse(Item item)
		{
			return true;
		}

		public virtual bool AllowEquipFrom(Mobile mob)
		{
			return (mob == this || (mob.AccessLevel >= AccessLevel.GameMaster && mob.AccessLevel > this.AccessLevel));
		}

		public virtual bool EquipItem(Item item)
		{
			if (item == null || item.Deleted || !item.CanEquip(this))
				return false;

			//check region for equip requests.
			if (!Region.EquipItem(this, item))
				return false;

			if (CheckEquip(item) && OnEquip(item) && item.OnEquip(this))
			{
				if (m_Spell != null && !m_Spell.OnCasterEquiping(item))
					return false;

				//if ( m_Spell != null && m_Spell.State == SpellState.Casting )
				//	m_Spell.Disturb( DisturbType.EquipRequest );

				AddItem(item);
				return true;
			}

			return false;
		}

		internal int m_TypeRef;

		public Mobile(Serial serial)
		{
			m_Region = Map.Internal.DefaultRegion;
			m_Serial = serial;
			m_Aggressors = new ArrayList(1);
			m_Aggressed = new ArrayList(1);
			m_NextSkillTime = DateTime.MinValue;
			m_DamageEntries = new ArrayList(1);

			Type ourType = this.GetType();
			m_TypeRef = World.m_MobileTypes.IndexOf(ourType);

			if (m_TypeRef == -1)
				m_TypeRef = World.m_MobileTypes.Add(ourType);
		}

		public Mobile()
		{
			m_Region = Map.Internal.DefaultRegion;
			m_Serial = Server.Serial.NewMobile;

			DefaultMobileInit();

			World.AddMobile(this);

			Type ourType = this.GetType();
			m_TypeRef = World.m_MobileTypes.IndexOf(ourType);

			if (m_TypeRef == -1)
				m_TypeRef = World.m_MobileTypes.Add(ourType);
		}

		public void DefaultMobileInit()
		{
			m_StatCap = 225;
			m_FollowersMax = 5;
			m_Skills = new Skills(this);
			m_Items = new ArrayList(1);
			m_StatMods = new ArrayList(1);
			Map = Map.Internal;
			m_AutoPageNotify = true;
			m_Aggressors = new ArrayList(1);
			m_Aggressed = new ArrayList(1);
			m_Virtues = new VirtueInfo();
			m_Stabled = new ArrayList(1);
			m_DamageEntries = new ArrayList(1);

			m_NextSkillTime = DateTime.MinValue;
			m_CreationTime = DateTime.Now;
		}

		private static Queue<Mobile> m_DeltaQueue = new Queue<Mobile>();

		private bool m_InDeltaQueue;
		private MobileDelta m_DeltaFlags;

		public virtual void Delta(MobileDelta flag)
		{
			if (m_Map == null || m_Map == Map.Internal || m_Deleted)
				return;

			m_DeltaFlags |= flag;

			if (!m_InDeltaQueue)
			{
				m_InDeltaQueue = true;

				m_DeltaQueue.Enqueue(this);
			}

			Core.Set();
		}

		public Direction GetDirectionTo(int x, int y)
		{
			int dx = m_Location.m_X - x;
			int dy = m_Location.m_Y - y;

			int rx = (dx - dy) * 44;
			int ry = (dx + dy) * 44;

			int ax = Math.Abs(rx);
			int ay = Math.Abs(ry);

			Direction ret;

			if (((ay >> 1) - ax) >= 0)
				ret = (ry > 0) ? Direction.Up : Direction.Down;
			else if (((ax >> 1) - ay) >= 0)
				ret = (rx > 0) ? Direction.Left : Direction.Right;
			else if (rx >= 0 && ry >= 0)
				ret = Direction.West;
			else if (rx >= 0 && ry < 0)
				ret = Direction.South;
			else if (rx < 0 && ry < 0)
				ret = Direction.East;
			else
				ret = Direction.North;

			return ret;
		}

		public Direction GetDirectionTo(Point2D p)
		{
			return GetDirectionTo(p.m_X, p.m_Y);
		}

		public Direction GetDirectionTo(Point3D p)
		{
			return GetDirectionTo(p.m_X, p.m_Y);
		}

		public Direction GetDirectionTo(IPoint2D p)
		{
			if (p == null)
				return Direction.North;

			return GetDirectionTo(p.X, p.Y);
		}

		public virtual void ProcessDelta()
		{
			Mobile m = this;
			MobileDelta delta;

			delta = m.m_DeltaFlags;

			if (delta == MobileDelta.None)
				return;

			MobileDelta attrs = delta & MobileDelta.Attributes;

			m.m_DeltaFlags = MobileDelta.None;
			m.m_InDeltaQueue = false;

			bool sendHits = false, sendStam = false, sendMana = false, sendAll = false, sendAny = false;
			bool sendIncoming = false, sendNonlocalIncoming = false;
			bool sendUpdate = false, sendRemove = false;
			bool sendPublicStats = false, sendPrivateStats = false;
			bool sendMoving = false, sendNonlocalMoving = false;
			bool sendOPLUpdate = ObjectPropertyList.Enabled && (delta & MobileDelta.Properties) != 0;

			bool sendHair = false, sendFacialHair = false, removeHair = false, removeFacialHair = false;

			if (attrs != MobileDelta.None)
			{
				sendAny = true;

				if (attrs == MobileDelta.Attributes)
				{
					sendAll = true;
				}
				else
				{
					sendHits = ((attrs & MobileDelta.Hits) != 0);
					sendStam = ((attrs & MobileDelta.Stam) != 0);
					sendMana = ((attrs & MobileDelta.Mana) != 0);
				}
			}

			if ((delta & MobileDelta.GhostUpdate) != 0)
			{
				sendNonlocalIncoming = true;
			}

			if ((delta & MobileDelta.Hue) != 0)
			{
				sendNonlocalIncoming = true;
				sendUpdate = true;
				sendRemove = true;
			}

			if ((delta & MobileDelta.Direction) != 0)
			{
				sendNonlocalMoving = true;
				sendUpdate = true;
			}

			if ((delta & MobileDelta.Body) != 0)
			{
				sendUpdate = true;
				sendIncoming = true;
			}

			/*if ( (delta & MobileDelta.Hue) != 0 )
				{
					sendNonlocalIncoming = true;
					sendUpdate = true;
				}
				else if ( (delta & (MobileDelta.Direction | MobileDelta.Body)) != 0 )
				{
					sendNonlocalMoving = true;
					sendUpdate = true;
				}
				else*/
			if ((delta & (MobileDelta.Flags | MobileDelta.Noto)) != 0)
			{
				sendMoving = true;
			}

			if ((delta & MobileDelta.Name) != 0)
			{
				sendAll = false;
				sendHits = false;
				sendAny = sendStam || sendMana;
				sendPublicStats = true;
			}

			if ((delta & (MobileDelta.WeaponDamage | MobileDelta.Resistances | MobileDelta.Stat | MobileDelta.Weight | MobileDelta.Gold | MobileDelta.Armor | MobileDelta.StatCap | MobileDelta.Followers | MobileDelta.TithingPoints | MobileDelta.Race)) != 0)
			{
				sendPrivateStats = true;
			}

			if ((delta & MobileDelta.Hair) != 0)
			{
				if (m.HairItemID <= 0)
					removeHair = true;

				sendHair = true;
			}

			if ((delta & MobileDelta.FacialHair) != 0)
			{
				if (m.FacialHairItemID <= 0)
					removeFacialHair = true;

				sendFacialHair = true;
			}

			Packet[] cache = m_MovingPacketCache;

			if (sendMoving || sendNonlocalMoving)
			{
				for (int i = 0; i < cache.Length; ++i)
					Packet.Release(ref cache[i]);
			}

			NetState ourState = m.m_NetState;

			if (ourState != null)
			{
				if (sendUpdate)
				{
					ourState.Sequence = 0;
					ourState.Send(new MobileUpdate(m));
					ClearFastwalkStack();
				}

				if (sendIncoming)
					ourState.Send(new MobileIncoming(m, m));

				if (sendMoving)
				{
					int noto = Notoriety.Compute(m, m);
					ourState.Send(cache[noto] = Packet.Acquire(new MobileMoving(m, noto)));
				}

				if (sendPublicStats || sendPrivateStats)
				{
					ourState.Send(new MobileStatusExtended(m));
				}
				else if (sendAll)
				{
					ourState.Send(new MobileAttributes(m));
				}
				else if (sendAny)
				{
					if (sendHits)
						ourState.Send(new MobileHits(m));

					if (sendStam)
						ourState.Send(new MobileStam(m));

					if (sendMana)
						ourState.Send(new MobileMana(m));
				}

				if (sendStam || sendMana)
				{
					IParty ip = m_Party as IParty;

					if (ip != null && sendStam)
						ip.OnStamChanged(this);

					if (ip != null && sendMana)
						ip.OnManaChanged(this);
				}

				if (sendHair)
				{
					if (removeHair)
						ourState.Send(new RemoveHair(m));
					else
						ourState.Send(new HairEquipUpdate(m));
				}

				if (sendFacialHair)
				{
					if (removeFacialHair)
						ourState.Send(new RemoveFacialHair(m));
					else
						ourState.Send(new FacialHairEquipUpdate(m));
				}

				if (sendOPLUpdate)
					ourState.Send(OPLPacket);
			}

			sendMoving = sendMoving || sendNonlocalMoving;
			sendIncoming = sendIncoming || sendNonlocalIncoming;
			sendHits = sendHits || sendAll;

			if (m.m_Map != null && (sendRemove || sendIncoming || sendPublicStats || sendHits || sendMoving || sendOPLUpdate || sendHair || sendFacialHair))
			{
				Mobile beholder;

				IPooledEnumerable eable = m.Map.GetClientsInRange(m.m_Location);

				Packet hitsPacket = null;
				Packet statPacketTrue = null, statPacketFalse = null;
				Packet deadPacket = null;
				Packet hairPacket = null, facialhairPacket = null;

				foreach (NetState state in eable)
				{
					beholder = state.Mobile;

					if (beholder != m && beholder.CanSee(m))
					{
						if (sendRemove)
							state.Send(m.RemovePacket);

						if (sendIncoming)
						{
							state.Send(new MobileIncoming(beholder, m));

							if (m.IsDeadBondedPet)
							{
								if (deadPacket == null)
									deadPacket = Packet.Acquire(new BondedStatus(0, m.m_Serial, 1));

								state.Send(deadPacket);
							}
						}

						if (sendMoving)
						{
							int noto = Notoriety.Compute(beholder, m);

							Packet p = cache[noto];

							if (p == null)
								cache[noto] = p = Packet.Acquire(new MobileMoving(m, noto));

							state.Send(p);
						}

						if (sendPublicStats)
						{
							if (m.CanBeRenamedBy(beholder))
							{
								if (statPacketTrue == null)
									statPacketTrue = Packet.Acquire(new MobileStatusCompact(true, m));

								state.Send(statPacketTrue);
							}
							else
							{
								if (statPacketFalse == null)
									statPacketFalse = Packet.Acquire(new MobileStatusCompact(false, m));

								state.Send(statPacketFalse);
							}
						}
						else if (sendHits)
						{
							if (hitsPacket == null)
								hitsPacket = Packet.Acquire(new MobileHitsN(m));

							state.Send(hitsPacket);
						}

						if (sendHair)
						{
							if (hairPacket == null)
							{
								if (removeHair)
									hairPacket = Packet.Acquire(new RemoveHair(m));
								else
									hairPacket = Packet.Acquire(new HairEquipUpdate(m));
							}

							state.Send(hairPacket);
						}

						if (sendFacialHair)
						{
							if (facialhairPacket == null)
							{
								if (removeFacialHair)
									facialhairPacket = Packet.Acquire(new RemoveFacialHair(m));
								else
									facialhairPacket = Packet.Acquire(new FacialHairEquipUpdate(m));
							}

							state.Send(facialhairPacket);
						}

						if (sendOPLUpdate)
							state.Send(OPLPacket);
					}
				}

				Packet.Release(hitsPacket);
				Packet.Release(statPacketTrue);
				Packet.Release(statPacketFalse);
				Packet.Release(deadPacket);
				Packet.Release(hairPacket);
				Packet.Release(facialhairPacket);

				eable.Free();
			}

			if (sendMoving || sendNonlocalMoving)
			{
				for (int i = 0; i < cache.Length; ++i)
					Packet.Release(ref cache[i]);
			}
		}

		public static void ProcessDeltaQueue()
		{
			int count = m_DeltaQueue.Count;
			int index = 0;

			while (m_DeltaQueue.Count > 0 && index++ < count)
				m_DeltaQueue.Dequeue().ProcessDelta();
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Kills
		{
			get
			{
				return m_Kills;
			}
			set
			{
				int oldValue = m_Kills;

				if (m_Kills != value)
				{
					m_Kills = value;

					if (m_Kills < 0)
						m_Kills = 0;

					if ((oldValue >= 5) != (m_Kills >= 5))
					{
						Delta(MobileDelta.Noto);
						InvalidateProperties();
					}

					OnKillsChange(oldValue);
				}
			}
		}

		public virtual void OnKillsChange(int oldValue)
		{
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int ShortTermMurders
		{
			get
			{
				return m_ShortTermMurders;
			}
			set
			{
				if (m_ShortTermMurders != value)
				{
					m_ShortTermMurders = value;

					if (m_ShortTermMurders < 0)
						m_ShortTermMurders = 0;
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Criminal
		{
			get
			{
				return m_Criminal;
			}
			set
			{
				if (m_Criminal != value)
				{
					m_Criminal = value;
					Delta(MobileDelta.Noto);
					InvalidateProperties();
				}

				if (m_Criminal)
				{
					if (m_ExpireCriminal == null)
						m_ExpireCriminal = new ExpireCriminalTimer(this);
					else
						m_ExpireCriminal.Stop();

					m_ExpireCriminal.Start();
				}
				else if (m_ExpireCriminal != null)
				{
					m_ExpireCriminal.Stop();
					m_ExpireCriminal = null;
				}
			}
		}

		public bool CheckAlive()
		{
			return CheckAlive(true);
		}

		public bool CheckAlive(bool message)
		{
			if (!Alive)
			{
				if (message)
					this.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019048); // I am dead and cannot do that.

				return false;
			}
			else
			{
				return true;
			}
		}

		public void PublicOverheadMessage(MessageType type, int hue, bool ascii, string text)
		{
			PublicOverheadMessage(type, hue, ascii, text, true);
		}

		public void PublicOverheadMessage(MessageType type, int hue, bool ascii, string text, bool noLineOfSight)
		{
			if (m_Map != null)
			{
				Packet p = null;

				IPooledEnumerable eable = m_Map.GetClientsInRange(m_Location);

				foreach (NetState state in eable)
				{
					// wea: changed to a check to see if target is audible to the speaker 
					// if ( state.Mobile.CanSee( this ) && (noLineOfSight || state.Mobile.InLOS( this )) )
					if (state.Mobile.CanSee(this) && (noLineOfSight || state.Mobile.IsAudibleTo(this)))
					{
						if (p == null)
						{
							if (ascii)
								p = new AsciiMessage(m_Serial, Body, type, hue, 3, Name, text);
							else
								p = new UnicodeMessage(m_Serial, Body, type, hue, 3, m_Language, Name, text);

							p.Acquire();
						}

						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void PublicOverheadMessage(MessageType type, int hue, int number)
		{
			PublicOverheadMessage(type, hue, number, "", true);
		}

		public void PublicOverheadMessage(MessageType type, int hue, int number, string args)
		{
			PublicOverheadMessage(type, hue, number, args, true);
		}

		public void PublicOverheadMessage(MessageType type, int hue, int number, string args, bool noLineOfSight)
		{
			if (m_Map != null)
			{
				Packet p = null;

				IPooledEnumerable eable = m_Map.GetClientsInRange(m_Location);

				foreach (NetState state in eable)
				{
					// wea: InLOS -> IsAudibleTo
					if (state.Mobile.CanSee(this) && (noLineOfSight || state.Mobile.IsAudibleTo(this)))
					{
						if (p == null)
							p = Packet.Acquire(new MessageLocalized(m_Serial, Body, type, hue, 3, number, Name, args));

						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void PublicOverheadMessage(MessageType type, int hue, int number, AffixType affixType, string affix, string args)
		{
			PublicOverheadMessage(type, hue, number, affixType, affix, args, true);
		}

		public void PublicOverheadMessage(MessageType type, int hue, int number, AffixType affixType, string affix, string args, bool noLineOfSight)
		{
			if (m_Map != null)
			{
				Packet p = null;

				IPooledEnumerable eable = m_Map.GetClientsInRange(m_Location);

				foreach (NetState state in eable)
				{
					// wea: InLOS -> IsAudibleTo
					if (state.Mobile.CanSee(this) && (noLineOfSight || state.Mobile.IsAudibleTo(this)))
					{
						if (p == null)
							p = Packet.Acquire(new MessageLocalizedAffix(m_Serial, Body, type, hue, 3, number, Name, affixType, affix, args));

						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void PrivateOverheadMessage(MessageType type, int hue, bool ascii, string text, NetState state)
		{
			if (state == null) return;

			if (ascii)
				state.Send(new AsciiMessage(m_Serial, Body, type, hue, 3, Name, text));
			else
				state.Send(new UnicodeMessage(m_Serial, Body, type, hue, 3, m_Language, Name, text));
		}

		public void PrivateOverheadMessage(MessageType type, int hue, int number, NetState state)
		{
			PrivateOverheadMessage(type, hue, number, "", state);
		}

		public void PrivateOverheadMessage(MessageType type, int hue, int number, string args, NetState state)
		{
			if (state == null)
				return;

			state.Send(new MessageLocalized(m_Serial, Body, type, hue, 3, number, Name, args));
		}

		public void LocalOverheadMessage(MessageType type, int hue, bool ascii, string text)
		{
			NetState ns = m_NetState;

			if (ns != null)
			{
				if (ascii)
					ns.Send(new AsciiMessage(m_Serial, Body, type, hue, 3, Name, text));
				else
					ns.Send(new UnicodeMessage(m_Serial, Body, type, hue, 3, m_Language, Name, text));
			}
		}

		public void LocalOverheadMessage(MessageType type, int hue, int number)
		{
			LocalOverheadMessage(type, hue, number, "");
		}

		public void LocalOverheadMessage(MessageType type, int hue, int number, string args)
		{
			NetState ns = m_NetState;

			if (ns != null)
				ns.Send(new MessageLocalized(m_Serial, Body, type, hue, 3, number, Name, args));
		}

		public void NonlocalOverheadMessage(MessageType type, int hue, int number)
		{
			NonlocalOverheadMessage(type, hue, number, "");
		}

		public void NonlocalOverheadMessage(MessageType type, int hue, int number, string args)
		{
			if (m_Map != null)
			{
				Packet p = null;

				IPooledEnumerable eable = m_Map.GetClientsInRange(m_Location);

				foreach (NetState state in eable)
				{
					if (state != m_NetState && state.Mobile.CanSee(this))
					{
						if (p == null)
							p = Packet.Acquire(new MessageLocalized(m_Serial, Body, type, hue, 3, number, Name, args));

						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void NonlocalOverheadMessage(MessageType type, int hue, bool ascii, string text)
		{
			if (m_Map != null)
			{
				Packet p = null;

				IPooledEnumerable eable = m_Map.GetClientsInRange(m_Location);

				foreach (NetState state in eable)
				{
					if (state != m_NetState && state.Mobile.CanSee(this))
					{
						if (p == null)
						{
							if (ascii)
								p = new AsciiMessage(m_Serial, Body, type, hue, 3, Name, text);
							else
								p = new UnicodeMessage(m_Serial, Body, type, hue, 3, Language, Name, text);

							p.Acquire();
						}

						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void SendLocalizedMessage(int number)
		{
			NetState ns = m_NetState;

			if (ns != null)
				ns.Send(MessageLocalized.InstantiateGeneric(number));
		}

		public void SendLocalizedMessage(int number, string args)
		{
			SendLocalizedMessage(number, args, 0x3B2);
		}

		public void SendLocalizedMessage(int number, string args, int hue)
		{
			if (hue == 0x3B2 && (args == null || args.Length == 0))
			{
				NetState ns = m_NetState;

				if (ns != null)
					ns.Send(MessageLocalized.InstantiateGeneric(number));
			}
			else
			{
				NetState ns = m_NetState;

				if (ns != null)
					ns.Send(new MessageLocalized(Serial.MinusOne, -1, MessageType.Regular, hue, 3, number, "System", args));
			}
		}

		public void SendLocalizedMessage(int number, bool append, string affix)
		{
			SendLocalizedMessage(number, append, affix, "", 0x3B2);
		}

		public void SendLocalizedMessage(int number, bool append, string affix, string args)
		{
			SendLocalizedMessage(number, append, affix, args);
		}

		public void SendLocalizedMessage(int number, bool append, string affix, string args, int hue)
		{
			NetState ns = m_NetState;

			if (ns != null)
				ns.Send(new MessageLocalizedAffix(Serial.MinusOne, -1, MessageType.Regular, hue, 3, number, "System", (append ? AffixType.Append : AffixType.Prepend) | AffixType.System, affix, args));
		}

		public void LaunchBrowser(string url)
		{
			if (m_NetState != null)
				m_NetState.LaunchBrowser(url);
		}

		public void SendMessage(string text)
		{
			SendMessage(0x3B2, text);
		}

		public void SendMessage(string format, params object[] args)
		{
			SendMessage(0x3B2, String.Format(format, args));
		}

		public void SendMessage(int hue, string text)
		{
			NetState ns = m_NetState;

			if (ns != null)
				ns.Send(new UnicodeMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "ENU", "System", text));
		}

		public void SendMessage(int hue, string format, params object[] args)
		{
			SendMessage(hue, String.Format(format, args));
		}

		public void SendAsciiMessage(string text)
		{
			SendAsciiMessage(0x3B2, text);
		}

		public void SendAsciiMessage(string format, params object[] args)
		{
			SendAsciiMessage(0x3B2, String.Format(format, args));
		}

		public void SendAsciiMessage(int hue, string text)
		{
			NetState ns = m_NetState;

			if (ns != null)
				ns.Send(new AsciiMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "System", text));
		}

		public void SendAsciiMessage(int hue, string format, params object[] args)
		{
			SendAsciiMessage(hue, String.Format(format, args));
		}

		public bool InRange(Point2D p, int range)
		{
			return (p.m_X >= (m_Location.m_X - range))
				&& (p.m_X <= (m_Location.m_X + range))
				&& (p.m_Y >= (m_Location.m_Y - range))
				&& (p.m_Y <= (m_Location.m_Y + range));
		}

		public bool InRange(Point3D p, int range)
		{
			return (p.m_X >= (m_Location.m_X - range))
				&& (p.m_X <= (m_Location.m_X + range))
				&& (p.m_Y >= (m_Location.m_Y - range))
				&& (p.m_Y <= (m_Location.m_Y + range));
		}

		public bool InRange(IPoint2D p, int range)
		{
			return (p.X >= (m_Location.m_X - range))
				&& (p.X <= (m_Location.m_X + range))
				&& (p.Y >= (m_Location.m_Y - range))
				&& (p.Y <= (m_Location.m_Y + range));
		}

		public void InitStats(int str, int dex, int intel)
		{
			m_Str = str;
			m_Dex = dex;
			m_Int = intel;

			Hits = HitsMax;
			Stam = StamMax;
			Mana = ManaMax;

			Delta(MobileDelta.Stat | MobileDelta.Hits | MobileDelta.Stam | MobileDelta.Mana);
		}

		public virtual void DisplayPaperdollTo(Mobile to)
		{
			EventSink.InvokePaperdollRequest(new PaperdollRequestEventArgs(to, this));
		}

		private static bool m_DisableDismountInWarmode;

		public static bool DisableDismountInWarmode { get { return m_DisableDismountInWarmode; } set { m_DisableDismountInWarmode = value; } }

		/// <summary>
		/// Overridable. Event invoked when the Mobile is double clicked. By default, this method can either dismount or open the paperdoll.
		/// <seealso cref="CanPaperdollBeOpenedBy" />
		/// <seealso cref="DisplayPaperdollTo" />
		/// </summary>
		public virtual void OnDoubleClick(Mobile from)
		{
			if (this == from && (!m_DisableDismountInWarmode || !m_Warmode))
			{
				IMount mount = Mount;

				if (mount != null)
				{
					mount.Rider = null;
					return;
				}
			}

			if (CanPaperdollBeOpenedBy(from))
				DisplayPaperdollTo(from);
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile is double clicked by someone who is over 18 tiles away.
		/// <seealso cref="OnDoubleClick" />
		/// </summary>
		public virtual void OnDoubleClickOutOfRange(Mobile from)
		{
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the Mobile is double clicked by someone who can no longer see the Mobile. This may happen, for example, using 'Last Object' after the Mobile has hidden.
		/// <seealso cref="OnDoubleClick" />
		/// </summary>
		public virtual void OnDoubleClickCantSee(Mobile from)
		{
		}

		/// <summary>
		/// Overridable. Event invoked when the Mobile is double clicked by someone who is not alive. Similar to <see cref="OnDoubleClick" />, this method will show the paperdoll. It does not, however, provide any dismount functionality.
		/// <seealso cref="OnDoubleClick" />
		/// </summary>
		public virtual void OnDoubleClickDead(Mobile from)
		{
			if (CanPaperdollBeOpenedBy(from))
				DisplayPaperdollTo(from);
		}

		/// <summary>
		/// Overridable. Event invoked when the Mobile requests to open his own paperdoll via the 'Open Paperdoll' macro.
		/// </summary>
		public virtual void OnPaperdollRequest()
		{
			if (CanPaperdollBeOpenedBy(this))
				DisplayPaperdollTo(this);
		}

		private static int m_BodyWeight = 14;

		public static int BodyWeight { get { return m_BodyWeight; } set { m_BodyWeight = value; } }

		/// <summary>
		/// Overridable. Event invoked when <paramref name="from" /> wants to see this Mobile's stats.
		/// </summary>
		/// <param name="from"></param>
		public virtual void OnStatsQuery(Mobile from)
		{
			if (from.Map == this.Map && Utility.InUpdateRange(this, from) && from.CanSee(this))
				from.Send(new MobileStatus(from, this));

			if (from == this)
				Send(new StatLockInfo(this));

			IParty ip = m_Party as IParty;

			if (ip != null)
				ip.OnStatsQuery(from, this);
		}

		/// <summary>
		/// Overridable. Event invoked when <paramref name="from" /> wants to see this Mobile's skills.
		/// </summary>
		public virtual void OnSkillsQuery(Mobile from)
		{
			if (from == this)
				Send(new SkillUpdate(m_Skills));
		}

		/// <summary>
		/// Overridable. Virtual event invoked when <see cref="Region" /> changes.
		/// </summary>
		public virtual void OnRegionChange(Region Old, Region New)
		{
		}

		private Item m_MountItem;

		[CommandProperty(AccessLevel.GameMaster)]
		public IMount Mount
		{
			get
			{
				IMountItem mountItem = null;

				if (m_MountItem != null && !m_MountItem.Deleted && m_MountItem.Parent == this)
					mountItem = (IMountItem)m_MountItem;

				if (mountItem == null)
					m_MountItem = (mountItem = (FindItemOnLayer(Layer.Mount) as IMountItem)) as Item;

				return mountItem == null ? null : mountItem.Mount;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Mounted
		{
			get
			{
				return (Mount != null);
			}
		}

		private QuestArrow m_QuestArrow;

		public QuestArrow QuestArrow
		{
			get
			{
				return m_QuestArrow;
			}
			set
			{
				if (m_QuestArrow != value)
				{
					if (m_QuestArrow != null)
						m_QuestArrow.Stop();

					m_QuestArrow = value;
				}
			}
		}

		private static string[] m_GuildTypes = new string[]
			{
				"",
				" (Chaos)",
				" (Order)"
			};

		public virtual bool CanTarget { get { return true; } }
		public virtual bool ClickTitle { get { return true; } }

		private static bool m_DisableHiddenSelfClick = true;

		public static bool DisableHiddenSelfClick { get { return m_DisableHiddenSelfClick; } set { m_DisableHiddenSelfClick = value; } }

		private static bool m_AsciiClickMessage = true;

		public static bool AsciiClickMessage { get { return m_AsciiClickMessage; } set { m_AsciiClickMessage = value; } }

		private static bool m_GuildClickMessage = true;

		public static bool GuildClickMessage { get { return m_GuildClickMessage; } set { m_GuildClickMessage = value; } }

		public virtual bool ShowFameTitle { get { return true; } }//(m_Player || m_Body.IsHuman) && m_Fame >= 10000; } 

		/// <summary>
		/// Overridable. Event invoked when the Mobile is single clicked.
		/// </summary>
		public virtual void OnSingleClick(Mobile from)
		{
			if (m_Deleted)
				return;
			else if (AccessLevel == AccessLevel.Player && DisableHiddenSelfClick && Hidden && from == this)
				return;

			if (m_GuildClickMessage)
			{
				BaseGuild guild = m_Guild;

				if (guild != null && (m_DisplayGuildTitle || (m_Player && guild.Type != GuildType.Regular)))
				{
					string title = GuildTitle;
					string type;

					if (title == null)
						title = "";
					else
						title = title.Trim();

					if (guild.Type >= 0 && (int)guild.Type < m_GuildTypes.Length)
						type = m_GuildTypes[(int)guild.Type];
					else
						type = "";

					string text = String.Format(title.Length <= 0 ? "[{1}]{2}" : "[{0}, {1}]{2}", title, guild.Abbreviation, type);

					PrivateOverheadMessage(MessageType.Regular, SpeechHue, true, text, from.NetState);
				}
			}

			int hue;

			if (m_NameHue != -1)
				hue = m_NameHue;
			else if (AccessLevel > AccessLevel.Player)
				hue = 11;
			else
			{
				int notoriety = Notoriety.Compute(from, this);
				hue = Notoriety.GetHue(notoriety);

				//PIX: if they're looking innocent, see if there
				// are any ill-effects from beneficial actions
				if (notoriety == Notoriety.Innocent)
				{
					int namehue = Notoriety.GetBeneficialHue(from, this);
					if (namehue != 0)
					{
						hue = namehue;
					}
				}
			}


			string name = Name;

			if (name == null)
				name = String.Empty;

			string prefix = "";

			if (ShowFameTitle && (m_Player || m_Body.IsHuman) && m_Fame >= 10000)
				prefix = (m_Female ? "Lady" : "Lord");

			string suffix = "";

			if (ClickTitle && Title != null && Title.Length > 0)
				suffix = Title;

			suffix = ApplyNameSuffix(suffix);

			string val;

			if (prefix.Length > 0 && suffix.Length > 0)
				val = String.Concat(prefix, " ", name, " ", suffix);
			else if (prefix.Length > 0)
				val = String.Concat(prefix, " ", name);
			else if (suffix.Length > 0)
				val = String.Concat(name, " ", suffix);
			else
				val = name;

			PrivateOverheadMessage(MessageType.Label, hue, m_AsciiClickMessage, val, from.NetState);
		}

		public virtual void DisruptiveAction()
		{
			if (Meditating)
			{
				Meditating = false;
				SendLocalizedMessage(500134); // You stop meditating.
			}
		}

		public Item ShieldArmor
		{
			get
			{
				return FindItemOnLayer(Layer.TwoHanded) as Item;
			}
		}

		public Item NeckArmor
		{
			get
			{
				return FindItemOnLayer(Layer.Neck) as Item;
			}
		}

		public Item HandArmor
		{
			get
			{
				return FindItemOnLayer(Layer.Gloves) as Item;
			}
		}

		public Item HeadArmor
		{
			get
			{
				return FindItemOnLayer(Layer.Helm) as Item;
			}
		}

		public Item ArmsArmor
		{
			get
			{
				return FindItemOnLayer(Layer.Arms) as Item;
			}
		}

		public Item LegsArmor
		{
			get
			{
				Item ar = FindItemOnLayer(Layer.InnerLegs) as Item;

				if (ar == null)
					ar = FindItemOnLayer(Layer.Pants) as Item;

				return ar;
			}
		}

		public Item ChestArmor
		{
			get
			{
				Item ar = FindItemOnLayer(Layer.InnerTorso) as Item;

				if (ar == null)
					ar = FindItemOnLayer(Layer.Shirt) as Item;

				return ar;
			}
		}

		/// <summary>
		/// Gets or sets the maximum attainable value for <see cref="RawStr" />, <see cref="RawDex" />, and <see cref="RawInt" />.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int StatCap
		{
			get
			{
				return m_StatCap;
			}
			set
			{
				if (m_StatCap != value)
				{
					m_StatCap = value;

					Delta(MobileDelta.StatCap);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Meditating
		{
			get
			{
				return m_Meditating;
			}
			set
			{
				m_Meditating = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CanSwim
		{
			get
			{
				return m_CanSwim;
			}
			set
			{
				m_CanSwim = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CanFly
		{
			get
			{
				return m_CanFly;
			}
			set
			{
				m_CanFly = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CantWalk
		{
			get
			{
				return m_CantWalk;
			}
			set
			{
				m_CantWalk = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CanHearGhosts
		{
			get
			{
				return m_CanHearGhosts || AccessLevel >= AccessLevel.Counselor;
			}
			set
			{
				m_CanHearGhosts = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int RawStatTotal
		{
			get
			{
				return RawStr + RawDex + RawInt;
			}
		}

		public DateTime NextSpellTime
		{
			get
			{
				return m_NextSpellTime;
			}
			set
			{
				m_NextSpellTime = value;
			}
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the sector this Mobile is in gets <see cref="Sector.Activate">activated</see>.
		/// </summary>
		public virtual void OnSectorActivate()
		{
		}

		/// <summary>
		/// Overridable. Virtual event invoked when the sector this Mobile is in gets <see cref="Sector.Deactivate">deactivated</see>.
		/// </summary>
		public virtual void OnSectorDeactivate()
		{
		}
		public int GetHueForNameInList()
		{
			switch (this.AccessLevel)
			{
				case AccessLevel.Owner: return 0x35;
				case AccessLevel.Administrator: return 0x516;
				case AccessLevel.Seer: return 0x144;
				case AccessLevel.GameMaster: return 0x21;
				case AccessLevel.Counselor: return 0x2;
				case AccessLevel.FightBroker: return 0x8AB;
				case AccessLevel.Reporter: return 0x979;
				case AccessLevel.Player:
				default:
					{
						if (this.Kills >= 5)
							return 0x21;
						else if (this.Criminal)
							return 0x3B1;

						return 0x58;
					}
			}
		}

		#region CheckSkill

		public virtual bool CheckSkill(SkillName skillName, double minSkill, double maxSkill)
		{
			Skill skill = Skills[skillName];

			if (skill == null)
				return false;

			double chance = (skill.Value - minSkill) / (maxSkill - minSkill);

			Point2D loc = new Point2D(Location.X, Location.Y);
			return CheckSkill(skill, loc, chance);
		}

		public virtual bool CheckSkill(SkillName skillName, double chance)
		{
			Skill skill = Skills[skillName];

			if (skill == null)
				return false;

			Point2D loc = new Point2D(Location.X, Location.Y);
			return CheckSkill(skill, loc, chance);
		}

		public virtual bool CheckTargetSkill(SkillName skillName, object target, double minSkill, double maxSkill)
		{
			Skill skill = Skills[skillName];

			if (skill == null)
				return false;

			double chance = (skill.Value - minSkill) / (maxSkill - minSkill);

			return CheckSkill(skill, target, chance);
		}

		public virtual bool CheckTargetSkill(SkillName skillName, object target, double chance)
		{
			Skill skill = Skills[skillName];

			if (skill == null)
				return false;

			return CheckSkill(skill, target, chance);
		}

		protected virtual double GainChance(Skill skill, double chance, bool success)
		{
			double gc = 0.5;
			gc += (skill.Cap - skill.Base) / skill.Cap;
			gc /= 2;

			gc += (1.0 - chance) * (success ? 0.5 : 0.2);
			gc /= 2;

			gc *= skill.Info.GainFactor;

			if (gc < 0.01)
				gc = 0.01;

			return gc;
		}

		protected virtual bool CheckSkill(Skill skill, object amObj, double chance)
		{
			if (skill == null)
				return false;
			if (Skills.Cap == 0)
				return false;

			if (chance < 0.0)
				return false; // Too difficult
			else if (chance >= 1.0)
				return true; // No challenge

			bool success = (chance >= Utility.RandomDouble());
			double gc = GainChance(skill, chance, success);

			// DIFFERENCE FROM PREVIOUS BEHAVIOR!            &&          <--------  ||  --------->
			if (Alive && ((gc >= Utility.RandomDouble() || skill.Base < 10.0)) && AllowGain(skill, amObj))
				Gain(skill);

			return success;
		}

		protected virtual bool AllowGain(Skill skill, object obj)
		{
			return true;
		}

		public virtual void Gain(Skill skill)
		{
			if (skill.Base < skill.Cap && skill.Lock == SkillLock.Up)
			{
				int toGain = 1;

				if (skill.Base <= 10.0)
					toGain = Utility.Random(4) + 1;

				if ((Skills.Total / Skills.Cap) >= Utility.RandomDouble())//( skills.Total >= skills.Cap )
				{
					for (int i = 0; i < Skills.Length; ++i)
					{
						Skill toLower = Skills[i];

						if (toLower != skill && toLower.Lock == SkillLock.Down && toLower.BaseFixedPoint >= toGain)
						{
							toLower.BaseFixedPoint -= toGain;
							break;
						}
					}
				}

				if ((Skills.Total + toGain) <= Skills.Cap)
				{
					skill.BaseFixedPoint += toGain;
				}
			}

			if (skill.Lock == SkillLock.Up)
			{
				if (StrLock == StatLockType.Up && StatGainChance(skill, Stat.Str) > Utility.RandomDouble())
					GainStat(Stat.Str);
				else if (DexLock == StatLockType.Up && StatGainChance(skill, Stat.Dex) > Utility.RandomDouble())
					GainStat(Stat.Dex);
				else if (IntLock == StatLockType.Up && StatGainChance(skill, Stat.Int) > Utility.RandomDouble())
					GainStat(Stat.Int);
			}
		}

		protected virtual double StatGainChance(Skill skill, Stat stat)
		{
			switch (stat)
			{
				case Stat.Str:
					return skill.Info.StrGain / 20.0;
				case Stat.Dex:
					return skill.Info.DexGain / 20.0;
				case Stat.Int:
					return skill.Info.IntGain / 20.0;
				default:
					return 0;
			}
		}

		protected virtual bool CanLower(Stat stat)
		{
			switch (stat)
			{
				case Stat.Str: return (StrLock == StatLockType.Down && RawStr > 10 && StrMax != -1);
				case Stat.Dex: return (DexLock == StatLockType.Down && RawDex > 10 && DexMax != -1);
				case Stat.Int: return (IntLock == StatLockType.Down && RawInt > 10 && IntMax != -1);
			}

			return false;
		}

		protected virtual bool CanRaise(Stat stat)
		{

			switch (stat)
			{
				case Stat.Str: return (StrLock == StatLockType.Up && StrMax != -1 && RawStr < StrMax);
				case Stat.Dex: return (DexLock == StatLockType.Up && DexMax != -1 && RawDex < DexMax);
				case Stat.Int: return (IntLock == StatLockType.Up && IntMax != -1 && RawInt < IntMax);
			}

			return false;
		}

		public virtual void ValidateStatCap(Stat increased)
		{
			if (RawStatTotal <= StatCap)
				return; // no work to be done

			switch (increased)
			{
				case Stat.Str:
					{
						if (CanLower(Stat.Dex) && (RawDex < RawInt || !CanLower(Stat.Int)))
							RawDex--;
						else if (CanLower(Stat.Int))
							RawInt--;
						else
							RawStr--;

						break;
					}
				case Stat.Dex:
					{
						if (CanLower(Stat.Str) && (RawStr < RawInt || !CanLower(Stat.Int)))
							RawStr--;
						else if (CanLower(Stat.Int))
							RawInt--;
						else
							RawDex--;

						break;
					}
				case Stat.Int:
					{
						if (CanLower(Stat.Dex) && (RawDex < RawStr || !CanLower(Stat.Str)))
							RawDex--;
						else if (CanLower(Stat.Str))
							RawStr--;
						else
							RawInt--;

						break;
					}
			}
		}

		protected virtual void IncreaseStat(Stat stat, bool atrophy)
		{
			if (!CanRaise(stat))
				return;

			switch (stat)
			{
				case Stat.Str:
					{
						RawStr++;

						break;
					}
				case Stat.Dex:
					{
						RawDex++;

						break;
					}
				case Stat.Int:
					{
						RawInt++;

						break;
					}
			}

			ValidateStatCap(stat);
		}

		private static TimeSpan m_StatGainDelay = TimeSpan.FromMinutes(2.0);

		protected virtual void GainStat(Stat stat)
		{
			if ((LastStatGain + m_StatGainDelay) >= DateTime.Now)
				return;

			LastStatGain = DateTime.Now;

			bool atrophy = false;//( (from.RawStatTotal / (double)from.StatCap) >= Utility.RandomDouble() );

			IncreaseStat(stat, atrophy);
		}

		#endregion

		//SMD: support hook for new runuo-2.0 base networking
		[CommandProperty(AccessLevel.GameMaster)]
		public int HairItemID
		{
			get
			{
				Item hair;
				if ((hair = FindItemOnLayer(Layer.Hair)) != null)
				{
					return hair.ItemID;
				}

				return 0;
			}
		}

		//SMD: support hook for new runuo-2.0 base networking
		[CommandProperty(AccessLevel.GameMaster)]
		public int FacialHairItemID
		{
			get
			{
				Item hair;

				if ((hair = FindItemOnLayer(Layer.FacialHair)) != null)
				{
					return hair.ItemID;
				}

				return 0;
			}
		}

		//SMD: support hook for new runuo-2.0 base networking
		[CommandProperty(AccessLevel.GameMaster)]
		public int HairHue
		{
			get
			{
				Item hair;

				if ((hair = FindItemOnLayer(Layer.Hair)) != null)
				{
					return hair.Hue;
				}

				return 0;
			}
		}

		//SMD: support hook for new runuo-2.0 base networking
		[CommandProperty(AccessLevel.GameMaster)]
		public int FacialHairHue
		{
			get
			{
				Item hair;

				if ((hair = FindItemOnLayer(Layer.FacialHair)) != null)
				{
					return hair.Hue;
				}

				return 0;
			}
		}
	}

	public enum Stat
	{
		Str,
		Dex,
		Int
	}
}
