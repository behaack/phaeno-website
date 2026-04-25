namespace phaeno.api.Configuration
{
    public sealed class GoogleAuthSettings
    {
        public required string SecretKeySalt { get; set; }

        public required string RecaptchaProjectId { get; set; }
        public required string RecaptchaSecretKey { get; set; }
        public required string RecaptchaUrl { get; set; }
        public string? RecaptchaServiceAccountKeyPath { get; set; }
        public string? RecaptchaServiceAccountKeyJson { get; set; }
        public float RecaptchaThreshold { get; set; }
    }
}
