using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Raydreams.Atlas
{
    /// <summary>Atlas Cluster Object</summary>
    /// <remarks>Not yet complete</remarks>
    public class AtlasCluster
    {
        [JsonProperty( "groupId" )]
        public string GroupID { get; set; }

        [JsonProperty( "id" )]
        public string ID { get; set; }

        [JsonProperty( "name" )]
        public string Name { get; set; }

        [JsonProperty( "paused" )]
        public bool Paused { get; set; }

        [JsonProperty( "pitEnabled" )]
        public bool PitEnabled { get; set; }

        [JsonProperty( "providerBackupEnabled" )]
        public bool ProviderBackupEnabled { get; set; }

        [JsonProperty( "backupEnabled" )]
        public bool BackupEnabled { get; set; }

        [JsonProperty( "clusterType" )]
        public string ClusterType { get; set; }

        [JsonProperty( "srvAddress" )]
        public string ServerAddress { get; set; }

        [JsonProperty( "stateName" )]
        public string StateName { get; set; }

        [JsonProperty( "numShards" )]
        public int NumberOfShards { get; set; }

        [JsonProperty( "replicationFactor" )]
        public int ReplicationFactor { get; set; }

        [JsonProperty( "createDate" )]
        public DateTime CreateDate { get; set; }

        [JsonProperty( "diskSizeGB" )]
        public double DiskSizeGB { get; set; }

        [JsonProperty( "mongoDBMajorVersion" )]
        public string MongoDBMajorVersion { get; set; }

        [JsonProperty( "mongoDBVersion" )]
        public string MongoDBVersion { get; set; }

        [JsonProperty( "mongoURI" )]
        public string MongoURI { get; set; }

        [JsonProperty( "mongoURIUpdated" )]
        public DateTime MongoURIUpdated { get; set; }

        [JsonProperty( "connectionStrings" )]
        public ClusterConnectionStrings ConnectionStrings { get; set; }

        [JsonProperty( "links" )]
        public List<AtlasLink> Links { get; set; }
    }

    /// <summary></summary>
    public class ClusterConnectionStrings
    {
        [JsonProperty( "standardSrv" )]
        public string StandardSrv { get; set; }

        [JsonProperty( "standard" )]
        public string Standard { get; set; }
    }

    /// <summary>top level response from the GetProjects call</summary>
    public class AtlasOrgProject
    {
        [JsonProperty( "links" )]
        public List<AtlasLink> Links { get; set; }

        [JsonProperty( "results" )]
        public List<AtlasProject> Results { get; set; }

        [JsonProperty( "totalCount" )]
        public int TotalCount { get; set; }
    }

    /// <summary>Atlas Project/Group Description</summary>
    public class AtlasProject
    {
        [JsonProperty( "clusterCount" )]
        public int ClusterCount { get; set; }

        [JsonProperty( "created" )]
        public DateTime Created { get; set; }

        [JsonProperty( "links" )]
        public List<AtlasLink> Links { get; set; }

        [JsonProperty( "name" )]
        public string Name { get; set; }

        [JsonProperty( "orgId" )]
        public string OrgID { get; set; }
    }

    /// <summary>top level response from the GetClusters call</summary>
    public class AtlasProjectClusters
    {
        [JsonProperty( "links" )]
        public List<AtlasLink> Links { get; set; }

        [JsonProperty( "results" )]
        public List<AtlasCluster> Results { get; set; }

        [JsonProperty( "totalCount" )]
        public int TotalCount { get; set; }
    }

    /// <summary></summary>
    public class AtlasLink
    {
        [JsonProperty( "href" )]
        public string Reference { get; set; }

        [JsonProperty( "rel" )]
        public string Relative { get; set; }
    }
}
