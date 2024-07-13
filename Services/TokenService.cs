using Microsoft.Identity.Client;

namespace LudoLab_ConnectSys_Server.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IConfidentialClientApplication _clientApp;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            var clientSecret = _configuration["AzureAd:ClientSecret"];
            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentNullException(nameof(clientSecret), "Client secret cannot be null or empty.");
            }

            _clientApp = ConfidentialClientApplicationBuilder.Create(_configuration["AzureAd:ClientId"])
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri(_configuration["AzureAd:Authority"]))
                .Build();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var scopes = new string[] { $"{_configuration["MicrosoftGraph:BaseUrl"]}/.default" };
            var result = await _clientApp.AcquireTokenForClient(scopes).ExecuteAsync();
            return result.AccessToken;
        }
    }



}
