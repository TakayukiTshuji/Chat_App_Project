using ChatAppTest.FunctionController.Session;
using ChatAppTest.FunctionController.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text.Json;

namespace ChatAppTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatSessionCtl : ControllerBase
    {
        //セッションIDからユーザの情報を取得する
        [HttpGet]
        public JsonResult Get()
        {
            AuthSessionResult asr = new AuthSessionResult();
            asr.status = false;
            asr.message = "クッキーが存在しません";
            asr.result = null;
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(asr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();
            asr = new AuthSessionResult(ChatSessionController.AuthSession(sessionId));
            ChatUser? cu = asr.User();
            if(cu != null)
            {
                cu.password = null;
                asr.user = cu;
            }
            return new JsonResult(asr);
        }

        //セッションを作成する　ログインみたいな
        [HttpPost]
        public JsonResult Post(string userId, string password)
        {
            Console.WriteLine("sessionPost");
            IssueSessionResult session = ChatSessionController.IssueSession(userId, password);
            if(session.result != null)
            {
                var op = new CookieOptions();
                op.HttpOnly = true;
                op.Secure = true;
                op.Expires = session.result.expirationDate;
                op.SameSite=SameSiteMode.None;
                Console.WriteLine(op.Expires);
                Response.Cookies.Append("session_id", session.result.sessionId, op);
                Console.WriteLine(session.result.sessionId);
            }
            return new JsonResult(session);
        }

        //セッションを延長する　すでに期限切れのセッションは延長できない
        [HttpPatch]
        public JsonResult Patch()
        {
            RenewSessionResult rsr = new RenewSessionResult();
            rsr.status = false;
            rsr.message = "クッキーが存在しません";
            rsr.result = null;
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(rsr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            rsr = ChatSessionController.RenewSession(sessionId);
            if (rsr.result != null)
            {
                var op = new CookieOptions();
                op.Secure = true;
                op.Expires = rsr.result.expirationDate;
                Response.Cookies.Append("session_id", rsr.result.sessionId, op);
            }

            return new JsonResult(rsr);
        }

        [HttpDelete]
        public JsonResult Delete()
        {
            DeleteSessionResult dsr = new DeleteSessionResult();
            dsr.status = false;
            dsr.message = "クッキーが存在しません";
            dsr.result = null;
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(dsr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            dsr = ChatSessionController.DeleteSession(sessionId);
            var op = new CookieOptions();
            op.Secure = true;
            op.Expires = DateTime.Now.AddDays(-1.0);
            if(dsr.result == null)
            {
                Response.Cookies.Append("session_id", "NO", op);
            }
            else
            {
                Response.Cookies.Append("session_id", dsr.result.sessionId, op);
            }
            return new JsonResult(dsr);
        }
    }
}
