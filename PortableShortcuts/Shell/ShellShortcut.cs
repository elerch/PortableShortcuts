/**************************************************************************
*
* Filename:     ShellShortcut.cs
* Author:       Mattias Sjögren (mattias@mvps.org)
*               http://www.msjogren.net/dotnet/
*
* Description:  Defines a .NET friendly class, ShellShortcut, for reading
*               and writing shortcuts.
*               Define the conditional compilation symbol UNICODE to use
*               IShellLinkW internally.
*
* Public types: class ShellShortcut
*
*
* Dependencies: ShellLinkNative.cs
*
*
* Copyright ©2001-2002, Mattias Sjögren
* 
**************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace PortableShortcuts.Shell
{
    /// <remarks>
    ///   .NET friendly wrapper for the ShellLink class
    /// </remarks>
    public class ShellShortcut : IDisposable
    {
        private const int INFOTIPSIZE = 1024;
        private const int MAX_PATH = 260;

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOWMINNOACTIVE = 7;


#if UNICODE
        private IShellLinkW _nativeLink;
#else
        private IShellLinkA _nativeLink;
#endif
        private readonly string _path;

        ///
        /// <param name='linkPath'>
        ///   Path to new or existing shortcut file (.lnk).
        /// </param>
        ///
        public ShellShortcut(string linkPath)
        {
            IPersistFile pf;

            _path = linkPath;

#if UNICODE
            _nativeLink = (IShellLinkW) new ShellLink();
#else
            _nativeLink = (IShellLinkA)new ShellLink();
#endif

            if (File.Exists(linkPath)) {
                pf = (IPersistFile)_nativeLink;
                pf.Load(linkPath, 0);
            }

        }

        //
        //  IDisplosable implementation
        //
        public void Dispose()
        {
            if (_nativeLink != null) {
                Marshal.ReleaseComObject(_nativeLink);
                _nativeLink = null;
            }
        }

        /// <value>
        ///   Gets or sets the argument list of the shortcut.
        /// </value>
        public string Arguments
        {
            get
            {
                var sb = new StringBuilder(INFOTIPSIZE);
                _nativeLink.GetArguments(sb, sb.Capacity);
                return sb.ToString();
            }
            set { _nativeLink.SetArguments(value); }
        }

        /// <value>
        ///   Gets or sets a description of the shortcut.
        /// </value>
        public string Description
        {
            get
            {
                var sb = new StringBuilder(INFOTIPSIZE);
                _nativeLink.GetDescription(sb, sb.Capacity);
                return sb.ToString();
            }
            set { _nativeLink.SetDescription(value); }
        }

        /// <value>
        ///   Gets or sets the working directory (aka start in directory) of the shortcut.
        /// </value>
        public string WorkingDirectory
        {
            get
            {
                var sb = new StringBuilder(MAX_PATH);
                _nativeLink.GetWorkingDirectory(sb, sb.Capacity);
                return sb.ToString();
            }
            set { _nativeLink.SetWorkingDirectory(value); }
        }

        //
        // If Path returns an empty string, the shortcut is associated with
        // a PIDL instead, which can be retrieved with IShellLink.GetIDList().
        // This is beyond the scope of this wrapper class.
        //
        /// <value>
        ///   Gets or sets the target path of the shortcut.
        /// </value>
        public string Path
        {
            get
            {
#if UNICODE
                var wfd = new WIN32_FIND_DATAW();
#else
// ReSharper disable RedundantAssignment
                var wfd = new WIN32_FIND_DATAA();
// ReSharper restore RedundantAssignment
#endif
                var sb = new StringBuilder(MAX_PATH);

                _nativeLink.GetPath(sb, sb.Capacity, out wfd, SLGP_FLAGS.SLGP_UNCPRIORITY);
                return sb.ToString();
            }
            set { _nativeLink.SetPath(value); }
        }

        /// <value>
        ///   Gets or sets the path of the <see cref="Icon"/> assigned to the shortcut.
        /// </value>
        /// <summary>
        ///   <seealso cref="IconIndex"/>
        /// </summary>
        public string IconPath
        {
            get
            {
                var sb = new StringBuilder(MAX_PATH);
                int nIconIdx;
                _nativeLink.GetIconLocation(sb, sb.Capacity, out nIconIdx);
                return sb.ToString();
            }
            set { _nativeLink.SetIconLocation(value, IconIndex); }
        }

        /// <value>
        ///   Gets or sets the index of the <see cref="Icon"/> assigned to the shortcut.
        ///   Set to zero when the <see cref="IconPath"/> property specifies a .ICO file.
        /// </value>
        /// <summary>
        ///   <seealso cref="IconPath"/>
        /// </summary>
        public int IconIndex
        {
            get
            {
                var sb = new StringBuilder(MAX_PATH);
                int nIconIdx;
                _nativeLink.GetIconLocation(sb, sb.Capacity, out nIconIdx);
                return nIconIdx;
            }
            set { _nativeLink.SetIconLocation(IconPath, value); }
        }

        /// <value>
        ///   Retrieves the Icon of the shortcut as it will appear in Explorer.
        ///   Use the <see cref="IconPath"/> and <see cref="IconIndex"/>
        ///   properties to change it.
        /// </value>
        public Icon Icon
        {
            get
            {
                var sb = new StringBuilder(MAX_PATH);
                int nIconIdx;


                _nativeLink.GetIconLocation(sb, sb.Capacity, out nIconIdx);
                var hInst = Marshal.GetHINSTANCE(GetType().Module);
                var hIcon = Native.ExtractIcon(hInst, sb.ToString(), nIconIdx);
                if (hIcon == IntPtr.Zero)
                    return null;

                // Return a cloned Icon, because we have to free the original ourselves.
                var ico = Icon.FromHandle(hIcon);
                var clone = (Icon)ico.Clone();
                ico.Dispose();
                Native.DestroyIcon(hIcon);
                return clone;
            }
        }

        /// <value>
        ///   Gets or sets the System.Diagnostics.ProcessWindowStyle value
        ///   that decides the initial show state of the shortcut target.
        ///   Keep in mind that ProcessWindowStyle.Hidden is not a valid property value.
        /// </value>
        public ProcessWindowStyle WindowStyle
        {
            get
            {
                int nWs;
                _nativeLink.GetShowCmd(out nWs);

                switch (nWs) {
                    case SW_SHOWMINIMIZED:
                    case SW_SHOWMINNOACTIVE:
                        return ProcessWindowStyle.Minimized;

                    case SW_SHOWMAXIMIZED:
                        return ProcessWindowStyle.Maximized;

                    default:
                        return ProcessWindowStyle.Normal;
                }
            }
            set
            {
                int nWs;

                switch (value) {
                    case ProcessWindowStyle.Normal:
                        nWs = SW_SHOWNORMAL;
                        break;

                    case ProcessWindowStyle.Minimized:
                        nWs = SW_SHOWMINNOACTIVE;
                        break;

                    case ProcessWindowStyle.Maximized:
                        nWs = SW_SHOWMAXIMIZED;
                        break;

                    default: // ProcessWindowStyle.Hidden
                        throw new ArgumentException("Unsupported ProcessWindowStyle value.");
                }

                _nativeLink.SetShowCmd(nWs);

            }
        }

        /// <value>
        ///   Gets or sets the hotkey for the shortcut.
        /// </value>
        public Keys Hotkey
        {
            get
            {
                short wHotkey;

                _nativeLink.GetHotkey(out wHotkey);

                //
                // Convert from IShellLink 16-bit format to Keys enumeration 32-bit value
                // IShellLink: 0xMMVK
                // Keys:  0x00MM00VK        
                //   MM = Modifier (Alt, Control, Shift)
                //   VK = Virtual key code
                //       
                int dwHotkey = ((wHotkey & 0xFF00) << 8) | (wHotkey & 0xFF);
                return (Keys)dwHotkey;
            }
            set
            {
                if ((value & Keys.Modifiers) == 0)
                    throw new ArgumentException("Hotkey must include a modifier key.");

                //    
                // Convert from Keys enumeration 32-bit value to IShellLink 16-bit format
                // IShellLink: 0xMMVK
                // Keys:  0x00MM00VK        
                //   MM = Modifier (Alt, Control, Shift)
                //   VK = Virtual key code
                //       
                var wHotkey = unchecked((short)(((int)(value & Keys.Modifiers) >> 8) | (int)(value & Keys.KeyCode)));
                _nativeLink.SetHotkey(wHotkey);

            }
        }

        /// <summary>
        ///   Saves the shortcut to disk.
        /// </summary>
        public void Save()
        {
            var pf = (IPersistFile)_nativeLink;
            pf.Save(_path, true);
        }

        /// <summary>
        ///   Returns a reference to the internal ShellLink object,
        ///   which can be used to perform more advanced operations
        ///   not supported by this wrapper class, by using the
        ///   IShellLink interface directly.
        /// </summary>
        public object ShellLink
        {
            get { return _nativeLink; }
        }


        #region Native Win32 API functions
        private static class Native
        {
            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

            [DllImport("user32.dll")]
            public static extern bool DestroyIcon(IntPtr hIcon);
        }
        #endregion

    }
}
