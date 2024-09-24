using ChatAppTest.FunctionController.Chat;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatReadCountCtl : ControllerBase
    {
        [HttpGet]
        public JsonResult Get(int roomId)
        {
            ReadCounterGetResult result = new ReadCounterGetResult();
            result.status = false;
            result.message = "クッキーが存在しません";
            result.result = null;
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(result);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            result = ChatReadCountController.GetCounter(sessionId, roomId);
            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult Post(int roomId, int messageId)
        {
            ReadCounterPostResult result = new ReadCounterPostResult();
            result.status = false;
            result.message = "クッキーが存在しません";
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(result);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            result = ChatReadCountController.PostCounter(sessionId, roomId, messageId);
            return new JsonResult(result);

        }
    }
}
