using DurableFunction_Worker.Models;
using DurableFunction_Worker.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DurableFunction_Worker
{
    public class HttpFunction
    {
        #region Variable・Const
        private readonly MySettings Settings;
        private readonly ILogger Logger;
        private readonly IDiceService DiceService;
        #endregion

        #region [EntryPoint]
        public HttpFunction(IOptions<MySettings> optionsAccessor, ILoggerFactory loggerFactory, IDiceService diceService)
        {
            this.Settings = optionsAccessor.Value;
            this.Logger = loggerFactory.CreateLogger<HttpFunction>();
            this.DiceService = diceService;
        }
        #endregion

        #region [Normal Functions]
        [FunctionName(nameof(HttpFunction))]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var responseMessage = $"HogehogePassword : {this.Settings.HogehogePassword}";

            return new OkObjectResult(responseMessage);
        }

        /// <summary>
        /// Execute trigger
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [FunctionName("ExecuteTrigger")]
        public async Task<IActionResult> RunExecuteTrigger(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            // GET/POST共にBODY部より取得可能
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var target = JsonConvert.DeserializeObject<ReceiveModel>(requestBody);

            var returnModel = new ReturnModel();

            try
            {
                returnModel.ProceedTime = await this.DiceService.RollDiceUntilAsync(target.TargetValue);
                // 返却用モデル生成
                returnModel.IsSucceed = true;
            }
            catch (Exception ex)
            {
                // 返却用モデル生成
                returnModel.IsSucceed = false;
                returnModel.ProceedTime = 0;
                returnModel.Exception = ex.ToString();
            }

            return new OkObjectResult(JsonConvert.SerializeObject(returnModel, Formatting.Indented));
        }
        #endregion

        #region [Durable Functions]
        [FunctionName(nameof(HelloCities))]
        public static async Task<string> HelloCities([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            string result = "";
            result += await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo") + " ";
            result += await context.CallActivityAsync<string>(nameof(SayHello), "London") + " ";
            result += await context.CallActivityAsync<string>(nameof(SayHello), "Seattle");
            return result;
        }

        [FunctionName(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] string cityName, ILogger logger)
        {
            logger.LogInformation("Saying hello to {name}", cityName);
            return $"Hello, {cityName}!";
        }

        [FunctionName(nameof(StartHelloCities))]
        public static async Task<IActionResult> StartHelloCities(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger logger)
        {
            string instanceId = await starter.StartNewAsync(nameof(HelloCities));
            logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }


        [FunctionName(nameof(ExecuteOrchestrator))]
        public async Task<ReturnModel> ExecuteOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var receivePram = context.GetInput<ReceiveModel>();
            var output = new ReturnModel();
            output = await context.CallActivityAsync<ReturnModel>(nameof(ExecuteActivity), receivePram);

            return output;
        }

        [FunctionName(nameof(ExecuteActivity))]
        public async Task<ReturnModel> ExecuteActivity([ActivityTrigger] ReceiveModel receivePram, ILogger logger)
        {
            var returnModel = new ReturnModel();

            try
            {
                returnModel.ProceedTime = await this.DiceService.RollDiceUntilAsync(receivePram.TargetValue);
                // 返却用モデル生成
                returnModel.IsSucceed = true;
            }
            catch (Exception ex)
            {
                // 返却用モデル生成
                returnModel.IsSucceed = false;
                returnModel.ProceedTime = 0;
                returnModel.Exception = ex.ToString();
            }

            return returnModel;
        }

        [FunctionName(nameof(DurableExecuteTrigger))]
        public async Task<IActionResult> DurableExecuteTrigger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger logger)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var target = JsonConvert.DeserializeObject<ReceiveModel>(requestBody);

            string instanceId = await starter.StartNewAsync(nameof(ExecuteOrchestrator), target);
            logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
        #endregion
    }
}
