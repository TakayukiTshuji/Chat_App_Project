using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.IO;
using ChatAppTest.FunctionController.Chat;
using Microsoft.Net.Http.Headers;

namespace ChatAppTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatCtl : ControllerBase
    {
        
        //roomで指定した部屋のメッセージを取得
        [HttpGet]
        public JsonResult Get(int room)
        {
            Console.WriteLine("Get");
            ReadMessageResult rmr = new ReadMessageResult();
            rmr.status = false;
            rmr.message = "クッキーが存在しません";
            rmr.result = null;
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(rmr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();
            rmr = ChatController.ReadMessage(room, sessionId);
            return new JsonResult(rmr);
        }
        [HttpOptions]
        public JsonResult Options()
        {
            Console.WriteLine("option");
            Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Cookie, Accept");
            Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:3000");
            return new JsonResult(new object());
        }
        //メッセージを投稿
        [HttpPost]
        public JsonResult Post(string message, int room)//frombodyを追加?
        {

            Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Cookie, Accept");
            Response.Headers.Append("Access-Control-Allow-Origin", "*");
            
            Console.WriteLine("post");
            Console.WriteLine(message);

            WriteMessageResult wmr = new WriteMessageResult();

            //if (!Request.Cookies.ContainsKey("session_id"))
            //{
            //    Console.WriteLine("クッキーにsession_idが含まれていません。");
            //    wmr.message = "クッキーが存在しません";
            //    return new JsonResult(wmr);
            //}
            //wmr.status = false;
            //wmr.message = "クッキーが存在しません";
            //wmr.result = null;

            //string sessionId = "1234567890"; // テスト用
            //string sessionId = Request.Cookies["session_id"].ToString();
            string sessionId = Request.Cookies["session_id"];//修正
            //Console.WriteLine(sessionId);
            if (sessionId == null)
            {
                Console.WriteLine("クッキーにsession_idが含まれていません。");
                wmr.message = "クッキーが存在しません";
                return new JsonResult(wmr);
            }
            //wmr = ChatController.WriteMessage(message, sessionId, room);
            wmr = ChatController.WriteMessage(message,sessionId,0);

            //Response.Cookies.Append("Access-Control-Allow-Origin", "*");
            return new JsonResult(wmr);
        }

        //メッセージを削除する　書き込んだ本人かモデレータしか削除できない
        [HttpDelete]
        public JsonResult Delete(int room, int messageId)
        {
            HideMessageResult hmr = new HideMessageResult();
            hmr.status = false;
            hmr.message = "クッキーが存在しません";
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(hmr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            hmr = ChatController.HideMessage(sessionId, room, messageId);
            return new JsonResult(hmr);
        }

        //メッセージを編集する　書き込んだ本人しか編集できない　編集するとeditedがtrueになる
        [HttpPatch]
        public JsonResult Patch(int room, int messageId, string message)
        {
            ChangeMessageResult cmr = new ChangeMessageResult();
            cmr.status = false;
            cmr.message = "クッキーが存在しません";
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(cmr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            cmr = ChatController.ChangeMessage(sessionId, room, messageId, message);
            return new JsonResult(cmr);
        }

        public class GetMessage
        {
            public bool status { get; set; }
            public string message { get; set; }
            public MessageTemplate[] result { get; set; }
        }

        public class PostResponse
        {
            public string message { get; set; }
        }
    }
}