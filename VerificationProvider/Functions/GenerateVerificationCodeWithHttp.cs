using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Models;
using VerificationProvider.Services;

namespace VerificationProvider.Functions
{
	public class GenerateVerificationCodeWithHttp(ILogger<GenerateVerificationCodeWithHttp> logger, IVerificationService verificationService)
	{
		private readonly ILogger<GenerateVerificationCodeWithHttp> _logger = logger;
		private readonly IVerificationService _verificationService = verificationService;

		[Function(nameof(GenerateVerificationCodeWithHttp))]
		[HttpPost]
		public async Task<IActionResult> RunAsync(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
		{
			try
			{
				// Deserialize the request body to extract necessary data
				var requestBody = await req.ReadAsStringAsync();
				var verificationRequest = JsonConvert.DeserializeObject<VerificationRequest>(requestBody!);

				// Call UnpackVerificationRequest method passing the deserialized verification request
				var unpackedRequest = _verificationService.UnpackHTTPVerificationRequest(verificationRequest!);

				// Generate verification code
				var code = _verificationService.GenerateCode();

				// Save verification request (if needed)
				// var result = await _verificationService.SaveVerificationRequest(unpackedRequest, code);

				// Assuming you need to return the generated code in the response
				return new OkObjectResult(code);
			}
			catch (Exception ex)
			{
				_logger.LogError($"ERROR : GenerateVerificationCodeHttp.RunAsync() :: {ex.Message}");
				return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
			}
		}
	}
}
