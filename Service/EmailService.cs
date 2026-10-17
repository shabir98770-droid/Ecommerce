namespace Ecommerce.Services
{
    public interface IEmailService
    {
        Task SendOrderEmail(string email, int orderId);
    }
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        public EmailService(ILogger<EmailService> logger) => _logger = logger;
        public async Task SendOrderEmail(string email, int orderId)
        {
            _logger.LogInformation("CROSS CUTTING - EMAIL SENT to {Email} for Order {Id}", email, orderId);
            await Task.Delay(200);
        }
    }
}