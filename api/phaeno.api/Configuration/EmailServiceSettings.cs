namespace phaeno.api.Configuration
{
    /// <summary>
    /// Email service settings (Mail Gun) from appsettings.json
    /// </summary>
    public sealed class EmailServiceSettings
    { 
        /// <summary>
        /// Mail Gun url
        /// </summary>
        public required string Url { get; set; }

        /// <summary>
        /// Resource Endpoint
        /// </summary>
        public required string Resource { get; set; }

        /// <summary>
        /// Secret Key
        /// </summary>
        public required string ApiKey { get; set; }

        /// <summary>
        /// Email address of sender
        /// </summary>
        public required string AccountFrom { get; set; }

        /// <summary>
        /// Default email address of Phaeno
        /// </summary>
        public required string AccountTo { get; set; }

        /// <summary>
        /// Path of sender
        /// </summary>
        public required string PhaenoAccountName { get; set; }
    }
}
