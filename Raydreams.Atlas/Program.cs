using System;
using System.Configuration;
using Newtonsoft.Json;

namespace Raydreams.Atlas
{
    /// <remarks>
    /// You need to include an app.config file with keys
    /// For obvious security reasons it has been removed
    /// <?xml version="1.0" encoding="utf-8"?>
    /// <configuration>
    ///     <appSettings>
    ///         <add key = "publicKey" value=""/>
    ///         <add key = "privateKey" value=""/>
    ///         <add key = "projectID" value=""/>
    ///         <add key = "clusterName" value=""/>
    ///     </appSettings>
    /// </configuration>
    /// </remarks>
    public class Program
    {
        /// <summary>The API Public Key found in Atlas</summary>
        private static string publicKey = String.Empty;

        /// <summary>The API provate key shown only ONCE in Atlas when it was created</summary>
        private static string privateKey = String.Empty;

        /// <summary>The Project or Group ID you can find in the URL or by querying with Get Projects</summary>
        private static string projectID = String.Empty;

        /// <summary>The actual name of a cluster as seen in the Atlas dashboard</summary>
        private static string clusterName = String.Empty;

        /// <summary>Some randomizer to generator a client nonce. You can make your own.</summary>
        private static Randomizer rnd = new Randomizer();

        /// <summary>Main entry</summary>
        /// <param name="args">Use this as a script exe by passing in the above consts instead</param>
        /// <returns>0 if all is cool, else -1</returns>
        static int Main( string[] args )
        {
            try
            {
                // get all the settings from app.config
                publicKey = ConfigurationManager.AppSettings["publicKey"];
                privateKey = ConfigurationManager.AppSettings["privateKey"];
                projectID = ConfigurationManager.AppSettings["projectID"];
                clusterName = ConfigurationManager.AppSettings["clusterName"];

                // setup the manager
                AtlasManager app = new AtlasManager( publicKey, privateKey )
                {
                    // set the noncer to get a random code of n chars, we're using 8
                    Noncer = (int n) => { return rnd.RandomCode( n ); }
                };

                // get the org's list of projects
                //AtlasOrgProject results = app.GetProjects().GetAwaiter().GetResult();

                // get all the clusters in a project
                //AtlasProjectClusters results = app.GetClusters( projectID ).GetAwaiter().GetResult();

                // get info about the specified cluster
                //AtlasCluster results = app.GetClusterInfo( projectID, clusterName ).GetAwaiter().GetResult();

                // pause or resume a cluster by name, true will pause, false will resume
                AtlasCluster results = app.PauseCluster( projectID, clusterName, false ).GetAwaiter().GetResult();

                // just dump the results for now
                Console.WriteLine( $"{JsonConvert.SerializeObject( results )}" );
            }
            catch (System.Exception exp)
            {
                Console.WriteLine(exp.Message);
                return -1;
            }

            return 0;
        }
    }
}
