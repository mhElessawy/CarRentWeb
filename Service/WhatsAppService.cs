using System.Net.Http.Headers;
using System.Text;

namespace CarRentWeb.Service
{
    public class WhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;

        // Keep Meta fields so existing DI registration still compiles
        private readonly string _phoneNumberId;
        private readonly string _accessToken;

        public WhatsAppService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _phoneNumberId = configuration["WhatsApp:PhoneNumberId"] ?? "";
            _accessToken  = configuration["WhatsApp:AccessToken"]  ?? "";

            _accountSid  = configuration["Twilio:AccountSid"]  ?? "";
            _authToken   = configuration["Twilio:AuthToken"]    ?? "";
            _fromNumber  = configuration["Twilio:FromNumber"]   ?? "";
        }

        public async Task SendInvoiceMessageAsync(
            string toPhone,
            string customerName,
            int? billNo,
            decimal? amount,
            DateOnly? fromDate,
            DateOnly? toDate,
            int? noOfDays,
            DateOnly? billDate,
            string contractType)
        {
            if (string.IsNullOrWhiteSpace(toPhone) || string.IsNullOrWhiteSpace(_accountSid))
                return;

            // Normalize phone: remove spaces/dashes, ensure starts with country code
            var phone = toPhone.Trim().Replace(" ", "").Replace("-", "").Replace("+", "");
            if (phone.StartsWith("0"))
                phone = "965" + phone.Substring(1);
            else if (!phone.StartsWith("965"))
                phone = "965" + phone;

            string message =
                $"🧾 *فاتورة إيجار سيارة*\n" +
                $"━━━━━━━━━━━━━━━━\n" +
                $"👤 العميل: {customerName}\n" +
                $"🔢 رقم الفاتورة: {billNo}\n" +
                $"📅 تاريخ الفاتورة: {billDate?.ToString("yyyy/MM/dd")}\n" +
                $"📆 من: {fromDate?.ToString("yyyy/MM/dd")} إلى: {toDate?.ToString("yyyy/MM/dd")}\n" +
                (noOfDays.HasValue ? $"🗓️ عدد الأيام: {noOfDays}\n" : "") +
                $"💰 المبلغ المدفوع: {amount:N3} د.ك\n" +
                $"🚗 نوع العقد: {contractType}\n" +
                $"━━━━━━━━━━━━━━━━\n" +
                $"شكراً لتعاملكم معنا 🙏";

            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json";

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("From", $"whatsapp:{_fromNumber}"),
                new KeyValuePair<string, string>("To",   $"whatsapp:+{phone}"),
                new KeyValuePair<string, string>("Body", message)
            });

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = formData
            };

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_accountSid}:{_authToken}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            Console.WriteLine($"[Twilio DEBUG] AccountSid: {_accountSid}");
            Console.WriteLine($"[Twilio DEBUG] AuthToken length: {_authToken?.Length}");
            Console.WriteLine($"[Twilio DEBUG] URL: {url}");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[Twilio ERROR] Status: {response.StatusCode}");
                Console.WriteLine($"[Twilio ERROR] Response: {responseBody}");
            }
            else
            {
                Console.WriteLine($"[Twilio OK] Message sent to whatsapp:+{phone}");
            }
        }
    }
}
