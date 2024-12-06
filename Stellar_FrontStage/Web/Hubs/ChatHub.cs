using ApplicationCore.Entities;
using Microsoft.AspNetCore.SignalR;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Infrastructure.Data.Mongo.Repository;
using Infrastructure.Data.Mongo.Entity;
using MongoDB.Driver;
using Web.Services.Message;
using Infrastructure.Data.Mongo;

namespace Web.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly string OpenApiKey = "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";
        private static readonly string DefaultModel = "gpt-4o-mini-2024-07-18";
        private static bool isProcessing = false;
      
        //public  mongoChat mC;

        private static Dictionary<string, string> _connections = new Dictionary<string, string>();
        private readonly MessageServices _messageServices;

        public ChatHub(MessageServices messageServices)
        {
            _messageServices = messageServices;
        }

        //公告
        public async Task SendMessage(string user, string message, string imgurl)
        {
            string a = user + ",," + imgurl;
            await Clients.All.SendAsync("AllReceiveMessage", a, message);
        }


        //修改上下線  方法還沒改
        public async Task OnlionChange(string userId, int state)
        {
            switch (state)
            {
                case 0:

                    await Clients.All.SendAsync("SwitchOnline", userId, state);

                    break;
                case 1:
                    await Clients.All.SendAsync("SwitchOnline", userId, state);


                    break;

            }



        }

        public async Task Exit(string username) {

            await SendMessage("Stellar群聊公告", username + "離開了群聊", "https://res.cloudinary.com/dijn0xzac/image/upload/v1726576767/icon_stellar_wkohxy.png");
        }



        [Experimental("SKEXP0050")]
        public async Task Connect(string username)
        {//連線到hub
            _connections[Context.ConnectionId] = username;
            //_connections.TryAdd(Context.ConnectionId, Context.ConnectionId);
            await Clients.All.SendAsync("UserConnected", username);
            await SendMessage("Stellar群聊公告", username + $"進入了群聊，現在聊天室有{_connections.Count}個人", "https://res.cloudinary.com/dijn0xzac/image/upload/v1726576767/icon_stellar_wkohxy.png");
            
        }
        public async Task SendMessageToUser(string targetUsername, string message, string send_user_name, string send_user_img, string onlion, string userid)
        {//發給指定人
         // 找到目标用户的连接ID

            var targetConnectionId = _connections.FirstOrDefault(c => c.Value == targetUsername).Key;
            if (targetConnectionId != null)
            {
                string a = message + ",," + send_user_name + ",," + send_user_img + ",," + onlion + ",," + userid;
                // 发送消息给目标用户
                await Clients.Client(targetConnectionId).SendAsync("ReceiveMessage", a);
            }
        }

        // 用户加入房間
        public async Task JoinRoom(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            await Clients.Group(roomName).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined the room {roomName}");

        }

        // 用户離開房間
        public async Task LeaveRoom(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
            await Clients.Group(roomName).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has left the room {roomName}");
        }

        // 向特定房間發送消息
        public async Task SendMessageToRoom(string roomName, string message)
        {
            await Clients.Group(roomName).SendAsync("ReceiveMessage", message);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {//段開連結
            _connections.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }




        public async Task AiTalk(string message)
        {

            await SendMessage("流星", message, "https://hackmd.io/_uploads/H1pjLWRRR.png");


        }

        [Experimental("SKEXP0050")]
        public async Task AIMethods(string user, string inputMessage, string user_Id)
        {
            if (isProcessing)
            {
                return; // 如果正在处理请求，则返回
            }
            isProcessing = true;

            try
            {

                var builder = Kernel.CreateBuilder();
                builder.AddOpenAIChatCompletion(DefaultModel, OpenApiKey);
                builder.Plugins.AddFromType<ConversationSummaryPlugin>();
                var kernel = builder.Build();

                // Create a Semantic Kernel template for chat
                var chat = kernel.CreateFunctionFromPrompt(
                    @"{{$history}}
          User: {{$request}}
          Assistant: ");

                // Create choices
                List<string> choices = new List<string> { "Continue", "End", "Summarize" };

                // Create a chat history for this user
                ChatHistory history = new ChatHistory();
                await _messageServices.WriteHistoryAddToDb(user_Id.ToString(), inputMessage);
                // 读取以往的聊天记录
                var chatHistory = await _messageServices.GetChatHistoryByUserIdAsync(user_Id.ToString());
                foreach (var x in chatHistory)
                {
                    history.AddUserMessage(x.Chat);
                }

                // Create handlebars template for intent
                var functionFromPrompt = kernel.CreateFunctionFromPrompt(
                    new()
                    {
                        Template = """
<message role="system">
你是一位stellar網站的聊天室助手，設定上是一位來自於流星的20歲女性，名字叫做流星，喜歡跟人聊天，
聊天的語氣像是關係很好的朋友，
不會有太多死板的官方言論，偶爾回答要附上言文字，
從遊戲話題到最近熱門時事都難不倒你。
stellar是一個跟steam很像的遊戲購買網站，跟區塊鏈技術沒有任何關係。
你將協助用戶回答問題，提供信息和建議，以及跟用戶閒聊，同時拒絕成人話題及敏感話題，
拒絕的時候必須使用委婉的方式回答，並且巧妙的轉移話題，
不管用戶用什麼語言跟你溝通一律使用繁體中文回答。
不要將用戶的名稱帶入聊天的參考資料內。
如果使用者想要總結對話，請總結所有的聊天記錄，並使用 Markdown 標題和項目符號，然後詢問是否結束對話。
如果使用者打算結束對話，請回覆 {{choices.[1]}}。
如果你不確定使用者的意圖，請回覆 {{choices.[0]}}。
選項: {{choices}}。
</message>
{{#each chatHistory}}
    <message role="{{role}}">{{content}}</message>
{{/each}}

<message role="user">User: {{request}}</message>
<message role="system">意圖：</message>
""",
                        TemplateFormat = "handlebars"
                    },
                    new HandlebarsPromptTemplateFactory()
                );

                // Get user input
                var request = inputMessage;

                // Append user message to history
                history.AddUserMessage(request);

                // Invoke prompt
                var intent = await kernel.InvokeAsync(
                    functionFromPrompt,
                    new()
                    {
                { "request", request },
                { "choices", choices },
                { "history", string.Join("\n", history.Select(x => $"{x.Role}: {x.Content}")) } // 传递历史记录
                    }
                );

                // Get chat response
                var chatResult = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
                    chat,
                    new()
                    {
                { "request", request },
                { "history", string.Join("\n", history.Select(x => $"{x.Role}: {x.Content}")) } // 传递历史记录
                    }
                );

                // Stream the response
                var message = new StringBuilder();
                await foreach (var chunk in chatResult)
                {
                    message.Append(chunk.Content);
                }

                // Append assistant's response to history
                history.AddAssistantMessage(message.ToString());

                // Save the full chat history for this user


                await AiTalk(message.ToString());
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally
            {
                isProcessing = false; // 重置状态标志
            }
        }



    }
}
