﻿using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Functions;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class VerificationService(ILogger<VerificationService> logger, IServiceProvider serviceProvider) : IVerificationService
{
	private readonly ILogger<VerificationService> _logger = logger;
	private readonly IServiceProvider _serviceProvider = serviceProvider;

	public VerificationRequest UnpackVerificationRequest(ServiceBusReceivedMessage message)
	{
		try
		{
			var verificationRequest = JsonConvert.DeserializeObject<VerificationRequest>(message.Body.ToString());
			if (verificationRequest != null && !string.IsNullOrEmpty(verificationRequest.Email))
				return verificationRequest;
		}
		catch (Exception ex)
		{
			_logger.LogError($"ERROR : GenerateVerificationCode.UnpackVerificationRequest() :: {ex.Message}");
		}
		return null!;
	}

	public VerificationRequest UnpackHTTPVerificationRequest(VerificationRequest verificationRequest)
	{
		try
		{
			var req = JsonConvert.SerializeObject(verificationRequest);
            if (req != null && !string.IsNullOrEmpty(req))
                return verificationRequest;
        }
		catch (Exception ex)
		{
            _logger.LogError($"ERROR : GenerateVerificationCode.UnpackHTTPVerificationRequest() :: {ex.Message}");
        }
        return null!;

    }

    public string GenerateCodeFromHttp(VerificationRequest verificationRequest)
	{
		try
		{
			var payload = JsonConvert.SerializeObject(verificationRequest);
			if (!string.IsNullOrEmpty(payload))
			{
				return payload;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"ERROR : GenerateVerificationCode.GenerateServiceBusEmailRequest() :: {ex.Message}");
		}
		return null!;
	}


	public string GenerateCode()
	{
		try
		{
			var rnd = new Random();
			var code = rnd.Next(100000, 999999);

			return code.ToString();
		}
		catch (Exception ex)
		{
			_logger.LogError($"ERROR : GenerateVerificationCode.UnpackVerificationRequest() :: {ex.Message}");
		}
		return null!;
	}

	public async Task<bool> SaveVerificationRequest(VerificationRequest verificationRequest, string code)
	{
		try
		{
			using var context = _serviceProvider.GetRequiredService<DataContext>();

			var existingRequest = await context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == verificationRequest.Email);
			if (existingRequest != null)
			{
				existingRequest.Code = code;
				existingRequest.ExpiryDate = DateTime.Now.AddMinutes(5);
				context.Entry(existingRequest).State = EntityState.Modified;
			}
			else
			{
				context.VerificationRequests.Add(new Data.Entities.VerificationRequestEntity() { Email = verificationRequest.Email, Code = code });
			}

			await context.SaveChangesAsync();
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError($"ERROR : SaveVerificationRequest.Run() :: {ex.Message}");
		}
		return false;
	}

	public EmailRequest GenerateEmailRequest(VerificationRequest verificationRequest, string code)
	{
		try
		{
			if (!string.IsNullOrEmpty(verificationRequest.Email) && !string.IsNullOrEmpty(code))
			{
				var emailRequest = new EmailRequest()
				{
					To = verificationRequest.Email,
					Subject = $"Verification Code {code}",
					HtmlBody = $@"
                        <html lang='en'>
                           <head>
                                <meta charset='UTF-8'>
                                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                <title>Verification Code</title>
                           </head>
                           <body>      
                                <div style='color: #191919; max-width: 500px;'>
                                <div style='background-color: #4F85F6; color: white; text-align: center; padding: 20px 0;'>
                                     <h1 style='font-weight: 400;'>Verification Code</h1>
                                </div>
                                <div style='background-color: #f4f4f4; padding: 1rem 2rem;'>
                                <p>Dear user,</p>
                                <p>We recieved a request to sign in to your account using e-mail {verificationRequest.Email}. Please verify your account using this verification code:
                                   <div style='font-size:22px; color: #191919;'> {code} </div>
                                </p>
                                <div style='color: #191919; font-size: 11px;'>
                                <p>If you did not request this code, it is possible that someone else is trying to access the Silicon Account <span style='color: #0041cd;'>{verificationRequest.Email}.</span> This Email can't recieve replies. For more information, visit the Silicons Help Center.</p> 
                                </div>
                               </div>
                               <div style='color: #191919; text-align: center; font-size: 11px;>
                                <p>© Silicon, Sveavägen 1, SE-123 45 Stockholm, Sweden</p>
								</div>	
                                </div>
                           </body>
                        </html>
                      ",
					PlainText = $"Please verify your account using this verification code: {code}. If you did not request this code, it is possible that someone else is trying to access the Silicon Account {verificationRequest.Email}. This Email can't recieve replies. For more information, visit the Silicons Help Center."

				};
				return emailRequest;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"ERROR : GenerateEmailRequest.UnpackVerificationRequest() :: {ex.Message}");

		}
		return null!;
	}



	public string GenerateServiceBusEmailRequest(EmailRequest emailRequest)
	{
		try
		{
			var payload = JsonConvert.SerializeObject(emailRequest);
			if (!string.IsNullOrEmpty(payload))
			{
				return payload;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"ERROR : GenerateVerificationCode.GenerateServiceBusEmailRequest() :: {ex.Message}");
		}
		return null!;
	}
}


