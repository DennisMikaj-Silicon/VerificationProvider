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
    public class GenerateVerificationCodeWithHttp
    {
        private readonly ILogger<GenerateVerificationCodeWithHttp> _logger;
        private readonly IVerificationService _verificationService;

        public GenerateVerificationCodeWithHttp(ILogger<GenerateVerificationCodeWithHttp> logger, IVerificationService verificationService)
        {
            _logger = logger;
            _verificationService = verificationService;
        }

        [Function(nameof(GenerateVerificationCodeWithHttp))]
        public async Task<HttpResponseData> RunAsync(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
    FunctionContext context)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);

            try
            {
                var requestBody = await req.ReadAsStringAsync();

                var email = JsonConvert.DeserializeObject<string>(requestBody!);

                var verificationRequest = _verificationService.UnpackHTTPVerificationRequest(email);

                if (verificationRequest == null)
                {
                    _logger.LogError("Failed to unpack HTTP verification request.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                var code = _verificationService.GenerateCode();

                var emailRequest = _verificationService.GenerateEmailRequest(verificationRequest, code);

                var emailRequestJson = JsonConvert.SerializeObject(emailRequest);

                await response.WriteStringAsync(emailRequestJson);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : GenerateVerificationCodeHttp.RunAsync() :: {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }
        }
    }
}
