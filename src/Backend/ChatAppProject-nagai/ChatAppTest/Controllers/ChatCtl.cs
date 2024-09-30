using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.IO;
using ChatAppTest.FunctionController.Chat;

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
            ReadMessageResult rmr = new ReadMessageResult();
            rmr.status = false;
            rmr.message = "クッキーが存在しません";
            rmr.result = null;
            //何が欲しいってsessionIdが欲しいということだった
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(rmr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();
            rmr = ChatController.ReadMessage(room, sessionId);
            return new JsonResult(rmr);
        }


        //メッセージを投稿
        [HttpPost]
        public JsonResult Post(string message, int room)
        {
            WriteMessageResult wmr = new WriteMessageResult();
            wmr.status = false;
            wmr.message = "クッキーが存在しません";
            wmr.result = null;
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(wmr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();
            wmr = ChatController.WriteMessage(message, sessionId, room);
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