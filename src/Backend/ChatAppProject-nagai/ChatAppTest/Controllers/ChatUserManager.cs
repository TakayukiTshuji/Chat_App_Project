using ChatAppTest.FunctionController.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatUserManager : ControllerBase
    {
        //ユーザのタグを追加する　admin：管理者　moderator：モデレータ
        //adminユーザはadminタグを外すことはできない
        //adminユーザだけがadminタグをほかのユーザに付与できる
        //adminタグを持つユーザだけがmoderatorタグをほかのユーザに付与できる
        //moderatorタグを持つユーザは部屋の削除やメッセージの削除ができる
        [HttpPost]
        public JsonResult Post(string editUser, string[] tags)
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
            str = new SetTagsResult(ChatUserController.SetTags(sessionId, editUser, tags));
            if(str.result != null)
            {
                str.result.password = null;
            }
            return new JsonResult(str);
        }
    }
}
