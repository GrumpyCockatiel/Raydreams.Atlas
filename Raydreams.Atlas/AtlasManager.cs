using System;
using System.Net.Http;
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raydreams.Atlas
{
    /// <summary>Delegate function for generating the CNonce value</summary>
    /// <param name="len">Length of the output nonce</param>
    /// <returns>a string</returns>
    public delegate string CNonceGenerator( int len );

    /// <summary>Encapsulates the WWW Auth Fields into a tuple</summary>
    /// <remarks>All these values are returned in first "login" to Auth Digest</remarks>
    public struct WWWAuthFields
    {
        public string Realm { get; set; }
        public string Nonce { get; set; }
        public string QoP { get; set; }
    }

    /// <summary>Manages Atlas Cloud via the APIs</summary>
    public class AtlasManager
    {
        #region [ Fields ]

        /// <summary>What user agent string do you want to send to Atlas</summary>
        private static readonly string _userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/601.3.9 (KHTML, like Gecko) Version/9.0.2 Safari/601.3.9";

        /// <summary>The Atlas API Authority</summary>
        private string _apiBase = "https://cloud.mongodb.com";

        /// <summary>The Atlas Base API Path</summary>
        private string _dir = "/api/atlas/v1.0";

        /// <summary>The API Private Key</summary>
        private string _privateKey = String.Empty;

        /// <summary>The API Public Key</summary>
        private string _publicKey = String.Empty;

        #endregion [ Fields ]

        #region [ Constructor ]

        /// <summary>Constructor</summary>
        /// <param name="publicKey"></param>
        /// <param name="privateKey"></param>
        public AtlasManager( string publicKey, string privateKey )
        {
            this._publicKey = String.IsNullOrWhiteSpace( publicKey ) ? String.Empty : publicKey.Trim();
            this._privateKey = String.IsNullOrWhiteSpace( privateKey ) ? String.Empty : privateKey.Trim();
        }

        #endregion [ Constructor ]

        // <summary>Create a new HTTP client to Atlas API</summary>
        protected HttpClient Client( Uri uri )
        {
            // setup the credentials
            CredentialCache creds = new CredentialCache
            {
                {
                    new Uri( uri.GetLeftPart( UriPartial.Authority ) ), // request url's host
                    "Digest",  // authentication type 
                    new NetworkCredential( _publicKey, _privateKey ) // credentials 
                }
            };

            // create the client and add some headers
            HttpClient client = new HttpClient( ( new HttpClientHandler { Credentials = creds } ) );
            client.DefaultRequestHeaders.Add( "Accept", "application/json" );
            client.DefaultRequestHeaders.Add( "Host", "cloud.mongodb.com" );
            client.DefaultRequestHeaders.Add( "User-Agent", _userAgent );
            client.DefaultRequestHeaders.Add( "Connection", "keep-alive" );

            // what kind of encoding do you want back
            string encoding = ( this.ZipResponse ) ? "gzip, deflate, br" : "json";
            client.DefaultRequestHeaders.Add( "Accept-Encoding", encoding );

            return client;
        }

        #region [ Properties ]

        /// <summary>If set to true the client with request a GZipped response</summary>
        public bool ZipResponse { get; set; } = false;

        /// <summary>Set this delegate to generate a CNonce value</summary>
        public CNonceGenerator Noncer { get; set; }

        #endregion [ Properties ]

        #region [ API Methods ]

        /// <summary>Gets the list of Projects (aka Groups) this key has access to</summary>
        /// <returns></returns>
        public async Task<AtlasOrgProject> GetProjects()
        {
            Uri uri = new Uri( $"{_apiBase}{_dir}/groups" );

            HttpResponseMessage final = null;

            try
            {
                final = await this.GetRequest( uri );
            }
            catch ( System.Exception )
            {
                throw;
            }

            string json = ( IsGZipped( final ) ) ? Decompress( final ) : await final.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AtlasOrgProject>( json );
        }

        /// <summary>Gets all the cluster info for all clusters in a Project</summary>
        /// <param name="projID"></param>
        /// <returns></returns>
        public async Task<AtlasProjectClusters> GetClusters( string projID )
        {
            if ( String.IsNullOrWhiteSpace( projID ) )
                return null;

            projID = projID.Trim();

            Uri uri = new Uri( $"{_apiBase}{_dir}/groups/{projID}/clusters" );

            HttpResponseMessage final = null;

            try
            {
                final = await this.GetRequest( uri );
            }
            catch ( System.Exception )
            {
                throw;
            }

            string json = ( IsGZipped( final ) ) ? Decompress( final ) : await final.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AtlasProjectClusters>( json );

        }

        /// <summary>Returns all the details of the specified cluster</summary>
        /// <param name="projID"></param>
        /// <param name="clusterName"></param>
        /// <returns></returns>
        public async Task<AtlasCluster> GetClusterInfo( string projID, string clusterName )
        {
            if ( String.IsNullOrWhiteSpace( projID ) || String.IsNullOrWhiteSpace( clusterName ) )
                return null;

            projID = projID.Trim();
            clusterName = clusterName.Trim();

            Uri uri = new Uri( $"{_apiBase}{_dir}/groups/{projID}/clusters/{clusterName}" );

            HttpResponseMessage final = null;

            try
            {
                final = await this.GetRequest( uri );
            }
            catch ( System.Exception )
            {
                throw;
            }

            string json = ( IsGZipped( final ) ) ? Decompress( final ) : await final.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AtlasCluster>( json );
        }

        /// <summary>Pause or resume the specified cluster</summary>
        /// <param name="projID"></param>
        /// <param name="clusterName"></param>
        /// <param name="pause">Pass true to pause the cluster, false to resume the cluster</param>
        /// <returns></returns>
        /// <remarks>PATCH is similar to POST but Pause is the only method using this for now so its not yet broken out</remarks>
        public async Task<AtlasCluster> PauseCluster( string projID, string clusterName, bool pause = false )
        {
            if ( String.IsNullOrWhiteSpace( projID ) || String.IsNullOrWhiteSpace( clusterName ) )
                return null;

            projID = projID.Trim();
            clusterName = clusterName.Trim();

            Uri uri = new Uri( $"{_apiBase}{_dir}/groups/{projID}/clusters/{clusterName}" );

            HttpResponseMessage final = null;

            try
            {
                // make a client
                HttpClient client = this.Client( uri );

                // roll the body
                string body = JsonConvert.SerializeObject( new { paused = pause } ).ToLower();

                HttpRequestMessage req1 = new HttpRequestMessage( HttpMethod.Patch, uri );
                req1.Headers.Clear();

                // add the body
                req1.Content = new StringContent( body, Encoding.UTF8, "application/json" );
                byte[] bytes = Encoding.UTF8.GetBytes( body );
                req1.Content.Headers.Add( "Content-Length", bytes.Length.ToString() );

                // send the first request
                HttpResponseMessage resp1 = await client.SendAsync( req1 );

                // at this point we expect a 401 with details for the Authorization header

                // pull apart the auth response
                string authHeader = resp1.Headers.WwwAuthenticate?.ToString();
                WWWAuthFields header = this.ParseWWWAuth( authHeader );

                // send a 2nd request with Authorization populated with details from www-authenticate
                HttpRequestMessage req2 = new HttpRequestMessage( HttpMethod.Patch, uri );
                req2.Headers.Clear();
                req2.Headers.Add( "Authorization", this.GetDigestHeader( uri.AbsolutePath, header, HttpMethod.Patch ) );

                // add the body again
                req2.Content = req1.Content;

                // send the 2nd request
                final = await client.SendAsync( req2 );
            }
            catch ( System.Exception )
            {
                throw;
            }

            string json = ( IsGZipped( final ) ) ? Decompress( final ) : await final.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AtlasCluster>( json );
        }

        #endregion [ API Methods ]

        #region [ Private Methods ]

        /// <summary>Rolls a generic GET request to Atlas</summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> GetRequest( Uri uri )
        {
            HttpResponseMessage final = null;

            if ( uri == null || String.IsNullOrWhiteSpace( uri.AbsoluteUri ) )
                return final;

            // setup a new client
            HttpClient client = this.Client( uri );

            // setup the first request
            HttpRequestMessage req1 = new HttpRequestMessage( HttpMethod.Get, uri );
            req1.Headers.Clear();

            // send the 'login' request
            HttpResponseMessage resp1 = await client.SendAsync( req1 );

            // at this point we expect a 401 with details for the Authorization header

            // pull apart the auth response
            string authHeader = resp1.Headers.WwwAuthenticate?.ToString();
            WWWAuthFields header = this.ParseWWWAuth( authHeader );

            // send a 2nd request with Authorization populated with details from www-authenticate
            HttpRequestMessage req2 = new HttpRequestMessage( HttpMethod.Get, uri );
            req2.Headers.Clear();
            req2.Headers.Add( "Authorization", GetDigestHeader( uri.AbsolutePath, header, HttpMethod.Get ) );
            final = await client.SendAsync( req2 );

            return final;
        }

        /// <summary>Checks to see if the Content Response is GZipped</summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        private static bool IsGZipped( HttpResponseMessage resp )
        {
            if ( resp == null || resp.Content == null || resp.Content.Headers == null )
                return false;

            if ( resp.Content.Headers.Count() < 1 || resp.Content.Headers.ContentEncoding == null )
                return false;

            return ( resp.Content.Headers.ContentEncoding.Where( i => i == "gzip" ).FirstOrDefault() != null );
        }

        /// <summary>Create an Auth Header Digest</summary>
        /// <remarks>If you continue to use the orginal Auth request you need to increment the nonce value each request</remarks>
        private string GetDigestHeader( string dir, WWWAuthFields header, HttpMethod method )
        {
            // increment on subsequent calls if you re-use
            int nc = 1;

            // create a client nonce
            string cnonce = this.Noncer( 8 );

            string h1 = $"{_publicKey}:{header.Realm}:{_privateKey}".HashToMD5();
            string h2 = $"{method.ToString().ToUpperInvariant()}:{dir}".HashToMD5();
            string resp = $"{h1}:{header.Nonce}:{nc:00000000}:{cnonce}:{header.QoP}:{h2}".HashToMD5();

            return $"Digest username=\"{_publicKey}\", realm=\"{header.Realm}\", nonce=\"{header.Nonce}\", uri=\"{dir}\", algorithm=MD5, response=\"{resp}\", qop={header.QoP}, nc={nc:00000000}, cnonce=\"{cnonce}\"";
        }

        /// <summary>Parses all the WWW Auth Fields to a struct</summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <remarks>
        /// Use whatever parsing technique makes you happy. A couple are in StringExtensions
        /// An www auth header will look something like this
        /// Digest realm="MMS Public API", domain="", nonce="kWVA9Ciu7lNaN5QdjPe8kxPMReVjbt+B", algorithm=MD5, qop="auth", stale=false
        /// </remarks>
        private WWWAuthFields ParseWWWAuth( string header )
        {
            WWWAuthFields results = new WWWAuthFields();

            if ( String.IsNullOrWhiteSpace( header ) )
                return results;

            Dictionary<string, string> dict = header.PairsToDictionary(true);

            results.Realm = ( dict.ContainsKey( "realm" ) ) ? dict["realm"] : String.Empty;
            results.Nonce = ( dict.ContainsKey( "nonce" ) ) ? dict["nonce"] : String.Empty;
            results.QoP = ( dict.ContainsKey( "qop" ) ) ? dict["qop"] : String.Empty;

            return results;
        }

        /// <summary>Decompress a GZip stream into its original bytes</summary>
        public static string Decompress( HttpResponseMessage resp )
        {
            Stream source = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

            // read the decompressed length from the end of the stream
            byte[] sa = new byte[4];
            source.Seek( -4, SeekOrigin.End );
            source.Read( sa, 0, 4 );
            int len = BitConverter.ToInt32( sa, 0 );

            // reset the pointer position
            source.Position = 0;

            // read the complete stream
            using var decom = new GZipStream( source, CompressionMode.Decompress );
            byte[] result = new byte[len];
            decom.Read( result, 0, len );

            return Encoding.UTF8.GetString( result );
        }

        #endregion [ Private Methods ]

    }
}
