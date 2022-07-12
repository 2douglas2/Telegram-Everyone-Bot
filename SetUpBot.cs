using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System;
using Microsoft.Data.SqlClient;
using System.Text;

namespace TelegramFunctions
{
    public class SetUpBot
    {

        private readonly TelegramBotClient _botClient;
        private string connectionString = Environment.GetEnvironmentVariable("sqldb_connection");

        public SetUpBot()
        {
            _botClient = new TelegramBotClient(System.Environment.GetEnvironmentVariable("TelegramBotToken", EnvironmentVariableTarget.Process));


        }
        private const string SetUpFunctionName = "setup";
        private const string UpdateFunctionName = "handleupdate";

        [Function(SetUpFunctionName)]
        public async Task RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            var handleUpdateFunctionUrl = req.Url.ToString().Replace(SetUpFunctionName, UpdateFunctionName,
                                                ignoreCase: true, culture: CultureInfo.InvariantCulture);


            await _botClient.SetWebhookAsync(handleUpdateFunctionUrl);

        }

        [Function(UpdateFunctionName)]
        public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            var request = await req.ReadAsStringAsync();
            var update = JsonConvert.DeserializeObject<Telegram.Bot.Types.Update>(request);

            if (update.Type != UpdateType.Message)
                return;
            if (update.Message!.Type != MessageType.Text)
                return;

            await _botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: GetBotResponseForInput(update.Message.Text, update.Message.Chat.Id, update.Message.From.Username));
        }

        private string GetBotResponseForInput(string text, long ChatID, string Username)
        {
            try
            {
                /*if (text.Contains("pod bay doors", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "I'm sorry Dave, I'm afraid I can't do that";
                }*/
                string[] t = text.Split(" ", 2);
                string command = t[0].Split("@", 2)[0];
                string answer;
                switch (command)
                {
                    case "/start":
                        answer = "Bem vindo ao EveryoneBotGroup";
                        break;
                    case "/everyone":
                        answer = "@douglas_sakuta @marioces @viniciussec";
                        break;
                    case "/addMember":
                        answer = Username + " foi adicionado ao grupo " + t[1];
                        /*using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            Microsoft.Data.SqlClient.SqlParameter parameter;
                            using (Microsoft.Data.SqlClient.SqlCommand commandSql = new Microsoft.Data.SqlClient.SqlCommand())
                            {
                                commandSql.Connection = connection;
                                commandSql.CommandType = System.Data.CommandType.Text;
                                commandSql.CommandText = @"  
INSERT INTO SalesLT.Product  
        (Name,  
        ChatID,  
        GroupName 
        )  
    OUTPUT  
        INSERTED.ProductID  
    VALUES  
        (@Name,  
        @ChatID,  
        @GroupName
        ); ";

                                parameter = new Microsoft.Data.SqlClient.SqlParameter("@Name", System.Data.SqlDbType.NVarChar, 50);
                                parameter.Value = Username;
                                commandSql.Parameters.Add(parameter);

                                parameter = new Microsoft.Data.SqlClient.SqlParameter("@ChatID", System.Data.SqlDbType.Int);
                                parameter.Value = ChatID;
                                commandSql.Parameters.Add(parameter);

                                parameter = new Microsoft.Data.SqlClient.SqlParameter("@GroupName", System.Data.SqlDbType.NVarChar,50);
                                parameter.Value = t[1];
                                commandSql.Parameters.Add(parameter);

                                int productId = (int)commandSql.ExecuteScalar();
                            }
                        }*/
                        break;
                    case "/removeMember":
                        answer = Username + " foi removido do grupo " + t[1];
                        break;
                    default:
                        answer = "Não entendi seu pedido";
                        break;
                }
                //return new DataTable().Compute(t[1], null).ToString();
                return answer;
            }
            catch 
            {
                return $"Error interno no Bot";
            }
        }

    }

}


