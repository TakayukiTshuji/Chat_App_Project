using ChatAppTest.Controllers;
using ChatAppTest.FunctionController.Session;
using ChatAppTest.FunctionController.User;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using System.Xml.Linq;
using static ChatAppTest.Controllers.ChatCtl;

namespace ChatAppTest.FunctionController.Chat
{
    public class ChatController
    {
        public const string MESSAGE_DB_NAME = "test";
        public const string MESSAGE_COLLECTIN_NAME_BASE = "chatAppMessage";
        public const string ROOM_COLLECTION_NAME = "chatAppRoom";

        public static readonly List<string> validRoomTags = new List<string>()
        {
            "admin",
            "mods",
            "public",
            "private",
            "dm"
        };

        private static ArrayList addMessage = new ArrayList();
        //private static ArrayList safeList = ArrayList.Synchronized(addMessage);
        private static List<int> removeRoomList = new List<int>();
        private static ArrayList addRooms = new ArrayList();
        //private static ArrayList safeAddRooms = ArrayList.Synchronized(addRooms);
        private static Dictionary<int, ChatRoom> rooms = new Dictionary<int, ChatRoom>();
        private static Dictionary<int, List<MessageTemplate>> messages = new Dictionary<int, List<MessageTemplate>>();
        private static object roomsNextNumLockObj = new object();
        private static int roomsNextNum = 0;
        private static Dictionary<int, ChatRoom> patchRoomList = new Dictionary<int, ChatRoom>();
        private static Dictionary<int, Dictionary<int, int>> readCountIncrement = new Dictionary<int, Dictionary<int, int>>();
        private static Dictionary<int, Dictionary<int, int>> readCount = new Dictionary<int, Dictionary<int, int>>();
        private static Dictionary<int, List<string>> writingNow = new Dictionary<int, List<string>>();

        //private static readonly string chatRoomFilePath = @"./chatrooms.txt";


        private static List<ChatRoom> newChatRoomList = new List<ChatRoom>();
        public static MongoClient client = new MongoClient("mongodb://localhost");
        public static IMongoDatabase db = client.GetDatabase(MESSAGE_DB_NAME);
        public static IMongoCollection<ChatRoom> roomsCollection = db.GetCollection<ChatRoom>(ROOM_COLLECTION_NAME);

        public static bool isMessageChanged = true;
        private static object isMessageChangedLockObj = new object();

        private static Dictionary<int, List<int>> removeDic = new Dictionary<int, List<int>>();
        private static Dictionary<int, Dictionary<int, string>> changeDic = new Dictionary<int, Dictionary<int, string>>();
        private static bool isWriting = false;
        private static object isWritingLockObj = new object();
        private static System.Timers.Timer timer = new System.Timers.Timer();
        //private static readonly Encoding enc = Encoding.UTF8;
        //private static List<int> messageId = new List<int>();
        //private static int chatRoomId = 0;

        //intはlockできなかったのでobjectにする
        //private static Object chatRoomIdLocked = null;
        //private static Object messageIdLocked = null;

        //メッセージを書き込む　
        public static WriteMessageResult WriteMessage(string message, string sessionId, int room)
        {
            WriteMessageResult wmr = new WriteMessageResult();
            wmr.status = false;
            wmr.message = "謎失敗";
            wmr.result = null;
            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if (!asr.status)
            {
                wmr.message = asr.message;
                return wmr;
            }
            if(asr.result == null)
            {
                wmr.message = "謎失敗２";
                return wmr;
            }
            Dictionary<int, ChatRoom> tmp;
            lock (rooms)
            {
                tmp = new Dictionary<int, ChatRoom>(rooms);
            }
            if(tmp.Count <= room || room < 0)
            {
                wmr.message = "部屋が存在しません";
                return wmr;
            }
            if (tmp[room].hidden)
            {
                wmr.message = "部屋が存在しません";
                return wmr;
            }
            //whitelist check
            if (tmp[room].isPrivate)
            {
                if (!tmp[room].whiteList.Contains(asr.result.userId))
                {
                    wmr.message = "部屋が存在しません";
                    return wmr;
                }
            }
            string user = asr.result.userId;
            //0:正常　1:存在しない部屋　2:認証失敗
            ArrayList content = new ArrayList();
            DateTime date = DateTime.Now;
            MessageTemplate template = new MessageTemplate();
            template.date = date;
            template.message = message;
            template.user = user;
            template.hidden = false;
            template.edited = false;
            template.room = room;
            int lastMessageId = 0;
            //lock (messageIdLocked)
            //{
            //    List<int> lmi = new List<int>((List<int>)messageIdLocked);
            //    if(lmi.Count <= room || room < 0)
            //    {
            //        wmr.message = "部屋が存在しません";
            //        return wmr;
            //    }
            //    lastMessageId = lmi[room] + 1;
            //    lmi[room] += 1;
            //    messageIdLocked = new List<int>(lmi);
            //}
            lock (rooms)
            {
                lastMessageId = rooms[room].next_id;
                rooms[room].next_id++;
            }
            template.message_id = lastMessageId;
            content.Add(template);
            content.Add(date);
            var rnd = new Random();
            while (isWriting)
            {
                Thread.Sleep(rnd.Next(10, 100));
            }
            lock(addMessage)
            {
                addMessage.Add(content);
            }
            wmr.status = true;
            wmr.message = "成功";
            wmr.result = new MessageTemplate(template);
            return wmr;
        }
        //チャット部屋を追加する
        public static AddRoomResult AddChatRooms(string name, string sessionId, bool isPrivate, bool isDm, List<string> whitelist)
        {
            AddRoomResult crr = new AddRoomResult();
            crr.message = "失敗";
            crr.status = false;

            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if (!asr.status)
            {
                crr.message = asr.message;
                return crr;
            }
            if(asr.result == null)
            {
                crr.message = "謎失敗２";
                return crr;
            }
            ChatUser? aur = ChatUserController.GetUser(asr.result.userId);
            if(aur == null)
            {
                crr.message = "謎失敗３";
                return crr;
            }
            //if(!aur.tags.Contains("moderator") && !aur.tags.Contains("admin"))
            //{
            //    crr.message = "モデレーターか管理者のみがチャット部屋を作成できます。";
            //}

            lock (rooms)
            {
                foreach(ChatRoom rum in rooms.Values)
                {
                    if(rum.name == name && !rum.hidden)
                    {
                        crr.message = "既存の部屋名";
                        return crr;
                    }
                }
            }
            ChatRoom roomInfo = new ChatRoom();
            roomInfo.name = name;
            roomInfo.creator = asr.result.userId;
            roomInfo.isPrivate = isPrivate;
            roomInfo.tags = new List<string>();
            roomInfo.whiteList = whitelist;

            //ホワイトリストに作成ユーザがあるかチェック
            if (!roomInfo.whiteList.Contains(asr.result.userId))
            {
                roomInfo.whiteList.Add(asr.result.userId);
            }
            //tag付与
            if (isPrivate)
            {
                roomInfo.tags.Add("private");
                if (isDm)
                {
                    roomInfo.tags.Add("dm");
                }
            }
            else
            {
                roomInfo.whiteList.Clear();
                roomInfo.tags.Add("public");
                if (isDm)
                {
                    crr.message = "ダイレクトメッセージはpublicにはできません";
                    return crr;
                }
            }
            int chatRoomLastId;
            lock(roomsNextNumLockObj)
            {
                chatRoomLastId = roomsNextNum;
                roomsNextNum++;
            }
            //lock (chatRoomIdLocked)
            //{
            //    chatRoomLastId = (int)chatRoomIdLocked + 1;
            //    chatRoomIdLocked = chatRoomLastId;
            //}
            //lock (messageIdLocked)
            //{
            //    List<int> tmp = new List<int>((List<int>)messageIdLocked);
            //    tmp.Add(-1);
            //    messageIdLocked = new List<int>(tmp);
            //}
            roomInfo.room_id = chatRoomLastId;
            //roomInfo.roomPath = $"./ChatRooms/{roomInfo.roomId.ToString()}";
            var rnd = new Random();
            while (isWriting)
            {
                //謎スリープ　いらんかも
                Thread.Sleep(rnd.Next(10, 100));
            }
            lock(addRooms)
            {
                addRooms.Add(roomInfo);
            }
            crr.status = true;
            crr.message = "成功";
            return crr;
        }
        //初期化　プログラム実行時に一度実行する
        public static void Initialize()
        {
            isWriting = true;
            timer.Interval = 100;
            timer.Elapsed += StartWriting;
            timer.AutoReset = true;
            timer.Enabled = true;
            //timer.Start();

            try
            {
                db.CreateCollection(ROOM_COLLECTION_NAME);
            }
            catch
            {

            }

            //loading chat rooms
            //chat room last id
            ////chatRoomIdLocked = new Object();
            ////chatRoomIdLocked = chatRoomId;
            int lastId1 = 0;
            //string? rawJson = null;
            //string? prejson = null;
            //do
            //{
            //    prejson = rawJson;
            //    rawJson = sr.ReadLine();
            //    if(rawJson != null && rawJson != "")
            //    {
            //        ChatRoom? room =  JsonSerializer.Deserialize<ChatRoom>(rawJson);
            //        if(room == null) { continue; }
            //        rooms.Add(room);
            //        //lastId++;
            //    }
            //} while (rawJson != null);
            //if(prejson != null && prejson != "")
            //{
            //    ChatRoom? cr = JsonSerializer.Deserialize<ChatRoom>(prejson);
            //    lastId1 = cr.roomId;
            //}
            List<ChatRoom> roomsListTemp = new List<ChatRoom>(roomsCollection.Find<ChatRoom>(Builders<ChatRoom>.Filter.Empty).ToList());
            Dictionary<int, ChatRoom> roomsList = new Dictionary<int, ChatRoom>();
            foreach (ChatRoom room in roomsListTemp)
            {
                roomsList.Add(room.room_id, room);
            }
            //foreach(ChatRoom room in roomsList)
            //{
            //    try
            //    {
            //        room.messagesCollection = db.GetCollection<MessageTemplate>(MESSAGE_COLLECTIN_NAME_BASE + lastId1.ToString());
            //    }
            //    catch
            //    {
            //        continue;
            //    }
            //    finally
            //    {
            //        lastId1++;
            //    }
            //}
            //lock (chatRoomIdLocked)
            //{
            //    chatRoomIdLocked = new object();
            //    chatRoomIdLocked = lastId1;
            //}
            //set message last id
            //messageIdLocked = new Object();
            //messageIdLocked = messageId;

            List<List<MessageTemplate>> mts = new List<List<MessageTemplate>>();
            //コレクションとメッセージを取得
            foreach(ChatRoom cr in roomsList.Values)
            {
                try
                {
                    cr.messagesCollection = db.GetCollection<MessageTemplate>(MESSAGE_COLLECTIN_NAME_BASE + cr.room_id.ToString());
                    mts.Add(cr.messagesCollection.Find<MessageTemplate>(Builders<MessageTemplate>.Filter.Empty).ToList());
                    cr.next_id = mts[cr.room_id].Count;
                }
                catch
                {
                    db.CreateCollection(MESSAGE_COLLECTIN_NAME_BASE + cr.room_id.ToString());
                    cr.messagesCollection = db.GetCollection<MessageTemplate>(MESSAGE_COLLECTIN_NAME_BASE + cr.room_id.ToString());
                    mts.Add(new List<MessageTemplate>());
                    cr.next_id = 0;
                }
            }
            lock(roomsNextNumLockObj)
            {
                if(roomsList.Count <= 0)
                {
                    roomsNextNum = 0;
                }
                else
                {
                    roomsNextNum = roomsList.Keys.Max() + 1;
                }
            }
            lock (rooms)
            {
                rooms = new Dictionary<int, ChatRoom>(roomsList);
            }
            //loading messages to list
            isWriting = false;
            LoadMessages();
        }
        //メッセージ読み取り
        public static ReadMessageResult ReadMessage(int room, string sessionId)
        {
            ReadMessageResult crr = new ReadMessageResult();
            crr.message = "失敗";
            crr.status = false;

            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if (!asr.status)
            {
                crr.message = asr.message;
                return crr;
            }
            if(asr.result == null)
            {
                crr.message = "謎失敗２";
                return crr;
            }
            Dictionary<int, ChatRoom> tmp;
            lock (rooms)
            {
                tmp = new Dictionary<int, ChatRoom>(rooms);
            }
            if(tmp == null)
            {
                crr.message = "謎失敗e";
                return crr;
            }
            if (tmp.Count <= 0)
            {
                crr.message = "部屋が存在しません";
                return crr;
            }
            if (tmp.Keys.Max() < room || room < 0)
            {
                crr.message = "部屋が存在しません";
                return crr;
            }
            //whitelist check
            Dictionary<int, ChatRoom> temp2 = new Dictionary<int, ChatRoom>(tmp);
            foreach(ChatRoom room2 in tmp.Values)
            {
                if(room2.isPrivate)
                {
                    if(!room2.whiteList.Contains(asr.result.userId))
                    {
                        temp2.Remove(room2.room_id);
                    }
                }
            }
            tmp = temp2;
            if (!tmp.ContainsKey(room))
            {
                crr.message = "部屋が存在しません";
                return crr;
            }
            ChatRoom cr = tmp[room];
            if (cr.hidden)
            {
                crr.message = "部屋が存在しません ";
                return crr;
            }
            List<MessageTemplate> data = new List<MessageTemplate>();
            lock (messages)
            {
                if (messages.Count <= room || room < 0)
                {
                    crr.message = "謎失敗g";
                    return crr;
                }
                data = new List<MessageTemplate>(messages[room]);
            }
            //if(((List<int>)messageIdLocked).Count <= room || room < 0)
            //{
            //    crr.message = "部屋が存在しません";
            //    return crr;
            //}
            if(data.Count <= 0)
            {
                crr.message = "メッセージがありません";
                return crr;
            }
            data.RemoveAll(x => x.hidden);
            if(data.Count <= 0)
            {
                crr.message = "メッセージがありません";
                return crr;
            }
            //ニックネームを入力
            List<string> tmpUsers = new List<string>();
            foreach(MessageTemplate mt in data)
            {
                if (!tmpUsers.Contains(mt.user))
                {
                    tmpUsers.Add(mt.user);
                }
            }
            Dictionary<string, string> nicknameDic_jp = new Dictionary<string, string>();
            Dictionary<string, string> nicknameDic_en = new Dictionary<string, string>();
            foreach (string user in tmpUsers)
            {
                ChatUser? cu = new ChatUser(ChatUserController.GetUser(user));
                if(cu == null)
                {
                    nicknameDic_jp.Add(user, "削除されたユーザー");
                    nicknameDic_en.Add(user, "DeletedUser");
                    continue;
                }
                nicknameDic_jp.Add(user, cu.nickname_ja);
                nicknameDic_en.Add(user, cu.nickname_en);
            }
            //nicknameDicをdataに適応させる
            foreach(MessageTemplate mt in data)
            {
                mt.nickname_jp = nicknameDic_jp[mt.user];
                mt.nickname_en = nicknameDic_en[mt.user];
            }

            crr.result = data;
            crr.status = true;
            crr.message = "成功";
            return crr;
        }
        //チャット部屋の一覧を取得
        public static GetRoomsResult GetRoomList(string sessionId)
        {
            GetRoomsResult grr = new GetRoomsResult();
            grr.message = "謎失敗";
            grr.status = false;
            grr.result = null;

            AuthSessionResult asr = new AuthSessionResult(ChatSessionController.AuthSession(sessionId));
            if (!asr.status)
            {
                grr.message = asr.message;
                return grr;
            }
            if(asr.result == null)
            {
                grr.message = "謎失敗２";
                return grr;
            }

            Dictionary<int, ChatRoom> cr;
            lock(rooms)
            {
                cr = new Dictionary<int, ChatRoom>(rooms);
            }
            //cr.RemoveAll(x => x.hidden);
            Dictionary<int, ChatRoom> temp2 = new Dictionary<int, ChatRoom>(cr);
            foreach (ChatRoom room in cr.Values)
            {
                if (room.hidden)
                {
                    temp2.Remove(room.room_id);
                }
            }
            cr = temp2;
            //whitelist check for private room
            Dictionary<int, ChatRoom> temp = new Dictionary<int, ChatRoom>(cr);
            foreach(ChatRoom room in cr.Values)
            {
                if (room.isPrivate)
                {
                    string? find = room.whiteList.Find(x => x == asr.result.userId);
                    if(find == null)
                    {
                        temp.Remove(room.room_id);
                    }
                }
            }
            cr = temp;

            Dictionary<int, ChatRoom> crs1 = new Dictionary<int, ChatRoom>();

            foreach (ChatRoom room in cr.Values)
            {
                ChatRoom cr1 = new ChatRoom(room);
                crs1.Add(cr1.room_id, cr1);
            }
            //cr.ForEach(x => x.messagesCollection = null);
            if(crs1.Count <= 0)
            {
                grr.message = "部屋が一つもありません　管理者に問い合わせてください";
                return grr;
            }
            //if(crs1.Keys.Max() <= 0)
            //{

            //}
            grr.result = crs1.Values.ToList();
            grr.message = "成功";
            grr.status = true;
            return grr;
        }

        public static DeleteRoomResult DeleteRoom(string sessionId, int room)
        {
            DeleteRoomResult drr = new DeleteRoomResult();
            drr.status = false;
            drr.message = "謎失敗";

            AuthSessionResult asr = new AuthSessionResult(ChatSessionController.AuthSession(sessionId));
            if (!asr.status || asr.User() == null)
            {
                drr.message = asr.message;
                return drr;
            }
            Dictionary<int, ChatRoom> crs = new Dictionary<int, ChatRoom>();
            lock (rooms)
            {
                crs = new Dictionary<int, ChatRoom>(rooms);
            }
            if(crs.Count <= 0)
            {
                drr.message = "部屋が存在しません";
                return drr;
            }
            if (crs.Keys.Max() < room || room < 0)
            {
                drr.message = "部屋が存在しません";
                return drr;
            }

            ChatRoom cr = crs[room];
            ChatUser? cu = asr.User();
            if (cu == null || cr == null)
            {
                drr.message = "謎失敗３";
                return drr;
            }
            if (cr.hidden)
            {
                drr.message = "部屋が存在しません";
                return drr;
            }
            if (!cu.tags.Contains("admin") && !cu.tags.Contains("moderator"))
            {
                drr.message = "部屋の削除は管理者かモデレータに問い合わせてください";
                return drr;
            }
            lock (removeRoomList)
            {
                removeRoomList.Add(room);
            }
            drr.status = true;
            drr.message = "成功";
            return drr;
        }

        public static PatchRoomResult PatchRoom(string sessionId, int roomId, string roomName, bool isPrivate, List<string> whiteList)
        {
            PatchRoomResult prr = new PatchRoomResult();
            prr.message = "謎失敗";
            prr.status = false;
            prr.result = null;
            
            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if(asr == null)
            {
                return prr;
            }
            if (!asr.status)
            {
                prr.message = asr.message;
                return prr;
            }
            if (roomId < 0)
            {
                prr.message = "部屋IDが範囲外です";
                return prr;
            }
            if(asr.result == null)
            {
                prr.message = "謎失敗２";
                return prr;
            }

            Dictionary<int, ChatRoom> tmp;
            lock (rooms)
            {
                tmp = new Dictionary<int, ChatRoom>(rooms);
            }
            if(tmp.Count <= 0)
            {
                prr.message = "部屋が存在しません";
                return prr;
            }
            if (tmp.Keys.Max() < roomId || roomId < 0)
            {
                prr.message = "部屋が存在しません";
                return prr;
            }
            ChatRoom cr = tmp[roomId];
            if (cr.hidden)
            {
                prr.message = "部屋が存在しません ";
                return prr;
            }

            if(cr.creator != asr.result.userId)
            {
                prr.message = "部屋の作成者のみが部屋を編集できます";
                return prr;
            }

            cr.whiteList = whiteList;
            //whitelistに作成ユーザが含まれているかのチェック
            if (!cr.whiteList.Contains(asr.result.userId))
            {
                cr.whiteList.Add(asr.result.userId);
            }
            cr.isPrivate = isPrivate;
            lock (rooms)
            {
                foreach(ChatRoom crTemp in rooms.Values)
                {
                    if(crTemp.name == roomName)
                    {
                        prr.message = "既存の部屋名";
                        return prr;
                    }
                }
            }
            cr.name = roomName;

            lock(patchRoomList)
            {
                if (patchRoomList.ContainsKey(roomId))
                {
                    patchRoomList[roomId] = cr;
                }
                else
                {
                    patchRoomList.Add(roomId, cr);
                }
                prr.message = "成功";
                prr.status = true;
                prr.result = cr;
            }
            return prr;

        }
        //メッセージ削除（非表示にする）
        public static HideMessageResult HideMessage(string sessionId, int room, int messageId)
        {
            HideMessageResult hmr = new HideMessageResult();
            hmr.status = false;
            hmr.message = "謎失敗";

            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if (asr.result == null)
            {
                hmr.message = "謎失敗２";
                return hmr;
            }
            if(!asr.status)
            {
                hmr.message = asr.message;
                return hmr;
            }
            if(room < 0 || messageId < 0)
            {
                hmr.message = "パラメータの値が範囲外です";
                return hmr;
            }

            Dictionary<int, ChatRoom> tmp;
            lock (rooms)
            {
                tmp = new Dictionary<int, ChatRoom>(rooms);
            }
            if (tmp.Count <= 0)
            {
                hmr.message = "部屋が存在しません";
                return hmr;
            }
            if (tmp.Keys.Max() < room || room < 0)
            {
                hmr.message = "部屋が存在しません";
                return hmr;
            }
            ChatRoom cr = tmp[room];
            if (cr.hidden)
            {
                hmr.message = "部屋が存在しません ";
                return hmr;
            }

            List<MessageTemplate> mts;
            lock (messages)
            {
                if(messages.Count <= 0)
                {
                    hmr.message = "メッセージが存在しません";
                    return hmr;
                }
                if(messages.Keys.Max() < room || room < 0)
                {
                    hmr.message = "メッセージが存在しません";
                    return hmr;
                }
                mts = new List<MessageTemplate>(messages[room]);
            }
            //if (tmp..Count > room)
            //{
            if(mts.Count <= messageId || messageId < 0)
            {
                hmr.message = "存在しないメッセージ";
                return hmr;
            }
            if (mts[messageId].hidden)
            {
                hmr.message = "すでに非表示です";
                return hmr;
            }
            //}
            //else
            //{
            //    hmr.message = "存在しない部屋";
            //    return hmr;
            //}
            //List<List<MessageTemplate>> tmp2;
            //lock(messages)
            //{
            //    tmp2 = new List<List<MessageTemplate>>(messages);
            //}
            ChatUser? cu = asr.User();
            if(cu == null)
            {
                hmr.message = "無効なユーザー";
                return hmr;
            }
            if (!cu.tags.Contains("moderator"))
            {
                if (!ChatSessionController.CheckUser(sessionId, mts[messageId].user))
                {
                    hmr.message = "メッセージを削除できるのはメッセージ投稿者かモデレーターのみです。";
                    return hmr;
                }
            }
            lock(removeDic)
            {
                if (removeDic.ContainsKey(room))
                {
                    List<int> il = new List<int>(removeDic[room]);
                    if (il.Contains(messageId))
                    {
                        hmr.message = "既に削除されています";
                        return hmr;
                    }
                    il.Add(messageId);
                    removeDic.Add(room, il);
                }
                else
                {
                    List<int> il = new List<int>();
                    il.Add(messageId);
                    removeDic[room] = il;
                }
            }
            hmr.status = true;
            hmr.message = "成功";
            return hmr;
        }

        public static ChangeMessageResult ChangeMessage(string sessionId, int room, int messageId, string message)
        {
            ChangeMessageResult cmr = new ChangeMessageResult();
            cmr.status = false;
            cmr.message = "謎失敗";
            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if(!asr.status)
            {
                cmr.message = asr.message;
                return cmr;
            }
            Dictionary<int, ChatRoom> crs = new Dictionary<int, ChatRoom>();
            lock(rooms)
            {
                crs = new Dictionary<int, ChatRoom>(rooms);
            }
            if(crs.Count <= 0)
            {
                cmr.message = "存在しない部屋";
                return cmr;
            }
            if (crs.Keys.Max() < room || room < 0)
            {
                cmr.message = "存在しない部屋";
                return cmr;
            }
            ChatRoom cr = crs[room];
            List<MessageTemplate> mts;
            lock (messages)
            {
                if (messages.Count <= 0)
                {
                    cmr.message = "存在しない部屋 ";
                    return cmr;
                }
                if (messages.Keys.Max() < room || room < 0)
                {
                    cmr.message = "存在しない部屋 ";
                    return cmr;
                }
                mts = new List<MessageTemplate>(messages[room]);
            }
            if (mts == null)
            {
                cmr.message = "謎失敗３";
                return cmr;
            }
            if (mts.Count <= messageId || room < 0)
            {
                cmr.message = "存在しないメッセージ";
                return cmr;
            }
            if (asr.result == null)
            {
                cmr.message = "謎失敗２";
                return cmr;
            }

            Dictionary<int, ChatRoom> tmp;
            lock (rooms)
            {
                tmp = new Dictionary<int, ChatRoom>(rooms);
            }
            if(tmp.Count <= 0)
            {
                cmr.message = "部屋が存在しません";
                return cmr;
            }
            if (tmp.Keys.Max() < room || room < 0)
            {
                cmr.message = "部屋が存在しません";
                return cmr;
            }
            if (tmp[room].hidden)
            {
                cmr.message = "部屋が存在しません";
                return cmr;
            }


            if (mts[messageId].user != asr.result.userId)
            {
                cmr.message = "書き込んだユーザーのみがメッセージを編集できます";
                return cmr;
            }
            lock(changeDic)
            {
                if(changeDic.ContainsKey(room))
                {
                    Dictionary<int, string> dic = new Dictionary<int, string>(changeDic[room]);
                    if(dic.ContainsKey(messageId))
                    {
                        dic[messageId] = message;
                    }
                    else
                    {
                        dic.Add(messageId, message);
                    }
                    changeDic[room] = dic;
                }
                else
                {
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    dic.Add(messageId, message);
                    changeDic.Add(room, dic);
                }
            }
            cmr.status = true;
            cmr.message = "成功";
            return cmr;
        }

        //一時的に集めたメッセージをファイルに書き込む
        //一時的に集めた追加された部屋を作成する
        private static void StartWriting(Object source, ElapsedEventArgs e)
        {
            lock (isWritingLockObj)
            {
                if (isWriting) { return; }
                isWriting = true;
            }
            AddMessages();
            AddRooms();
            LockedPatchRoom();
            LockedHideRoom();
            LockedHideMessage();
            LockedChangeMessage();
            LoadMessages();

            lock (isWritingLockObj)
            {
                isWriting = false;
            }

        }
        //メッセージをファイルに書き込み　取得用のメッセージリストに追加する
        private static void AddMessages()
        {
            ArrayList mts;
            lock (addMessage)
            {
                mts = new ArrayList(addMessage);
                addMessage.Clear();
            }
            if (mts.Count <= 0) { return; }
            lock (isMessageChangedLockObj)
            {
                isMessageChanged = true;
            }

            Dictionary<long, MessageTemplate> contentDic = new Dictionary<long, MessageTemplate>();
            foreach (ArrayList al in mts)
            {
                MessageTemplate message = (MessageTemplate)al[0];
                long ticks = ((DateTime)al[1]).Ticks;

                contentDic.Add(ticks, message);
            }

            List<MessageTemplate> sorted = new List<MessageTemplate>();

            foreach (var n in contentDic.OrderBy(c => c.Key))
            {
                sorted.Add(n.Value);
            }
            Dictionary<int, ChatRoom> rums;
            lock (rooms)
            {
                rums = new Dictionary<int, ChatRoom>(rooms);
            }

            foreach(MessageTemplate n in sorted)
            {
                if(n==null) { continue; }

                //ファイルに書き込み
                if (rums[n.room].messagesCollection == null) { continue; }
                lock (rooms)
                {
                    rooms[n.room].messagesCollection.InsertOne(n);
                }

                //using (StreamWriter sw = new StreamWriter(rooms[mt.room].roomPath + "/messages.txt", true, enc))
                //{
                //    sw.WriteLine(n);
                //}

            }
            //メッセージファイルから取得用メッセージリストにメッセージを読み込む
            //LoadMessages();
        }
        //部屋を追加
        private static void AddRooms()
        {
            ArrayList list;
            lock (addRooms)
            {
                list = new ArrayList(addRooms);
                addRooms.Clear();
            }
            if (list.Count <= 0) { return; }
            lock (isMessageChangedLockObj)
            {
                isMessageChanged = true;
            }
            Dictionary<int, ChatRoom> crs;
            lock (rooms)
            {
                crs = new Dictionary<int, ChatRoom>(rooms);
            }
            int keyMax;
            if(crs.Count <= 0)
            {
                keyMax = 0;
            }
            else
            {
                keyMax = crs.Keys.Max();
            }

            //foreach(ChatRoom r in list)
            //{
            //    //collectionをdbに追加
            //}

            //roomsに部屋を追加
            //roomsをdbに追加
            foreach(ChatRoom r in list)
            {
                lock (db)
                {
                    try
                    {
                        db.CreateCollection(MESSAGE_COLLECTIN_NAME_BASE + r.room_id.ToString());
                    }
                    catch
                    {
                        lock (rooms)
                        {
                            if (rooms.ContainsKey(r.room_id))
                            {
                                rooms[r.room_id].hidden = false;
                                rooms[r.room_id].messagesCollection?.DeleteMany(Builders<MessageTemplate>.Filter.Empty);
                            }
                            lock(roomsCollection)
                            {
                                roomsCollection.UpdateOne(Builders<ChatRoom>.Filter.Eq("room_id", r.room_id), Builders<ChatRoom>.Update.Set("hidden", false));
                            }
                            lock (messages)
                            {
                                messages[r.room_id].Clear();
                            }
                        }
                    }
                }
                ChatRoom cr = new ChatRoom();
                cr.next_id = 0;
                cr.room_id = r.room_id;
                cr.name = r.name;
                cr.hidden = false;
                cr.creator = r.creator;
                cr.whiteList = new List<string>(r.whiteList);
                cr.tags = new List<string>(r.tags);
                cr.isPrivate = r.isPrivate;
                lock (db)
                {
                    cr.messagesCollection = db.GetCollection<MessageTemplate>(MESSAGE_COLLECTIN_NAME_BASE + cr.room_id.ToString());
                }
                lock(roomsCollection)
                {
                    roomsCollection.InsertOne(cr);
                }
                lock (rooms)
                {
                    rooms.Add(cr.room_id, cr);
                }
            }

            //using (StreamWriter sw = new StreamWriter(chatRoomFilePath, true, enc))
            //{
            //    foreach (ChatRoom s in list)
            //    {
            //        bool skip = false;
            //        foreach(ChatRoom s2 in rooms)
            //        {
            //            if(s2.name == s.name) { skip = true; break; }
            //        }
            //        if (skip) { continue; }
            //        string json = JsonSerializer.Serialize(s);
            //        sw.WriteLine(json);
            //        keyMax++;
            //        rooms.Add(s);
            //        //部屋のディレクトリ追加
            //        Directory.CreateDirectory(s.roomPath);
            //        //部屋のメッセージ保存用テキストファイル追加
            //        using (FileStream fs = File.Create(s.roomPath + "/messages.txt")) ;
            //    }
            //}
            //LoadMessages();
        }

        private static void LockedHideRoom()
        {
            List<int> removeList = new List<int>();
            lock (removeRoomList)
            {
                if (removeRoomList.Count <= 0) { return; }
                removeList = new List<int>(removeRoomList);
                removeRoomList.Clear();
            }
            lock(isMessageChangedLockObj)
            {
                isMessageChanged = true;
            }
            foreach (int a in removeList)
            {   
                string guid = Guid.NewGuid().ToString();
                string oldName;
                //roomsから部屋を削除
                lock (rooms)
                {
                    if(rooms.Count <= a || a < 0) { continue; }
                    oldName = rooms[a].name + "_old_" + guid;
                    rooms[a].hidden = true;
                    rooms[a].name = oldName;
                }
                //dbから部屋を削除
                lock (roomsCollection)
                {
                    roomsCollection.UpdateOne(Builders<ChatRoom>.Filter.Eq("room_id", a), Builders<ChatRoom>.Update.Set("hidden", true));
                    roomsCollection.UpdateOne(Builders<ChatRoom>.Filter.Eq("room_id", a), Builders<ChatRoom>.Update.Set("name", oldName));
                }

                //string path = "chatrooms.txt";
                //string[] arrLine = File.ReadAllLines(path);
                //ChatRoom? mt = JsonSerializer.Deserialize<ChatRoom>(arrLine[a]);
                //if (mt == null) { continue; }
                //mt.hidden = true;
                //lock (rooms)
                //{
                //    rooms[a] = mt;
                //}
                //arrLine[a] = JsonSerializer.Serialize(mt);
                //File.WriteAllLines(path, arrLine);
            }
            //LoadMessages();
        }

        private static void LockedPatchRoom()
        {
            Dictionary<int, ChatRoom> patchList = new Dictionary<int, ChatRoom>();
            lock (patchRoomList)
            {
                if (patchRoomList.Count <= 0) { return; }
                patchList = new Dictionary<int, ChatRoom>(patchRoomList);
                patchRoomList.Clear();
            }
            lock (isMessageChangedLockObj)
            {
                isMessageChanged = true;
            }
            foreach (int key in patchList.Keys)
            {
                ChatRoom cr = patchList[key];
                lock (rooms)
                {
                    rooms[key].name = cr.name;
                    rooms[key].isPrivate = cr.isPrivate;
                    rooms[key].whiteList = cr.whiteList;
                    rooms[key].tags = cr.tags;
                }

                lock(roomsCollection)
                {
                    roomsCollection.UpdateOne(Builders<ChatRoom>.Filter.Eq("room_id", key), Builders<ChatRoom>.Update.Set("name", cr.name));
                    roomsCollection.UpdateOne(Builders<ChatRoom>.Filter.Eq("room_id", key), Builders<ChatRoom>.Update.Set("isPrivate", cr.isPrivate));
                    roomsCollection.UpdateOne(Builders<ChatRoom>.Filter.Eq("room_id", key), Builders<ChatRoom>.Update.Set("whiteList", cr.whiteList));
                    roomsCollection.UpdateOne(Builders<ChatRoom>.Filter.Eq("room_id", key), Builders<ChatRoom>.Update.Set("tags", cr.tags));
                }
            }
        }

        private static void LockedHideMessage()
        {
            Dictionary<int, List<int>> al;
            lock (removeDic)
            {
                if (removeDic.Count <= 0) { return; }
                al = new Dictionary<int, List<int>>(removeDic);
                removeDic.Clear();
            }
            lock (isMessageChangedLockObj)
            {
                isMessageChanged = true;
            }

            foreach (int a in al.Keys)
            {
                int rNum;
                lock (rooms)
                {
                    rNum = rooms[a].room_id;
                }
                foreach(int b in al[a])
                {
                    //メッセージを隠す
                    int mNum = b;
                    lock(messages)
                    {
                        messages[rNum][mNum].hidden = true;
                    }
                    //dbのコレクションのメッセージを隠す
                    lock (rooms)
                    {
                        rooms[rNum].messagesCollection.UpdateOne(Builders<MessageTemplate>.Filter.Eq("message_id", mNum), Builders<MessageTemplate>.Update.Set("hidden", true));
                    }

                    //string path = rooms[a].roomPath + "/messages.txt";
                    //string[] arrLine = File.ReadAllLines(path);
                    //if (arrLine.Length <= b || b < 0) { continue; }
                    //MessageTemplate? mt = JsonSerializer.Deserialize<MessageTemplate>(arrLine[b]);
                    //if (mt == null) { continue; }
                    //mt.hidden = true;
                    //arrLine[b] = JsonSerializer.Serialize(mt);
                    //File.WriteAllLines(path, arrLine);
                }
            }
            //LoadMessages();
        }

        private static void LockedChangeMessage()
        {
            Dictionary<int, Dictionary<int, string>> dic;
            lock(changeDic)
            {
                if(changeDic.Count <= 0) { return; }
                dic = new Dictionary<int, Dictionary<int, string>>(changeDic);
                changeDic.Clear();
            }
            lock (isMessageChangedLockObj)
            {
                isMessageChanged = true;
            }

            foreach (int a in dic.Keys)
            {
                int rNum;
                lock (rooms)
                {
                    rNum = rooms[a].room_id;
                }
                foreach (int b in dic[a].Keys)
                {
                    //messagesのメッセージを変更する
                    int mNum = b;
                    lock (messages)
                    {
                        messages[rNum][mNum].message = dic[a][b];
                        messages[rNum][mNum].edited = true;
                    }
                    //dbのコレクションのメッセージを変更する
                    lock (rooms)
                    {
                        rooms[rNum].messagesCollection.UpdateOne(Builders<MessageTemplate>.Filter.Eq("message_id", mNum), Builders<MessageTemplate>.Update.Set("edited", true));
                        rooms[rNum].messagesCollection.UpdateOne(Builders<MessageTemplate>.Filter.Eq("message_id", mNum), Builders<MessageTemplate>.Update.Set("message", dic[a][b]));
                    }

                    //string path = rooms[a].roomPath + "/messages.txt";
                    //string[] arrLine = File.ReadAllLines(path);
                    //MessageTemplate? mt = JsonSerializer.Deserialize<MessageTemplate>(arrLine[b]);
                    //if (mt == null) { continue; }
                    //mt.message = dic[a][b];
                    //mt.edited = true;
                    //arrLine[b] = JsonSerializer.Serialize(mt);
                    //File.WriteAllLines(path, arrLine);


                }
            }
            //LoadMessages();
        }
        //メッセージコレクションから取得用リストにメッセージを読み込む　部屋ごとに分けてある
        private static void LoadMessages()
        {
            lock (isMessageChangedLockObj)
            {
                if(!isMessageChanged) { return; }
                isMessageChanged = false;
            }
            Dictionary<int, List<MessageTemplate>> msgListList = new Dictionary<int, List<MessageTemplate>>();
            Dictionary<int, ChatRoom> crs;
            lock (rooms)
            {
                crs = new Dictionary<int, ChatRoom>(rooms);
            }
            foreach(ChatRoom room in crs.Values)
            {
                List<MessageTemplate> mts = new List<MessageTemplate>();
                lock(rooms)
                {
                    mts = rooms[room.room_id].messagesCollection.Find(Builders<MessageTemplate>.Filter.Empty).ToList();
                }
                msgListList.Add(room.room_id, mts);

                //using (TextReader streamReader = new StreamReader(room.roomPath + "/messages.txt", enc))
                //using (TextReader safeSr = TextReader.Synchronized(streamReader))
                //{
                //    string? m = null;
                //    List<MessageTemplate> tmpList = new List<MessageTemplate>();
                //    do
                //    {
                //        m = safeSr.ReadLine();
                //        if (m == null || m == "") break;
                //        MessageTemplate? pr = JsonSerializer.Deserialize<MessageTemplate>(m);
                //        if (pr == null) break;
                //        tmpList.Add(pr);
                //    } while (m != null);
                //    msgListList.Add(tmpList);
                //}
            }
            lock(messages)
            {
                messages = new Dictionary<int, List<MessageTemplate>>(msgListList);
            }
        }
    }

    //部屋を追加する際の結果
    public class AddRoomResult
    {
        public bool status { get; set; }

        public string message { get; set; }
    }

    //メッセージのクラス
    public class MessageTemplate
    {
        [BsonId]
        ObjectId id { get; set; }
        public MessageTemplate(MessageTemplate mt)
        {
            this.message = mt.message;
            this.user = mt.user;
            this.date = mt.date;
            this.hidden = mt.hidden;
            this.message_id = mt.message_id;
            this.room = mt.room;
            this.edited = mt.edited;
        }
        public MessageTemplate() { }
        [BsonElement("message")]
        public string message { get; set; }
        [BsonElement("user")]
        public string user { get; set; }
        [BsonElement("date")]
        public DateTime date { get; set; }
        [BsonElement("hidden")]
        public bool hidden { get; set; }
        [BsonElement("message_id")]
        public int message_id { get; set; }
        [BsonElement("room")]
        public int room { get; set; }
        [BsonElement("edited")]
        public bool edited { get; set; }
        [BsonIgnore]
        public string nickname_jp { get; set; }
        [BsonIgnore]
        public string nickname_en { get; set; }

    }

    //チャット部屋のクラス
    public class ChatRoom
    {
        public ChatRoom(ChatRoom cr)
        {
            this.name = cr.name;
            this.room_id = cr.room_id;
            this.hidden = cr.hidden;
            this.creator = cr.creator;
            this.next_id = cr.next_id;
            this.messagesCollection = null;
            this.isPrivate = cr.isPrivate;
            this.whiteList = new List<string>(cr.whiteList);
            this.tags = new List<string>(cr.tags);
        }
        public ChatRoom() { }

        [BsonId]
        ObjectId _id { get; set; }

        [BsonElement("name")]
        public string name { get; set; }

        [BsonElement("room_id")]
        public int room_id { get; set; }

        [BsonElement("hidden")]
        public bool hidden { get; set; }

        //[BsonElement("room_path")]
        //public string roomPath { get; set; }

        [BsonElement("creator")]
        public string creator { get; set; }

        [BsonElement("tags")]
        public List<string> tags { get; set; }

        [BsonElement("isPrivate")]
        public bool isPrivate { get; set; }

        [BsonElement("whiteList")]
        public List<string> whiteList { get; set; }

        [BsonIgnore]
        internal int next_id { get; set; }

        //[BsonIgnore]
        //public List<MessageTemplate>? messages { get; set; }

        [BsonIgnore]
        internal IMongoCollection<MessageTemplate>? messagesCollection { get; set; }
    }

    public class GetRoomsResult
    {
        public List<ChatRoom>? result { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
    }

    public class ReadMessageResult
    {
        public List<MessageTemplate>? result { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
    }

    public class WriteMessageResult
    {
        public MessageTemplate? result { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
    }
    public class HideMessageResult
    {
        public bool status { get; set; }
        public string message { get; set; }
    }
    public class ChangeMessageResult
    {
        public bool status { get; set; }
        public string message { get; set; }
    }
    public class DeleteRoomResult
    {
        public bool status { get; set; }
        public string message { get; set; }

    }
    public class PatchRoomResult
    {
        public bool status { get; set; }
        public string message { get; set; }
        public ChatRoom? result { get; set; }
    }
}
