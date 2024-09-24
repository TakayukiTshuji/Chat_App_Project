using ChatAppTest.FunctionController.Chat;
using ChatAppTest.FunctionController.User;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Timers;

namespace ChatAppTest.FunctionController.Session
{
    //mongodbを使用してセッションを管理
    public static class ChatSessionController
    {
        //とりあえずセッションの有効期限は発行してから7日間
        public const double SESSION_EXPIRATION_DATE = 10080.0;
        public const string SESSION_COLLECTIN_NAME = "chatAppSession";
        public const string SESSION_DB_NAME = "test";

        public static Dictionary<string, ChatSession> sessionDic = new Dictionary<string, ChatSession>();
        public static MongoClient client = new MongoClient("mongodb://localhost");
        public static IMongoDatabase db = client.GetDatabase(SESSION_DB_NAME);
        public static IMongoCollection<ChatSession> collection;
        public static List<ChatSession> sessionAddList = new List<ChatSession>();
        public static List<string> sessionDeleteList = new List<string>();
        public static List<string> sessionRenewList = new List<string>();
        public static bool isWriting = false;
        private static object isWritingLockObject = new object();
        private static System.Timers.Timer timer = new System.Timers.Timer();
        private static bool isSessionChanged = true;
        private static object isSessionChangedLockObj = new object();

        public static void Initialize()
        {
            isWriting = true;
            try
            {
                db.CreateCollection(SESSION_COLLECTIN_NAME);
            }
            catch
            {

            }
            collection = db.GetCollection<ChatSession>(SESSION_COLLECTIN_NAME);
            timer.Interval = 100;
            timer.Elapsed += StartWriting;
            timer.AutoReset = true;
            timer.Enabled = true;
            isWriting = false;
        }
        //session idでセッションを取得　無効はnull
        private static ChatSession? GetSession(string sessionId)
        {
            ChatSession? ret = null;
            lock (sessionDic)
            {
                if (sessionDic.ContainsKey(sessionId))
                {
                    ret = sessionDic[sessionId];
                }
            }

            return ret;
        }

        public static bool CheckUser(string sessionId, string userId)
        {
            ChatSession? cs = GetSession(sessionId);
            if (cs == null) return false;
            if (cs.userId == userId) return true;
            return false;
        }

        //ユーザ名からセッションを作成
        public static IssueSessionResult IssueSession(string userId, string password)
        {
            IssueSessionResult isr = new IssueSessionResult();
            isr.message = "謎失敗　管理者に問い合わせてください。";
            isr.status = false;
            isr.result = null;
            //エラーの場合nullにする
            //存在するユーザか認証する処理を書いといてね未来の自分

            //認証ここ
            AuthUserResult aur = ChatUserController.AuthUser(userId, password);
            if (!aur.status)
            {
                isr.message = aur.message;
                return isr;
            }

            bool isSkip = false;
            var cs = new ChatSession();
            cs.sessionId = Guid.NewGuid().ToString();
            cs.userId = userId;
            cs.expirationDate = new DateTime(DateTime.Now.Ticks).AddMinutes(SESSION_EXPIRATION_DATE);
            lock (sessionAddList)
            {
                //foreach(ChatSession session in sessionAddList)
                //{
                //    if(session.userId == userId)
                //    {
                //        isSkip = true;
                //        break;
                //    }
                //}
                //if(isSkip) { return 1; }
                sessionAddList.Add(cs);
            }
            isr.status = true;
            isr.result = cs;
            isr.message = "成功";
            //collection.DeleteMany(Builders<ChatSession>.Filter.Eq("s", "aa"));
            return isr;
        }

        //sessionは7日たつと無効になる　7日以内にrenewするとそのsessionIdの期限を延長できる
        public static RenewSessionResult RenewSession(string sessionId)
        {
            RenewSessionResult rsr = new RenewSessionResult();
            rsr.status = false;
            rsr.message = "謎失敗　管理者に問い合わせてください。";
            rsr.result = null;
            ChatSession? cs = GetSession(sessionId);

            if(cs == null)
            {
                rsr.message = "存在しないセッションID";
                return rsr;
            }

            DateTime n = DateTime.Now;
            if(cs.expirationDate < n)
            {
                rsr.message = "存在しないセッションID";
                return rsr;
            }
            lock (sessionRenewList)
            {
                sessionRenewList.Add(sessionId);
            }
            rsr.status = true;
            rsr.result = cs;
            rsr.message = "成功";
            return rsr;
        }

        //入力されたセッションIDが有効かどうか検証する
        //有効であればChatSession、無効であればnullが返ってくる
        public static AuthSessionResult AuthSession(string sessionId)
        {
            AuthSessionResult result = new AuthSessionResult();
            result.status = false;
            result.message = "謎失敗　管理者に問い合わせてください。";
            result.result = null;

            ChatSession? ret = null;
            DateTime dt = new DateTime(DateTime.Now.Ticks);
            Dictionary<string, ChatSession> authDic = new Dictionary<string, ChatSession>();
            lock(sessionDic)
            {
                authDic = new Dictionary<string, ChatSession>(sessionDic);
            }
            if(authDic.ContainsKey(sessionId))
            {
                ChatSession cs = authDic[sessionId];
                result.result = cs;
                if(result.User() == null)
                {
                    result.message = "存在しないユーザー";
                    result.result = null;
                    return result;
                }
                result.result = null;
                if(cs.expirationDate.Ticks >= dt.Ticks)
                {
                    ret = cs;
                    result.message = "成功";
                    result.status = true;
                    result.result = cs;
                }
                else
                {
                    result.message = "存在しないセッションID";
                    lock(sessionDeleteList)
                    {
                        sessionDeleteList.Add(sessionId);
                    }
                }
            }
            else
            {
                result.message = "存在しないセッションID";
                return result;
            }
            return result;
        }

        public static DeleteSessionResult DeleteSession(string sessionId)
        {
            DeleteSessionResult rsr = new DeleteSessionResult();
            rsr.status = false;
            rsr.message = "謎失敗　管理者に問い合わせてください。";
            rsr.result = null;
            ChatSession? cs = GetSession(sessionId);

            if (cs == null)
            {
                rsr.message = "存在しないセッションID";
            }
            lock (sessionDeleteList)
            {
                sessionDeleteList.Add(sessionId);
            }
            rsr.status = true;
            rsr.result = cs;
            rsr.message = "成功";
            return rsr;

        }

        //mongodbにセッションを追加
        private static void LockedIssueSession()
        {
            List<ChatSession> addSessionList = new List<ChatSession>();
            lock(sessionAddList)
            {
                if (sessionAddList.Count > 0)
                {
                    addSessionList = new List<ChatSession>(sessionAddList);
                    sessionAddList.Clear();
                }
            }
            if (addSessionList.Count <= 0)
            {
                return;
            }
            lock (isSessionChangedLockObj)
            {
                isSessionChanged = true;
            }


            lock (sessionDic)
            {
                foreach(ChatSession session in addSessionList)
                {
                    foreach (ChatSession cs in sessionDic.Values.ToList())
                    {
                        if(cs.userId == session.userId)
                        {
                            lock (sessionDeleteList)
                            {
                                //ユーザがすでにセッションを持っていたらそれを削除して新しいセッションを作成する
                                sessionDeleteList.Add(cs.sessionId);
                            }
                        }
                    }
                    //sessionDic.Add(session.sessionId, session);
                }
            }
            if (addSessionList.Count > 0)
            {
                collection.InsertMany(addSessionList);
            }
        }

        private static void LockedRemoveSession()
        {
            List<string> removeList = new List<string>();
            lock (sessionDeleteList)
            {
                if (sessionDeleteList.Count > 0)
                {
                    removeList = new List<string>(sessionDeleteList);
                    sessionDeleteList.Clear();
                }
            }
            if(removeList.Count <= 0) { return; }
            lock (isSessionChangedLockObj)
            {
                isSessionChanged = true;
            }

            foreach (string remove in removeList)
            {
                //sessionDic.Remove(remove);

                collection.DeleteMany(Builders<ChatSession>.Filter.Eq("session_id", remove));
            }
        }

        private static void LockedRenewSession()
        {
            List<string> renewList = new List<string>();
            lock (sessionRenewList)
            {
                if (sessionRenewList.Count > 0)
                {
                    renewList = new List<string>(sessionRenewList);
                    sessionRenewList.Clear();
                }
            }
            if(renewList.Count <= 0) { return; }
            lock (isSessionChangedLockObj)
            {
                isSessionChanged = true;
            }

            foreach (string renew in renewList)
            {
                DateTime dt = DateTime.Now.AddMinutes(SESSION_EXPIRATION_DATE);
                //sessionDic[renew].expiraitonDate = dt;
                collection.UpdateMany(Builders<ChatSession>.Filter.Eq("session_id", renew), Builders<ChatSession>.Update.Set("expiration_date", dt));
            }

        }

        //mongodbをsessionDicに読み込み
        private static void LockedSyncSessionDic()
        {
            lock(isSessionChangedLockObj)
            {
                if (!isSessionChanged)
                {
                    return;
                }
                isSessionChanged = false;
            }
            List<ChatSession> csList = new List<ChatSession>();
            Dictionary<string,ChatSession> csDic = new Dictionary<string,ChatSession>();
            try
            {
                csList = collection.Find(Builders<ChatSession>.Filter.Empty).ToList();
            }
            catch
            {

            }
            if(csList == null) { return; }
            if(csList.Count > 0 && csList != null)
            {
                foreach (ChatSession cs in csList)
                {
                    //mongodbはDateTimeをUTCで保存するのでJSTに変換する
                    cs.expirationDate = cs.expirationDate.ToLocalTime();
                    csDic.Add(cs.sessionId, cs);
                }
            }
            lock (sessionDic)
            {
                sessionDic = new Dictionary<string, ChatSession>(csDic);
            }
        }

        //mongodbの操作はここでやること
        private static void StartWriting(Object source, ElapsedEventArgs e)
        {
            lock(isWritingLockObject)
            {
                if (isWriting) { return; }
                isWriting = true;
            }

            LockedIssueSession();
            LockedRemoveSession();
            LockedRenewSession();
            LockedSyncSessionDic();

            lock (isWritingLockObject)
            {
                isWriting = false;
            }
        }

        public static void ManualWriting()
        {
            lock (isWritingLockObject)
            {
                if (isWriting) { return; }
                isWriting = true;
            }

            LockedIssueSession();

            lock (isWritingLockObject)
            {
                isWriting = false;
            }
        }
    }

    public class ChatSession
    {
        public ChatSession() { }
        public ChatSession(ChatSession cs)
        {
            this.sessionId = cs.sessionId;
            this.userId = cs.userId;
            this.expirationDate = cs.expirationDate;
        }
        //メンバ名はmongodbのコレクションのメンバ名と同じにすること
        [BsonId]
        ObjectId id { get; set; }

        [BsonElement("session_id")]
        public string sessionId { get; set; }

        [BsonElement("user_id")]
        public string userId { get; set; }

        [BsonElement("expiration_date")]
        public DateTime expirationDate { get; set; }


    }

    public class IssueSessionResult
    {
        public bool status { get; set; }
        public string message { get; set; }
        public ChatSession? result { get; set; }
    }

    public class AuthSessionResult
    {
        public AuthSessionResult() { }
        public AuthSessionResult(AuthSessionResult asr)
        {
            this.status = asr.status;
            this.message = asr.message;
            if(asr.result == null) { return; }
            this.result = new ChatSession(asr.result);
        }
        public bool status { get; set; }
        public string message { get; set; }
        public ChatSession? result { get; set; }
        public ChatUser? user { get; set; }
        public ChatUser? User()
        {
            if(result == null) { return null; }
            ChatUser? user = ChatUserController.GetUser(result.userId);
            if(user == null) { return null; }
            return new ChatUser(user);
        }
    }

    public class DeleteSessionResult
    {
        public bool status { get; set; }
        public string message { get; set; }
        public ChatSession? result { get; set; }
    }

    public class RenewSessionResult
    {
        public bool status { get; set; }
        public string message { get; set; }
        public ChatSession? result { get; set; }
    }

}
