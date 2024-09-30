using ChatAppTest.FunctionController.Chat;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using static ChatAppTest.Controllers.ChatCtl;

namespace ChatAppTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatRoomCtl : ControllerBase
    {
        //チャット部屋の一覧を取得する　hiddenがtrueの部屋は取得しない
        [HttpGet(Name = "ChatRoom")]
        public JsonResult Get()
        {
            GetRoomsResult grr = new GetRoomsResult();
            grr.status = false;
            grr.message = "クッキーが存在しません";
            grr.result = null;            
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(grr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            grr = ChatController.GetRoomList(sessionId);
            return new JsonResult(grr);
        }

        //チャット部屋を作成する
        [HttpPost(Name = "ChatRoom")]
        public JsonResult Post(string name, bool isPrivate, bool isDm, string[] whitelist)
        {
            AddRoomResult arr = new AddRoomResult();
            arr.status = false;
            arr.message = "クッキーが存在しません";
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(arr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            arr = ChatController.AddChatRooms(name, sessionId, isPrivate, isDm, whitelist.ToList());
            return new JsonResult(arr);
        }

        //チャット部屋を削除する　モデレータと管理者しかできない　実際にはhiddenがtrueになり見えなくなるだけである
        [HttpDelete]
        public JsonResult Delete(int room)
        {
            DeleteRoomResult drr = new DeleteRoomResult();
            drr.status = false;
            drr.message = "クッキーが存在しません";
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(drr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            drr = ChatController.DeleteRoom(sessionId, room);
            return new JsonResult(drr);
        }

        [HttpPatch]
        public JsonResult Patch(int roomId, string roomName, bool isPrivate, string[] whitelist)
        {
            PatchRoomResult prr = new PatchRoomResult();
            prr.status = false;
            prr.message = "クッキーが存在しません";
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(prr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();
            prr = ChatController.PatchRoom(sessionId, roomId, roomName, isPrivate, whitelist.ToList());
            return new JsonResult(prr);
        }

    }
    public class ChatRoomResult
    {
        public bool status { get; set; }
        public string message { get; set; }
        public ChatRoom[] result { get; set; }
    }
}
