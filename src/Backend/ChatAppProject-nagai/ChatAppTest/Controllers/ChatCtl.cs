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
        //room�Ŏw�肵�������̃��b�Z�[�W���擾
        [HttpGet]
        public JsonResult Get(int room)
        {
            ReadMessageResult rmr = new ReadMessageResult();
            rmr.status = false;
            rmr.message = "�N�b�L�[�����݂��܂���";
            rmr.result = null;
            //�����~��������sessionId���~�����Ƃ������Ƃ�����
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(rmr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();
            rmr = ChatController.ReadMessage(room, sessionId);
            return new JsonResult(rmr);
        }


        //���b�Z�[�W�𓊍e
        [HttpPost]
        public JsonResult Post(string message, int room)
        {
            WriteMessageResult wmr = new WriteMessageResult();
            wmr.status = false;
            wmr.message = "�N�b�L�[�����݂��܂���";
            wmr.result = null;
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(wmr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();
            wmr = ChatController.WriteMessage(message, sessionId, room);
            return new JsonResult(wmr);
        }

        //���b�Z�[�W���폜����@�������񂾖{�l�����f���[�^�����폜�ł��Ȃ�
        [HttpDelete]
        public JsonResult Delete(int room, int messageId)
        {
            HideMessageResult hmr = new HideMessageResult();
            hmr.status = false;
            hmr.message = "�N�b�L�[�����݂��܂���";
            if (!Request.Cookies.ContainsKey("session_id"))
            {
                return new JsonResult(hmr);
            }
            string sessionId = Request.Cookies["session_id"].ToString();

            hmr = ChatController.HideMessage(sessionId, room, messageId);
            return new JsonResult(hmr);
        }

        //���b�Z�[�W��ҏW����@�������񂾖{�l�����ҏW�ł��Ȃ��@�ҏW�����edited��true�ɂȂ�
        [HttpPatch]
        public JsonResult Patch(int room, int messageId, string message)
        {
            ChangeMessageResult cmr = new ChangeMessageResult();
            cmr.status = false;
            cmr.message = "�N�b�L�[�����݂��܂���";
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