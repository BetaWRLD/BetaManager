using System;
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BetaManager
{
    public class FontProvider
    {
        public static FontFamily LoadFontFamily ( string resourceName )
        {
            var assembly = Assembly.GetExecutingAssembly(); // Change this if needed
            var stream = assembly.GetManifestResourceStream( resourceName );

            if ( stream != null )
            {
                var fontData = new byte[stream.Length];
                stream.Read( fontData, 0, ( int )stream.Length );

                IntPtr data = Marshal.AllocCoTaskMem( fontData.Length );
                Marshal.Copy( fontData, 0, data, fontData.Length );

                var fontCollection = new PrivateFontCollection();
                fontCollection.AddMemoryFont( data, fontData.Length );

                Marshal.FreeCoTaskMem( data );

                return new FontFamily( fontCollection.Families[0].Name );
            }

            return new FontFamily( "Arial" ); // Fallback font if loading fails
        }
    }
}