using Demo.Web.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;

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
        emailTemplate = emailTemplate.Replace("{{Link}}", link);
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
    
    public async Task OfferMailToAll(List<string> emails, ProductViewModel discountedProduct)
    {
        MimeMessage emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_configuration["SmtpSettings:SenderName"], 
            _configuration["SmtpSettings:SenderEmail"]));
        
        foreach (var email in emails)
        {
            emailMessage.Bcc.Add(new MailboxAddress("", email));
        }

        decimal discountAmount = discountedProduct.Price * (discountedProduct.Discount / 100m);
        decimal amountAfterDiscount = discountedProduct.Price - discountAmount;

        emailMessage.Subject = "Offer!!";
        
        // Create a multipart/mixed parent container
        Multipart multipart = new Multipart("mixed");
        
        // Create the HTML body part
        BodyBuilder bodyBuilder = new BodyBuilder();
        
        string link = $"http://localhost:5214/CLA/ViewProduct/{discountedProduct.Id}";
        string emailTemplate = System.IO.File.ReadAllText("Views/EmailTemplate/ProductOfferTemplate.html");
        
        // Load the image file
        string imagePath = Path.Combine("wwwroot/images/product-images", discountedProduct.ImageUrl);
        MimeEntity image = await bodyBuilder.LinkedResources.AddAsync(imagePath);
        image.ContentId = MimeUtils.GenerateMessageId();
        
        // Replace placeholders - now using Content ID for the image
        emailTemplate = emailTemplate
            .Replace("{{ProductDiscount}}", $"{Math.Round(discountedProduct.Discount)}")
            .Replace("{{ProductName}}", discountedProduct.Name)
            .Replace("{{ProductAmount}}", discountedProduct.Price.ToString())
            .Replace("{{AmountAfterDiscount}}", amountAfterDiscount.ToString("F2"))
            .Replace("{{Link}}", link)
            .Replace("{{ProductImage}}", $"cid:{image.ContentId}"); // Using cid: scheme for embedded images
        
        bodyBuilder.HtmlBody = emailTemplate;
        multipart.Add(bodyBuilder.ToMessageBody());
        
        emailMessage.Body = multipart;

        using (SmtpClient client = new SmtpClient())
        {
            await client.ConnectAsync(
                _configuration["SmtpSettings:Server"], 
                int.Parse(_configuration["SmtpSettings:Port"]), 
                SecureSocketOptions.StartTls);
            
            await client.AuthenticateAsync(
                _configuration["SmtpSettings:Username"], 
                _configuration["SmtpSettings:Password"]);
            
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
