﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Shark_Remote.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Shark_Remote.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap add_user {
            get {
                object obj = ResourceManager.GetObject("add_user", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap bot_control {
            get {
                object obj = ResourceManager.GetObject("bot_control", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap create_package {
            get {
                object obj = ResourceManager.GetObject("create_package", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap delete_user {
            get {
                object obj = ResourceManager.GetObject("delete_user", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap DevPlugin {
            get {
                object obj = ResourceManager.GetObject("DevPlugin", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap documentation {
            get {
                object obj = ResourceManager.GetObject("documentation", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @for /f %%i in (&apos;@wmic path win32_desktopmonitor get pnpdeviceid ^|@find &quot;DISPLAY&quot;&apos;) do @set val=&quot;HKLM\SYSTEM\CurrentControlSet\Enum\%%i\Device Parameters&quot;
        ///@reg query %val% /v EDID&gt;NUL
        ///@if %errorlevel% GTR 0 @echo BAD EDID&amp;EXIT
        ///@for /f &quot;skip=2 tokens=1,2,3*&quot; %%a in (&apos;@reg query %val% /v EDID&apos;) do @set edid=%%c
        ///@set /A Y=%edid:~34,1%*16+%edid:~35,1%+1990
        ///@echo.%Y%.
        /// </summary>
        public static string getmon {
            get {
                return ResourceManager.GetString("getmon", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap loading {
            get {
                object obj = ResourceManager.GetObject("loading", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap plugins {
            get {
                object obj = ResourceManager.GetObject("plugins", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap settings {
            get {
                object obj = ResourceManager.GetObject("settings", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap site {
            get {
                object obj = ResourceManager.GetObject("site", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap telegram {
            get {
                object obj = ResourceManager.GetObject("telegram", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap telegram_developer {
            get {
                object obj = ResourceManager.GetObject("telegram_developer", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap window_mode {
            get {
                object obj = ResourceManager.GetObject("window_mode", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;service&gt;
        ///  &lt;id&gt;SRServer&lt;/id&gt;
        ///  &lt;name&gt;Shark Remote&lt;/name&gt;
        ///  &lt;description&gt;An application to create a convenient and functional bot for remote PC control from the Telegram messenger. Used as a Windows service.&lt;/description&gt;&gt;
        ///  &lt;executable&gt;Shark Remote&lt;/executable&gt;
        ///  &lt;stoptimeout&gt;10sec&lt;/stoptimeout&gt;
        ///  &lt;arguments&gt;--server&lt;/arguments&gt;
        ///  &lt;log mode=&quot;roll&quot;&gt;&lt;/log&gt;
        ///  &lt;interactive&gt;true&lt;/interactive&gt;
        ///  &lt;workingdirectory&gt;Path to Shark Remote.exe here&lt;/workingdirectory&gt;
        ///  &lt;serviceaccount&gt;
        ///		&lt;allowservicelogon&gt; [rest of string was truncated]&quot;;.
        /// </summary>
        public static string WinSW {
            get {
                return ResourceManager.GetString("WinSW", resourceCulture);
            }
        }
    }
}
