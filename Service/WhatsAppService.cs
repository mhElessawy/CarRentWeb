using System.Text;
using System.Text.Json;

namespace CarRentWeb.Service
{
    public class WhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly string _idInstance;
        private readonly string _apiToken;
        private readonly string _apiUrl;

        public WhatsAppService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _idInstance = configuration["GreenApi:IdInstance"] ?? "";
            _apiToken   = configuration["GreenApi:ApiToken"]   ?? "";
            _apiUrl     = configuration["GreenApi:ApiUrl"]     ?? "https://api.green-api.com";
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
            if (string.IsNullOrWhiteSpace(toPhone) || string.IsNullOrWhiteSpace(_idInstance))
                return;

            // Normalize phone: remove spaces/dashes/+, ensure starts with Kuwait country code
            var phone = toPhone.Trim().Replace(" ", "").Replace("-", "").Replace("+", "");
            if (phone.StartsWith("0"))
                phone = "965" + phone.Substring(1);
            else if (!phone.StartsWith("965"))
                phone = "965" + phone;

            // Green API chatId format: {phone}@c.us
            var chatId = $"{phone}@c.us";

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

            var url     = $"{_apiUrl}/waInstance{_idInstance}/sendMessage/{_apiToken}";
            var payload = new { chatId = chatId, message = message };
            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"[GreenAPI DEBUG] chatId: {chatId}");
            Console.WriteLine($"[GreenAPI DEBUG] IdInstance: {_idInstance}");
            Console.WriteLine($"[GreenAPI DEBUG] Sending to: {url}");

            var response     = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[GreenAPI ERROR] Status: {response.StatusCode}");
                Console.WriteLine($"[GreenAPI ERROR] Response: {responseBody}");
            }
            else
            {
                Console.WriteLine($"[GreenAPI OK] Message sent to {chatId}");
                Console.WriteLine($"[GreenAPI OK] Response: {responseBody}");
            }
        }
    }
}
