using Demo.Web.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace Demo.Web.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, int OTP)
    {
        MimeMessage emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_configuration["SmtpSettings:SenderName"], _configuration["SmtpSettings:SenderEmail"]));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = "Reset your password";

        string emailTemplate = System.IO.File.ReadAllText("Views/EmailTemplate/ForgotPasswordEmailTemplate.html");
        emailTemplate = emailTemplate.Replace("{{resetOTP}}", OTP.ToString());

        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = emailTemplate
        };

        using (SmtpClient client = new MailKit.Net.Smtp.SmtpClient())
        {
            await client.ConnectAsync(_configuration["SmtpSettings:Server"], int.Parse(_configuration["SmtpSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_configuration["SmtpSettings:Username"], _configuration["SmtpSettings:Password"]);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
    public async Task ContactUs(ContactUsViewModel contactUsViewModel)
    {
        MimeMessage emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(contactUsViewModel.Name, contactUsViewModel.Email));
        emailMessage.To.Add(new MailboxAddress("Developer", "devd.patel@tatvasoft.com"));
        emailMessage.Subject = contactUsViewModel.Subject;

        string emailTemplate = System.IO.File.ReadAllText("Views/EmailTemplate/ContactUsEmailTemplate.html");
        emailTemplate = emailTemplate.Replace("{{content}}", contactUsViewModel.Message);

        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = emailTemplate
        };

        using (SmtpClient client = new MailKit.Net.Smtp.SmtpClient())
        {
            await client.ConnectAsync(_configuration["SmtpSettings:Server"], int.Parse(_configuration["SmtpSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_configuration["SmtpSettings:Username"], _configuration["SmtpSettings:Password"]);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }

    public async Task NewSubscriberMail(string email, NewSubscriberModel newSubscriberModel)
    {
        MimeMessage emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_configuration["SmtpSettings:SenderName"], _configuration["SmtpSettings:SenderEmail"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = "Offer!!";
         string link = $"http://localhost:5214/Offer/Index";
        string emailTemplate = System.IO.File.ReadAllText("Views/EmailTemplate/OfferMailTemplate.html");
        emailTemplate = emailTemplate.Replace("{{Discount}}",
        $"{Math.Round(newSubscriberModel.MinDiscountPercentage)}-{Math.Round(newSubscriberModel.MaxDiscountPercentage)}");
         emailTemplate = emailTemplate.Replace("{{Link}}",link);
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = emailTemplate
        };

        using (SmtpClient client = new MailKit.Net.Smtp.SmtpClient())
        {
            await client.ConnectAsync(_configuration["SmtpSettings:Server"], int.Parse(_configuration["SmtpSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_configuration["SmtpSettings:Username"], _configuration["SmtpSettings:Password"]);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
    
    public async Task OfferMailToAll(List<string>emails, NewSubscriberModel newSubscriberModel)
    {
        MimeMessage emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_configuration["SmtpSettings:SenderName"], _configuration["SmtpSettings:SenderEmail"]));
        foreach (var email in emails)
        {
            emailMessage.Bcc.Add(new MailboxAddress("", email));
        }

        emailMessage.Subject = "Offer!!";
        string link = $"http://localhost:5214/Offer/Index";
        string emailTemplate = System.IO.File.ReadAllText("Views/EmailTemplate/OfferMailTemplate.html");
        emailTemplate = emailTemplate.Replace("{{Discount}}", 
        $"{Math.Round(newSubscriberModel.MinDiscountPercentage)}-{Math.Round(newSubscriberModel.MaxDiscountPercentage)}");
        emailTemplate = emailTemplate.Replace("{{Link}}",link);

        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = emailTemplate
        };

        using (SmtpClient client = new MailKit.Net.Smtp.SmtpClient())
        {
            await client.ConnectAsync(_configuration["SmtpSettings:Server"], int.Parse(_configuration["SmtpSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_configuration["SmtpSettings:Username"], _configuration["SmtpSettings:Password"]);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
