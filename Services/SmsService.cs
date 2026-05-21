using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Skolaris.Services
{
    public class SmsService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IConfiguration config, ILogger<SmsService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // Envoi d'un SMS à un numéro en format international (+33612345678)
        public async Task<(bool Success, string Error)> SendSmsAsync(string toPhone, string message)
        {
            var accountSid = _config["TwilioSettings:AccountSid"];
            var authToken = _config["TwilioSettings:AuthToken"];
            var fromPhone = _config["TwilioSettings:FromPhone"];

            if (string.IsNullOrWhiteSpace(accountSid) || accountSid == "VOTRE_ACCOUNT_SID")
            {
                _logger.LogWarning("Twilio non configuré. SMS vers {Phone} non envoyé.", toPhone);
                return (false, "Twilio non configuré dans appsettings.json.");
            }

            if (string.IsNullOrWhiteSpace(toPhone))
                return (false, "Numéro de téléphone vide.");

            try
            {
                TwilioClient.Init(accountSid, authToken);

                var sms = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(fromPhone),
                    to: new PhoneNumber(toPhone)
                );

                _logger.LogInformation("SMS envoyé à {Phone} — SID: {Sid}", toPhone, sms.Sid);
                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur SMS vers {Phone}", toPhone);
                return (false, ex.Message);
            }
        }

        // Envoi groupé — retourne un résultat par numéro
        public async Task<List<SmsResultItem>> SendBulkSmsAsync(List<string> phones, string message)
        {
            var results = new List<SmsResultItem>();

            foreach (var phone in phones)
            {
                var tel = phone.Trim();
                if (string.IsNullOrWhiteSpace(tel)) continue;

                var (success, error) = await SendSmsAsync(tel, message);
                results.Add(new SmsResultItem { Telephone = tel, Success = success, Error = error });
            }

            return results;
        }
    }

    public class SmsResultItem
    {
        public string Telephone { get; set; } = "";
        public bool Success { get; set; }
        public string Error { get; set; } = "";
    }
}
