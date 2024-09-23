using System.ComponentModel;

namespace Paralax.Persistence.MongoDB
{
    public class MongoDbOptions
    {
        /// <summary>
        /// The MongoDB connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The name of the MongoDB database.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Indicates if the database should be seeded with initial data.
        /// </summary>
        public bool Seed { get; set; }

        /// <summary>
        /// Adds a random suffix to the database name, useful for integration testing scenarios.
        /// </summary>
        [Description("Might be helpful for integration testing.")]
        public bool SetRandomDatabaseSuffix { get; set; }

        /// <summary>
        /// Enables SSL/TLS encryption for connections to MongoDB.
        /// </summary>
        [Description("Enables SSL/TLS encryption for connections.")]
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// Maximum size of the connection pool.
        /// </summary>
        [Description("The maximum number of connections allowed in the connection pool.")]
        public int MaxConnectionPoolSize { get; set; } = 100;

        /// <summary>
        /// Minimum size of the connection pool.
        /// </summary>
        [Description("The minimum number of connections allowed in the connection pool.")]
        public int MinConnectionPoolSize { get; set; } = 0;

        /// <summary>
        /// Maximum time (in milliseconds) to wait for a server selection before throwing an exception.
        /// </summary>
        [Description("The maximum time (in milliseconds) to wait for server selection.")]
        public int ServerSelectionTimeoutMs { get; set; } = 30000; // 30 seconds

        /// <summary>
        /// Connection timeout in milliseconds.
        /// </summary>
        [Description("The amount of time (in milliseconds) to attempt to establish a connection before timing out.")]
        public int ConnectTimeoutMs { get; set; } = 10000; // 10 seconds

        /// <summary>
        /// Socket timeout in milliseconds.
        /// </summary>
        [Description("The amount of time (in milliseconds) to wait before closing an inactive socket.")]
        public int SocketTimeoutMs { get; set; } = 60000; // 1 minute

        /// <summary>
        /// Enables automatic retry of write operations on transient errors.
        /// </summary>
        [Description("Enables retrying of write operations on transient errors.")]
        public bool RetryWrites { get; set; } = true;

        /// <summary>
        /// Enables retrying of read operations on transient errors.
        /// </summary>
        [Description("Enables retrying of read operations on transient errors.")]
        public bool RetryReads { get; set; } = true;

        /// <summary>
        /// The optional username for authenticating with the MongoDB server.
        /// </summary>
        [Description("The optional username for authenticating with the MongoDB server.")]
        public string Username { get; set; }

        /// <summary>
        /// The optional password for authenticating with the MongoDB server.
        /// </summary>
        [Description("The optional password for authenticating with the MongoDB server.")]
        public string Password { get; set; }

        /// <summary>
        /// The optional authentication mechanism to use (e.g., SCRAM-SHA-256, MONGODB-X509).
        /// </summary>
        [Description("The optional authentication mechanism to use (e.g., SCRAM-SHA-256, MONGODB-X509).")]
        public string AuthenticationMechanism { get; set; }

        /// <summary>
        /// Whether to use compression when communicating with MongoDB.
        /// </summary>
        [Description("Indicates if compression should be used for communication with MongoDB.")]
        public bool UseCompression { get; set; } = false;
    }
}
