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

/* Server/TimedLock.cs
 * Changelog
 *  01/18/06 Taran Kain
 *		Changed default timeout from 10 sec to 1.0 sec.
 *  01/18/06 Taran Kain
 *		Initial version. Thanks go to Ian Griffth (www.interact-sw.co.uk/iangblog/) for this code.
 */

using System;
using System.Threading;

// Thanks to Eric Gunnerson for recommending this be a struct rather
// than a class - avoids a heap allocation.
// Thanks to Change Gillespie and Jocelyn Coulmance for pointing out
// the bugs that then crept in when I changed it to use struct...
// Thanks to John Sands for providing the necessary incentive to make
// me invent a way of using a struct in both release and debug builds
// without losing the debug leak tracking.

namespace Server
{
	public struct TimedLock : IDisposable
	{
		public static TimedLock Lock(object o)
		{
			return Lock(o, TimeSpan.FromMilliseconds(1000));
		}

		public static TimedLock Lock(object o, TimeSpan timeout)
		{
			TimedLock tl = new TimedLock(o);
			if (!Monitor.TryEnter(o, timeout))
			{
#if DEBUG
            System.GC.SuppressFinalize(tl.leakDetector);
#endif
				throw new LockTimeoutException();
			}

			return tl;
		}

		private TimedLock(object o)
		{
			target = o;
#if DEBUG
        leakDetector = new Sentinel();
#endif
		}
		private object target;

		public void Dispose()
		{
			Monitor.Exit(target);

			// It's a bad error if someone forgets to call Dispose,
			// so in Debug builds, we put a finalizer in to detect
			// the error. If Dispose is called, we suppress the
			// finalizer.
#if DEBUG
        GC.SuppressFinalize(leakDetector);
#endif
		}

#if DEBUG
    // (In Debug mode, we make it a class so that we can add a finalizer
    // in order to detect when the object is not freed.)
    private class Sentinel
    {
        ~Sentinel()
        {
            // If this finalizer runs, someone somewhere failed to
            // call Dispose, which means we've failed to leave
            // a monitor!
            System.Diagnostics.Debug.Fail("Undisposed lock");
        }
    }
    private Sentinel leakDetector;
#endif

	}

	public class LockTimeoutException : ApplicationException
	{
		public LockTimeoutException()
			: base("Timeout waiting for lock")
		{
		}
	}
}