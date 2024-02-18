using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace BetaManager
{
    internal class FontManager
    {
        [DllImport( "gdi32", EntryPoint = "AddFontResource" )]
        public static extern int AddFontResourceA ( string lpFileName );
        [System.Runtime.InteropServices.DllImport( "gdi32.dll" )]
        private static extern int AddFontResource ( string lpszFilename );
        [System.Runtime.InteropServices.DllImport( "gdi32.dll" )]
        private static extern int CreateScalableFontResource ( uint fdwHidden, string
            lpszFontRes, string lpszFontFile, string lpszCurrentPath );
        public static bool IsFontInstalled ( string fontFamilyName )
        {
            using ( InstalledFontCollection installedFontCollection = new InstalledFontCollection() )
            {
                FontFamily[] fontFamilies = installedFontCollection.Families;
                foreach ( FontFamily fontFamily in fontFamilies )
                {
                    if ( fontFamily.Name == fontFamilyName )
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static void RegisterFont ( string contentFontName )
        {
            var fontDestination = Path.Combine( System.Environment.GetFolderPath( System.Environment.SpecialFolder.Fonts ), contentFontName );

            if ( !File.Exists( fontDestination ) )
            {
                System.IO.File.Copy( Path.Combine( System.IO.Directory.GetCurrentDirectory(), contentFontName ), fontDestination );

                PrivateFontCollection fontCol = new PrivateFontCollection();
                fontCol.AddFontFile( fontDestination );
                var actualFontName = fontCol.Families[0].Name;

                AddFontResource( fontDestination );
                Registry.SetValue( @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", actualFontName, contentFontName, RegistryValueKind.String );
            }
        }
        public static void InstallFont ( byte[] fontData )
        {
            IntPtr data = Marshal.AllocCoTaskMem( fontData.Length );
            Marshal.Copy( fontData, 0, data, fontData.Length );

            PrivateFontCollection fontCollection = new PrivateFontCollection();
            fontCollection.AddMemoryFont( data, fontData.Length );

            Marshal.FreeCoTaskMem( data );
        }
    }
}
