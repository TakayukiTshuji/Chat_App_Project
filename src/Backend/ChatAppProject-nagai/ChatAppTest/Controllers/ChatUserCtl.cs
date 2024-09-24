using ChatAppTest.FunctionController.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatUserCtl : ControllerBase
    {
        //ユーザIDとパスワードからユーザ情報を取得する　ログイン認証だけみたいな感じ　多分いらんな
        [HttpGet]
        public JsonResult Get(string userId, string password)
        {
            AuthUserResult? cu = new(ChatUserController.AuthUser(userId, password));
            if(cu.result != null)
            {
                cu.result.password = null;
            }
            return new JsonResult(cu);
        }

        //ユーザを作成する　ユーザIDは4文字以上で英数字と「-」、「_」が使える　パスワードは8文字以上
        [HttpPost]
        public JsonResult Post(string userId, string familyName, string firstName, string language, string studentId, string password, string nickname_ja, string nickname_en)
        {
            CreateUserResult cur = new CreateUserResult(ChatUserController.CreateUser(userId, familyName, firstName, language, studentId, password, nickname_ja, nickname_en));
            if(cur.result != null)
            {
                cur.result.password = null;
            }
            return new JsonResult(cur);
        }

        //ユーザを削除する　本人が行う
        [HttpDelete]
        public JsonResult Delete(string userId, string password)
        {
            DeleteUserResult dur = new DeleteUserResult(ChatUserController.DeleteUser(userId, password));
            if(dur.result != null)
            {
                dur.result.password = null;
            }
            return new JsonResult(dur);
        }

        //ユーザ情報を変更する　本人が行う
        [HttpPatch]
        public JsonResult Patch(string userId, string password, string familyName = "", string firstName = "", string language = "", string newPassword = "", string nickname_ja = "", string nickname_en = "")
        {
            ChangeUserResult cur = new ChangeUserResult(ChatUserController.ChangeUser(userId, password, familyName, firstName, language, newPassword, nickname_ja, nickname_en));
            if(cur.result != null)
            {
                cur.result.password = null;
            }
            return new JsonResult(cur);
        }
    }
}
