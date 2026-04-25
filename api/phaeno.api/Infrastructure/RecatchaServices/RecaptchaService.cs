
using Google.Apis.Auth.OAuth2;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.RecaptchaEnterprise.V1;
using Microsoft.Extensions.Options;
using phaeno.api.Configuration;

namespace phaeno.api.Infrastructure.RecatchaServices
{
    public class RecaptchaService(IOptions<GoogleAuthSettings> settings, IWebHostEnvironment env)
    {
        public async Task<bool> ValidReCaptchaCodeAsync(string token, string expectedAction, CancellationToken ct=default)
        {
            // 1. Quick sanity-checks up front.
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(expectedAction)) throw new ArgumentNullException(nameof(expectedAction));

            // 2. Resolve DI-backed settings.
            var opts = settings.Value;
            var threshold = opts.RecaptchaThreshold;
            var project = ProjectName.FromProject(opts.RecaptchaProjectId);

            var serviceAccountKeyJson = await ReadServiceAccountKeyJsonAsync(opts, ct);

            var clientBuilder = new RecaptchaEnterpriseServiceClientBuilder
            {
                GoogleCredential = CredentialFactory
                    .FromJson<ServiceAccountCredential>(serviceAccountKeyJson)
                    .ToGoogleCredential()
            };
            var client = clientBuilder.Build();

            // 3. Build the request.
            var request = new CreateAssessmentRequest
            {
                ParentAsProjectName = project,
                Assessment = new Assessment
                {
                    Event = new Event
                    {
                        SiteKey = opts.RecaptchaSecretKey,
                        Token = token,
                        ExpectedAction = expectedAction
                    }
                }
            };

            // 4. Call the API.
            var assessment = await client.CreateAssessmentAsync(request, cancellationToken: ct);

            // 5. Basic token integrity checks.
            if (!assessment.TokenProperties.Valid ||
                assessment.TokenProperties.Action != expectedAction)
            {
                // Optionally log the reasons for invalidity or action mismatch
                Console.WriteLine($"reCAPTCHA Token Invalid: {assessment.TokenProperties.Valid}");
                Console.WriteLine($"reCAPTCHA Action Mismatch: Expected '{expectedAction}', Got '{assessment.TokenProperties.Action}'");
                if (assessment.TokenProperties.InvalidReason != TokenProperties.Types.InvalidReason.Unspecified)
                {
                    Console.WriteLine($"Invalid Reason: {assessment.TokenProperties.InvalidReason}");
                }
                return false;
            }


            // 6. Evaluate the risk analysis.
            var score = assessment.RiskAnalysis.Score;
            var passed = score >= threshold;

            return passed;
        }

        private async Task<string> ReadServiceAccountKeyJsonAsync(
            GoogleAuthSettings opts,
            CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(opts.RecaptchaServiceAccountKeyJson))
                return opts.RecaptchaServiceAccountKeyJson;

            if (string.IsNullOrWhiteSpace(opts.RecaptchaServiceAccountKeyPath))
            {
                throw new InvalidOperationException(
                    "GoogleAuthSettings:RecaptchaServiceAccountKeyPath or GoogleAuthSettings:RecaptchaServiceAccountKeyJson is required.");
            }

            var keyPath = Path.IsPathRooted(opts.RecaptchaServiceAccountKeyPath)
                ? opts.RecaptchaServiceAccountKeyPath
                : Path.Combine(env.ContentRootPath, opts.RecaptchaServiceAccountKeyPath);

            if (!File.Exists(keyPath))
                throw new FileNotFoundException("reCAPTCHA Enterprise service account key JSON file was not found.", keyPath);

            var serviceAccountKeyJson = await File.ReadAllTextAsync(keyPath, ct);
            if (string.IsNullOrWhiteSpace(serviceAccountKeyJson))
            {
                throw new InvalidOperationException(
                    "reCAPTCHA Enterprise service account key JSON is empty.");
            }

            return serviceAccountKeyJson;
        }
    }
}
