﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook
{
    /// <summary>
    /// Provides a managed interface to the native thread ACLs.
    /// </summary>
    /// <remarks>
    /// Refer to the official guide to learn more about why thread ACLs are useful. 
    /// They can be used to exclude/include dedicated threads from interception or to dynamically
    /// apply different kind of hooks to different threads. Even if you could do this
    /// in managed code, it is not that easy to implement and also EasyHook evaluates
    /// those ACLs in unmanaged code. So if any thread is not intercepted, it will never
    /// enter the manged environment what will speed up things about orders of magnitudes.
    /// </remarks>
    public class HookAccessControl
    {
        private int[] m_ACL = new int[0];
        private IntPtr m_Handle;
        private bool m_IsExclusive;

        /// <summary>
        /// Is this ACL an exclusive one? Refer to <see cref="SetExclusiveACL"/> for more information.
        /// </summary>
        public bool IsExclusive { get { return m_IsExclusive; } }
        /// <summary>
        /// Is this ACL an inclusive one? Refer to <see cref="SetInclusiveACL"/> for more information.
        /// </summary>
        public bool IsInclusive { get { return !IsExclusive; } }

        /// <summary>
        /// Sets an inclusive ACL. This means all threads that are enumerated through <paramref name="InACL"/>
        /// are intercepted while all others are NOT. Of course this will overwrite the existing ACL.
        /// </summary>
        /// <remarks>
        /// Please note that this is not necessarily the final
        /// negotiation result. Refer to <see cref="LocalHook.IsThreadIntercepted"/> for more information.
        /// In general inclusive ACLs will restrict exclusive ACLs while local ACLs will overwrite the
        /// global ACL.
        /// </remarks>
        /// <param name="InACL">Threads to be explicitly included in negotiation.</param>
        /// <exception cref="ArgumentException">
        /// The limit of 128 access entries is exceeded!
        /// </exception>
        public void SetInclusiveACL(int[] InACL)
        {
            if (InACL == null)
                m_ACL = new int[0];
            else
                m_ACL = (int[])InACL.Clone();

            m_IsExclusive = false;

            if (m_Handle == IntPtr.Zero)
                NativeAPI.DetourSetGlobalInclusiveACL(m_ACL, m_ACL.Length);
            else
                NativeAPI.DetourSetInclusiveACL(m_ACL, m_ACL.Length, m_Handle);
        }

        /// <summary>
        /// Sets an exclusive ACL. This means all threads that are enumerated through <paramref name="InACL"/>
        /// are NOT intercepted while all others are. Of course this will overwrite the existing ACL.
        /// </summary>
        /// <remarks>
        /// Please note that this is not necessarily the final
        /// negotiation result. Refer to <see cref="LocalHook.IsThreadIntercepted"/> for more information.
        /// In general inclusive ACLs will restrict exclusive ACLs while local ACLs will overwrite the
        /// global ACL.
        /// </remarks>
        /// <param name="InACL">Threads to be explicitly included in negotiation.</param>
        /// <exception cref="ArgumentException">
        /// The limit of 128 access entries is exceeded!
        /// </exception>
        public void SetExclusiveACL(int[] InACL)
        {
            if (InACL == null)
            {
                m_ACL = new int[0];
            }
            else
            {
                m_ACL = (int[])InACL.Clone();
            }

            m_IsExclusive = true;

            if (m_Handle == IntPtr.Zero)
            {
                NativeAPI.DetourSetGlobalExclusiveACL(m_ACL, m_ACL.Length);
            }
            else
            {
                NativeAPI.DetourSetExclusiveACL(m_ACL, m_ACL.Length, m_Handle);
            }
        }

        /// <summary>
        /// Creates a copy of the internal thread list associated with this ACL. You may freely
        /// modify it without affecting the internal entries.
        /// </summary>
        /// <returns>
        /// A copy of the internal thread entries.
        /// </returns>
        public int[] GetEntries()
        {
            return (int[])m_ACL.Clone();
        }

        internal HookAccessControl(IntPtr InHandle)
        {
            if (InHandle == IntPtr.Zero)
            {
                m_IsExclusive = true;
            }
            else
            {
                m_IsExclusive = false;
            }
            m_Handle = InHandle;
        }
    }
}
