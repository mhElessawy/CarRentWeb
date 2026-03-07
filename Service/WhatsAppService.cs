using System.Text;
using System.Text.Json;

namespace CarRentWeb.Service
{
    public class WhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly string _phoneNumberId;
        private readonly string _accessToken;

        public WhatsAppService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _phoneNumberId = configuration["WhatsApp:PhoneNumberId"] ?? "";
            _accessToken = configuration["WhatsApp:AccessToken"] ?? "";
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
            if (string.IsNullOrWhiteSpace(toPhone) || string.IsNullOrWhiteSpace(_phoneNumberId) || string.IsNullOrWhiteSpace(_accessToken))
                return;

            // Normalize phone: remove spaces/dashes, ensure starts with country code
            var phone = toPhone.Trim().Replace(" ", "").Replace("-", "").Replace("+", "");

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

            var payload = new
            {
                messaging_product = "whatsapp",
                to = 96566696126,
                type = "text",
                text = new { body = message }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

            var url = $"https://graph.facebook.com/v18.0/{_phoneNumberId}/messages";
            await _httpClient.PostAsync(url, content);
        }
    }
}