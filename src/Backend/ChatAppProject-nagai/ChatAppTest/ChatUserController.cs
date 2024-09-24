using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using ChatAppTest.FunctionController.User;
using ChatAppTest.FunctionController.Session;

namespace ChatAppTest.FunctionController.User
{
    public static class ChatUserController
    {
        public const string USER_DB_NAME = "test";
        public const string USER_COLLECTINO_NAME = "chatAppUser";
        public static readonly List<string> USER_TAGS = new List<string>()
        {
            "admin",
            "moderator",
        };
        //デバッグ用
        public static readonly bool isDebugMode = true;


        public static Dictionary<string, ChatUser> userDic = new Dictionary<string, ChatUser>();
        public static MongoClient client = new MongoClient("mongodb://localhost");
        //public static MongoDatabaseSettings db_option = new MongoDatabaseSettings()
        //{
            
        //}
        public static IMongoDatabase db = client.GetDatabase(USER_DB_NAME);
        public static IMongoCollection<ChatUser> collection;
        public static List<ChatUser> userAddList = new List<ChatUser>();
        public static List<string> userDeleteList = new List<string>();
        public static List<ChatUser> userChangeList = new List<ChatUser>();
        private static System.Timers.Timer timer = new System.Timers.Timer();
        public static bool isWriting = false;
        private static object isWritingLockObject = new object();
        private static bool isUserChanged = true;
        private static object isUserChangedLockObj = new object();


        public static void Initialize()
        {
            isWriting = true;
            try
            {
                db.CreateCollection(USER_COLLECTINO_NAME);
            }
            catch
            {

            }
            collection = db.GetCollection<ChatUser>(USER_COLLECTINO_NAME);
            timer.Interval = 100;
            timer.Elapsed += StartWriting;
            timer.AutoReset = true;
            timer.Enabled = true;
            isWriting = false;
        }

        public static ChatUser? GetUser(string userId)
        {
            lock(userDic)
            {
                if (userDic.ContainsKey(userId))
                {
                    return userDic[userId];
                }
                else
                {
                    return null;
                }
            }
        }

        public static AuthUserResult AuthUser(string userId, string password)
        {
            AuthUserResult aur = new AuthUserResult();
            aur.status = false;
            aur.message = "謎失敗　管理者に問い合わせてください。";
            aur.result = null;
            ChatUser? ret = null;
            Dictionary<string, ChatUser> users = new Dictionary<string, ChatUser>();
            lock(userDic)
            {
                if(userDic.ContainsKey(userId))
                {
                    if (userDic[userId].password == password)
                    {
                        ret = userDic[userId];
                        aur.result = ret;
                        aur.status = true;
                        aur.message = "成功";
                    }
                    else
                    {
                        aur.message = "パスワードが違います。";
                    }
                }
                else
                {
                    aur.message = "存在しないユーザーです。";
                }
            }
            
            return aur;
        }

        //パスワードは生で送ること　フロントでハッシュを求めてはいけない
        //独り言：自分たちで作ったシステムを自分たちでハッキングしてみるってのも面白いかも
        public static CreateUserResult CreateUser(string userId, string familyName, string firstName, string language, string studentId, string password, string nickname_ja, string nickname_en)
        {
            CreateUserResult result = new CreateUserResult();
            result.message = "ユーザー登録失敗　管理者に問い合わせてください。";
            result.status = false;
            result.result = null;

            string? hashed = CalcHash(password);
            if (hashed == null)
            {
                result.message = "パスワードハッシュの計算に失敗しました。管理者に問い合わせてください。";
                return result;
            }

            if(userId == "" || userId == null)
            {
                result.message = "ユーザー名が空です";
                return result;
            }

            if(!Regex.IsMatch(userId, "^[0-9a-zA-Z-_]+$"))
            {
                result.message = "使用不可能な文字が含まれています";
                return result;
            }

            if(userId.Length < 4)
            {
                result.message = "ユーザーIDは4文字以上にしてください";
                return result;
            }

            if(password.Length < 8)
            {
                result.message = "パスワードは8文字以上にしてください";
                return result;
            }



            ChatUser user = new ChatUser();
            List<string> tags = new List<string>();
            user.tags = tags;
            user.userId = userId;
            user.familyName = familyName;
            user.firstName = firstName;
            user.studentId = studentId;
            user.nickname_ja = nickname_ja;
            user.nickname_en = nickname_en;
            user.password = hashed;
            user.language = language;

            lock(userDic)
            {
                foreach(ChatUser cu in userDic.Values)
                {
                    if (cu.userId == userId)
                    {
                        result.message = ("既存のユーザーID");
                        return result;
                    }
                }
            }

            lock (userAddList)
            {
                foreach(ChatUser cu in userAddList)
                {
                    if (cu.userId == userId)
                    {
                        result.message = ("既存のユーザーID");
                        return result;
                    }
                }
                userAddList.Add(user);
            }
            result.result = user;
            result.status = true;
            result.message = "成功";
            return result;
        }

        public static ChangeUserResult ChangeUser(string userId, string password, string familyName = "", string firstName = "", string language = "", string newPassword = "", string nickname_ja = "", string nickname_en = "")
        {
            ChangeUserResult result = new ChangeUserResult();
            result.status = false;
            result.message = "謎失敗　管理者に問い合わせてください。";
            result.result = null;
            AuthUserResult? aur = AuthUser(userId, password);
            if (!aur.status)
            {
                result.message = aur.message;
                return result;
            }
            if(aur.result == null)
            {
                result.message = "謎失敗２　管理者に問い合わせてください。";
                return result;
            }
            ChatUser cu = aur.result;

            if (familyName != "") cu.familyName = familyName;
            if (firstName != "") cu.firstName = firstName;
            if (language != "") cu.language = language;
            if (newPassword != "")
            {
                if(newPassword.Length < 8)
                {
                    result.message = "パスワードは8文字以上";
                    return result;
                }
                string? a = CalcHash(newPassword);
                if (a != null) cu.password = a;
            }
            if (nickname_ja != "") cu.nickname_ja = nickname_ja;
            if (nickname_en != "") cu.nickname_en = nickname_en;
            lock(userChangeList)
            {
                userChangeList.Add(cu);
            }
            result.status = true;
            result.message = "成功";
            result.result = cu;
            return result;
        }

        public static DeleteUserResult DeleteUser(string userId, string password)
        {
            DeleteUserResult result = new DeleteUserResult();
            result.status = false;
            result.message = "謎失敗　管理者に問い合わせてください。";
            result.result = null;
            AuthUserResult? aur = AuthUser(userId, password);
            if (!aur.status)
            {
                result.message = aur.message;
                return result;
            }
            if (aur.result == null)
            {
                result.message = "謎失敗２　管理者に問い合わせてください。";
                return result;
            }
            lock(userDeleteList)
            {
                userDeleteList.Add(userId);
            }
            result.status = true;
            result.message = "成功";
            result.result = aur.result;
            return result;

        }

        public static SetTagsResult SetTags(string sessionId, string editUser, string[] tags)
        {
            SetTagsResult str = new SetTagsResult();
            str.status = false;
            str.message = "謎失敗";
            str.result = null;
            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if(!asr.status)
            {
                str.message = asr.message;
                return str;
            }
            if(asr.result == null)
            {
                str.message = "謎失敗２";
                return str;
            }
            Dictionary<string, ChatUser> ud = new Dictionary<string, ChatUser>();
            lock (userDic)
            {
                ud = new Dictionary<string, ChatUser>(userDic);
            }
            if (!ud.ContainsKey(asr.result.userId))
            {
                str.message = "存在しないユーザー";
                return str;
            }
            ChatUser cu = ud[asr.result.userId];
            List<string> userTags = cu.tags;
            List<string> addTags = tags.ToList();
            if (!userTags.Contains("admin"))
            {
                str.message = "権限がありません";
                //adminユーザが存在しない時用
                if(!isDebugMode)
                {
                    return str;
                }
            }

            //無効なタグを付与することを禁止する
            foreach (string addTag in addTags)
            {
                if (!USER_TAGS.Contains(addTag))
                {
                    str.message = "存在しないタグが含まれています：" + addTag;
                    return str;
                }
            }
            //adminユーザ以外がadminタグを付与することを禁止する
            if (addTags.Contains("admin"))
            {
                if(cu.userId != "admin")
                {
                    str.message = "adminユーザ以外がadminタグを付与することはできません";
                    return str;
                }
            }
            //adminタグを持つユーザ以外がモデレータタグを付与することを禁止する
            if (addTags.Contains("moderator"))
            {
                if (!cu.tags.Contains("admin"))
                {
                    str.message = "adminタグがついたユーザ以外がmoderatorタグを付与することはできません";
                    return str;
                }
            }
            if (!ud.ContainsKey(editUser))
            {
                str.message = "編集対象のユーザーは存在しません";
                return str;
            }
            cu = ud[editUser];
            cu.tags.Clear();
            //adminユーザがadminタグを取り除くのを禁止する
            if(editUser == "admin")
            {
                if (!addTags.Contains("admin"))
                {
                    str.message = "adminユーザがadminタグを取り除くことはできません";
                    //return str;
                }
            }
            List<string> tmpList = new List<string>();
            foreach(string tag in tags)
            {
                if(tag != null && tag != "")
                {
                    tmpList.Add(tag);
                }
                else
                {
                    str.message = "タグの要素にnullか空白が含まれています";
                    return str;
                }
            }
            cu.tags.AddRange(tags);
            lock (userChangeList)
            {
                userChangeList.Add(cu);
            }
            str.message = "成功";
            str.status = true;
            str.result = cu;
            return str;
        }
        public static void StartWriting(Object source, ElapsedEventArgs e)
        {
            if (isWriting) return;
            isWriting = true;

            LockedCreateUser();
            LockedChangeUser();
            LockedDeleteUser();
            LockedSyncUserDic();

            isWriting = false;
        }

        private static void LockedSyncUserDic()
        {
            lock (isUserChangedLockObj)
            {
                if (!isUserChanged)
                {
                    return;
                }
                isUserChanged = false;
            }
            List<ChatUser> cuList = new List<ChatUser>();
            Dictionary<string, ChatUser> csDic = new Dictionary<string, ChatUser>();
            try
            {
                cuList = collection.Find(Builders<ChatUser>.Filter.Empty).ToList();
            }
            catch
            {

            }
            if (cuList == null) { return; }
            if (cuList.Count > 0 && cuList != null)
            {
                foreach (ChatUser cs in cuList)
                {
                    csDic.Add(cs.userId, cs);
                }
            }
            lock (userDic)
            {
                userDic = new Dictionary<string, ChatUser>(csDic);
            }
        }

        private static void LockedCreateUser()
        {
            List<ChatUser> addChatUser = new List<ChatUser>();
            List<ChatUser> tmpList = new List<ChatUser>();
            lock (userAddList)
            {
                if(userAddList.Count > 0)
                {
                    addChatUser = new List<ChatUser>(userAddList);
                    userAddList.Clear();
                }
            }
            if(addChatUser.Count <= 0) { return; }
            lock (isUserChangedLockObj)
            {
                isUserChanged = true;
            }
            foreach(ChatUser cu in addChatUser)
            {
                if (userDic.ContainsKey(cu.userId))
                {
                    //なぜかユーザ重複チェックを通過してきた奴らはここに行く　デバッグ用にどうぞ
                    continue;
                }
                tmpList.Add(cu);
            }
            if(tmpList.Count > 0)
            {
                collection.InsertMany(tmpList);
            }
        }

        private static void LockedDeleteUser()
        {
            List<string> deleteChatUserList = new List<string>();
            List<string> tmpList = new List<string>();
            lock (userDeleteList)
            {
                if (userDeleteList.Count > 0)
                {
                    deleteChatUserList = new List<string>(userDeleteList);
                    userDeleteList.Clear();
                }
            }
            if(deleteChatUserList.Count <= 0) { return; }
            lock (isUserChangedLockObj)
            {
                isUserChanged = true;
            }

            foreach (string cu in deleteChatUserList)
            {
                if (userDic.ContainsKey(cu))
                {
                    tmpList.Add(cu);
                }
            }
            if (tmpList.Count > 0)
            {
                foreach (string cu in tmpList)
                {
                    collection.DeleteMany(Builders<ChatUser>.Filter.Eq("user_id", cu));
                }
            }
        }

        private static void LockedChangeUser()
        {
            List<ChatUser> changeChatUserList = new List<ChatUser>();
            List<ChatUser> tmpList = new List<ChatUser>();
            lock (userChangeList)
            {
                if (userChangeList.Count > 0)
                {
                    changeChatUserList = new List<ChatUser>(userChangeList);
                    userChangeList.Clear();
                }
            }
            if(changeChatUserList.Count <= 0) { return; }
            lock (isUserChangedLockObj)
            {
                isUserChanged = true;
            }

            foreach (ChatUser cu in changeChatUserList)
            {
                if (userDic.ContainsKey(cu.userId))
                {
                    tmpList.Add(cu);
                }
            }
            if (tmpList.Count > 0)
            {
                foreach (ChatUser cu in tmpList)
                {
                    collection.ReplaceOne(Builders<ChatUser>.Filter.Eq("user_id", cu.userId), cu);
                }
            }

        }

        public static string? CalcHash(string data)
        {
            if (data == null || data == String.Empty) { return null; }
            string hashed = String.Empty;
            using (SHA256 hash = SHA256.Create())
            {
                byte[] bt = hash.ComputeHash(Encoding.UTF8.GetBytes(data));
                foreach (byte b in bt)
                {
                    hashed += $"{b:X2}";
                }
            }
            return hashed;
        }
    }

    public class ChatUser
    {
        public ChatUser(ChatUser? cu)
        {
            if(cu == null) { return; }
            this.userId = cu.userId;
            this.familyName = cu.familyName;
            this.firstName = cu.firstName;
            this.password = cu.password;
            this.studentId= cu.studentId;
            this.language = cu.language;
            this.nickname_ja = cu.nickname_ja;
            this.nickname_en = cu.nickname_en;
            this.tags = cu.tags;
        }
        public ChatUser()
        {

        }
        [BsonId]
        ObjectId id { get; set; }

        [BsonElement("user_id")]
        public string userId { get; set; }

        [BsonElement("family_name")]
        public string familyName { get; set; }

        [BsonElement("first_name")]
        public string firstName { get; set; }

        [BsonElement("password")]
        internal string? password { get; set; }

        [BsonElement("student_id")]
        public string studentId { get; set; }

        [BsonElement("language")]
        public string language { get; set; }

        [BsonElement("nickname_ja")]
        public string nickname_ja { get; set; }

        [BsonElement("nickname_en")]
        public string nickname_en { get; set; }

        [BsonElement("tags")]
        public List<string> tags { get; set; }
    }
    public class CreateUserResult
    {
        public ChatUser? result { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public CreateUserResult(CreateUserResult cur)
        {
            this.status = cur.status;
            this.message = cur.message;
            if (cur.result == null) return;
            this.result = new ChatUser(cur.result);
        }
        public CreateUserResult()
        {

        }
    }

    public class AuthUserResult
    {
        public AuthUserResult()
        {

        }
        public AuthUserResult(AuthUserResult aur)
        {
            this.status = aur.status;
            this.message = aur.message;
            if (aur.result == null) return;
            this.result = new ChatUser(aur.result);
        }
        public ChatUser? result { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
    }

    public class ChangeUserResult
    {
        public ChangeUserResult()
        {

        }
        public ChangeUserResult(ChangeUserResult cur)
        {
            this.status = cur.status;
            this.message = cur.message;
            if(cur.result == null) return;
            this.result = new ChatUser(cur.result);
        }
        public ChatUser? result { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
    }
    public class DeleteUserResult
    {
        public DeleteUserResult()
        {

        }
        public DeleteUserResult(DeleteUserResult dur)
        {
            this.status = dur.status;
            this.message = dur.message;
            if(dur.result == null) return;
            this.result = new ChatUser(dur.result);
        }
        public ChatUser? result { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
    }
    public class SetTagsResult
    {
        public SetTagsResult() { }
        public SetTagsResult(SetTagsResult str)
        {
            this.status = str.status;
            this.message = str.message;
            if(str.result == null) return;
            this.result = new ChatUser(str.result);
        }
        public ChatUser? result { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
    }

}
