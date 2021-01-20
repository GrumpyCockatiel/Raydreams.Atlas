using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Raydreams.Atlas
{
    /// <summary>Some String Extension helpers you may already have similar in your code.</summary>
    public static class StringExtensions
    {
        /// <summary>Given a string in the format key1=value1,key2=value2,key3=value3 splits into a dictionary</summary>
        public static Dictionary<string, string> PairsToDictionary( this string str, bool stripQuotes = true )
        {
            Dictionary<string, string> results = new Dictionary<string, string>();

            StringBuilder temp = new StringBuilder();

            foreach ( char c in str )
            {
                if ( c == ',' )
                {
                    string[] parts = temp.ToString().Split( '=', StringSplitOptions.None );
                    if ( parts != null && parts.Length > 0 && !String.IsNullOrWhiteSpace( parts[0] ) )
                    {
                        parts[1] = ( parts.Length < 2 || String.IsNullOrWhiteSpace( parts[1] ) ) ? String.Empty : parts[1].Trim();
                        if ( stripQuotes )
                            parts[1] = parts[1].Replace( "\"", "" );
                        results.Add( parts[0].Trim(), parts[1] );
                    }

                    temp = new StringBuilder();
                }
                else
                {
                    temp.Append( c );
                }
            }

            return results;
        }

        /// <summary>Get a value from the string in the format var=value</summary>
        public static string ParseField( this string str, string var )
        {
            Match match = new Regex( String.Format( @"{0}=""([^""]*)""", var ) ).Match( str );

            if ( !match.Success )
                return String.Empty;

            return match.Groups[1].Value;
        }

        /// <summary>Hashing an input string to its MD5 checksum and returns in hex format</summary>
        public static string HashToMD5( this string str )
        {
            byte[] hash = MD5.Create().ComputeHash( Encoding.ASCII.GetBytes( str ) );

            return String.Join( String.Empty, Array.ConvertAll( hash, b => b.ToString( "X2" ) ) ).ToLowerInvariant();
        }
    }
}
