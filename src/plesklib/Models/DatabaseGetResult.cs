﻿namespace plesklib.Models
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("packet")]
    public class DatabaseGetResult
    {
        public DatabaseGetResult()
        {
            this.databaseList = new List<DatabaseGetDatabaseGetDbNode>().ToArray();
        }

        [XmlArray("database")]
        [XmlArrayItem("get-db")]
        public DatabaseGetDatabaseGetDbNode[] databaseList { get; set; }
    }
    
    public class DatabaseGetDatabaseGetDbNode
    {
        public DatabaseGetDatabaseGetDbNode()
        {
            this.result = new DatabaseGetDatabaseResultNode();
        }
        
        [XmlElement("result")]
        public DatabaseGetDatabaseResultNode result { get; set; }
    }

    public class DatabaseGetDatabaseResultNode
    {
        [XmlElement("status")]
        public string status { get; set; }

        [XmlElement("filter-id")]
        public string filterId { get; set; }

        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("name")]
        public string name { get; set; }

        [XmlElement("type")]
        public string type { get; set; }

        [XmlElement("webspace-id")]
        public int webSpaceId { get; set; }

        [XmlElement("db-server-id")]
        public int dbsServerId { get; set; }

        [XmlElement("default-user-id")]
        public int defaultUserId { get; set; }
    }
}
