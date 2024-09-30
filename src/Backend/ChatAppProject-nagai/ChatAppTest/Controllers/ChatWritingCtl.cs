using ChatAppTest.FunctionController.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatWritingCtl : ControllerBase
    {
        [HttpGet]
        public JsonResult Get(int roomId)
        {
            SetTagsResult str = new SetTagsResult();
            str.status = false;
            str.message = "クッキーが存在しません";
            str.result = null;
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(str);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            WritingGetResult wgr = ChatWritingController.GetWriting(sessionId, roomId);
            return new JsonResult(wgr);

        }

        [HttpPost]
        public JsonResult Post(int roomId)
        {
            SetTagsResult str = new SetTagsResult();
            str.status = false;
            str.message = "クッキーが存在しません";
            str.result = null;
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(str);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            WritingStartResult wsr = ChatWritingController.StartWriting(sessionId, roomId);
            return new JsonResult(wsr);

        }

        [HttpDelete]
        public JsonResult Delete(int roomId)
        {
            SetTagsResult str = new SetTagsResult();
            str.status = false;
            str.message = "クッキーが存在しません";
            str.result = null;
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(str);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            WritingStopResult wsr = ChatWritingController.StopWriting(sessionId, roomId);
            return new JsonResult(wsr);
        }
    }
}
