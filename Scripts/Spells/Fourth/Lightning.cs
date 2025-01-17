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

/* Scripts/Spells/Base/Spell.cs
	ChangeLog:
	6/03/06, Kit
		Added damage type define
	5/15/06, Kit
		Added MinDamage/MaxDamage define for damage vs hardcode.
	7/4/04, Pix
		Added caster to SpellHelper.Damage call so corpse wouldn't stay blue to this caster
	6/5/04, Pix
		Merged in 1.0RC0 code.
	3/25/04 changes by smerX:
		Changed DamageDelay to 0.35	
	3/18/04 code changes by smerX:
		Added 0.3 second DamageDelay
*/
using System;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.Fourth
{
	public class LightningSpell : Spell
	{
		public override int MinDamage{get{return 9;}}
		public override int MaxDamage{get{return 12;}}
		public override SpellDamageType DamageType
		{
			get
			{
				return SpellDamageType.Electrical;
			}
		}

		private static SpellInfo m_Info = new SpellInfo(
				"Lightning", "Por Ort Grav",
				SpellCircle.Fourth,
				239,
				9021,
				Reagent.MandrakeRoot,
				Reagent.SulfurousAsh
			);

		public LightningSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public override bool DelayedDamage{ get{ return false; } }

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( CheckHSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );

				SpellHelper.CheckReflect( (int)this.Circle, Caster, ref m );

				double damage;

				if ( Core.AOS )
				{
					damage = GetNewAosDamage( 22, 1, 4 );
				}
				else
				{
					damage = Utility.Random( MinDamage, MaxDamage );

					if ( CheckResisted( m ) )
					{
						damage *= 0.75;

						m.SendLocalizedMessage( 501783 ); // You feel yourself resisting magical energy.
					}

					damage *= GetDamageScalar( m );
				}

				m.BoltEffect( 0 );

				//Pixie: 7/4/04: added caster to this so corpse wouldn't stay blue to this caster
				SpellHelper.Damage( TimeSpan.FromSeconds( 0.35 ), m, Caster, damage, 0, 0, 0, 0, 100 );
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private LightningSpell m_Owner;

			public InternalTarget( LightningSpell owner ) : base( 12, false, TargetFlags.Harmful )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
					m_Owner.Target( (Mobile)o );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}