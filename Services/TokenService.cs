/*using Microsoft.Identity.Client;
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



}*/


/*using Microsoft.Identity.Client;
using System.Threading.Tasks;
namespace LudoLab_ConnectSys_Server.Services
{
    public class TokenService
    {
        private readonly IConfidentialClientApplication _confidentialClientApplication;
        private readonly string[] _scopes;

        public TokenService(IConfiguration configuration)
        {
            // Configura MSAL con los parámetros del appsettings.json
            var azureAdOptions = configuration.GetSection("AzureAd").Get<AzureAdOptions>();

            _confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(azureAdOptions.ClientId)
                .WithClientSecret(azureAdOptions.ClientSecret)
                .WithAuthority(new Uri(azureAdOptions.Authority))
                .Build();

            // Define los scopes necesarios
            _scopes = configuration.GetSection("MicrosoftGraph:Scopes").Get<string[]>();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // Intenta obtener el token desde la caché
            var accounts = await _confidentialClientApplication.GetAccountsAsync();
            var account = accounts.FirstOrDefault();

            try
            {
                var result = await _confidentialClientApplication
                    .AcquireTokenSilent(_scopes, account)
                    .ExecuteAsync();

                return result.AccessToken;
            }
            catch (MsalUiRequiredException)
            {
                // Si no se puede obtener el token en silencio, usa el ClientCredentials para obtener uno nuevo
                var result = await _confidentialClientApplication
                    .AcquireTokenForClient(_scopes)
                    .ExecuteAsync();

                return result.AccessToken;
            }
        }
    }

    public class AzureAdOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
    }
}*/

using Microsoft.Identity.Client;
using System.Threading.Tasks;
namespace LudoLab_ConnectSys_Server.Services
{
    public class TokenService
    {
        private readonly IConfidentialClientApplication _confidentialClientApplication;
        private readonly string[] _scopes;

        public TokenService(IConfiguration configuration)
        {
            // Configura MSAL con los parámetros del appsettings.json
            var azureAdOptions = configuration.GetSection("AzureAd").Get<AzureAdOptions>();

            _confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(azureAdOptions.ClientId)
                .WithClientSecret(azureAdOptions.ClientSecret)
                .WithAuthority(new Uri(azureAdOptions.Authority))
                .Build();

            // Define los scopes necesarios
            _scopes = configuration.GetSection("MicrosoftGraph:Scopes").Get<string[]>();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var accounts = await _confidentialClientApplication.GetAccountsAsync();
                var account = accounts.FirstOrDefault();

                if (account != null)
                {
                    try
                    {
                        var silentResult = await _confidentialClientApplication
                            .AcquireTokenSilent(_scopes, account)
                            .ExecuteAsync();

                        return silentResult.AccessToken;
                    }
                    catch (MsalUiRequiredException)
                    {
                        // Si no se puede obtener en silencio, intentar obtener un nuevo token
                        var clientResult = await _confidentialClientApplication
                            .AcquireTokenForClient(_scopes)
                            .ExecuteAsync();

                        return clientResult.AccessToken;
                    }
                }
                else
                {
                    // Si no hay cuentas, obtener un nuevo token directamente
                    var result = await _confidentialClientApplication
                        .AcquireTokenForClient(_scopes)
                        .ExecuteAsync();

                    return result.AccessToken;
                }
            }
            catch (MsalServiceException ex)
            {
                // Manejo de errores relacionados con el servicio MSAL
                throw new Exception($"Error de autenticación del servicio: {ex.Message}");
            }
            catch (MsalClientException ex)
            {
                // Manejo de errores relacionados con el cliente MSAL
                throw new Exception($"Error del cliente MSAL: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Cualquier otro error
                throw new Exception($"Error al obtener el token: {ex.Message}");
            }
        }
    }

    public class AzureAdOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
    }
}