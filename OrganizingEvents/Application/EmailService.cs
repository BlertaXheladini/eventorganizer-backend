using System.Net;
using System.Net.Mail;

public class EmailService
{
    private readonly string fromEmail = "vjollcebaxhaku@gmail.com";
    private readonly string fromPassword = "ddwa ekht dpcb yati";

    public void SendResetCode(string toEmail, string code)
    {
        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            Credentials = new NetworkCredential(fromEmail, fromPassword)
        };


        var message = new MailMessage(fromEmail, toEmail)
        {
            Subject = "🔒 Your Password Reset Code",
            IsBodyHtml = true,
            Body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2 style='color: #2c3e50;'>Password Reset Request</h2>
                <p>Hello,</p>
                <p>You recently requested to reset your password. Use the code below to reset it:</p>
                <div style='margin: 20px 0; padding: 10px; background: #f1f1f1; border-left: 4px solid #4CAF50; font-size: 18px; font-weight: bold;'>
                    {code}
                </div>
                <p>This code will expire in 15 minutes.</p>
                <p>If you didn’t request this, you can ignore this email.</p>
                <br />
                <p style='color: #888;'>– The Event Organizer Team</p>
            </div>"
        };

        smtp.Send(message);
    }
}
