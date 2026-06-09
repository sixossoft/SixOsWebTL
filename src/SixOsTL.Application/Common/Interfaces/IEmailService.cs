using System.Threading;
using System.Threading.Tasks;

namespace SixOsTL.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
    }
}