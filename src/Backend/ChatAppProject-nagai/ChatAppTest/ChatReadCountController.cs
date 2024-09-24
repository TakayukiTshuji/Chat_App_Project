using ChatAppTest.FunctionController.Chat;
using ChatAppTest.FunctionController.Session;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Timers;

namespace ChatAppTest
{
    public class ChatReadCountController
    {
        public const string DB_NAME = "test";
        public const string READ_COUNT_COLLECTION = "chatAppReadCount";

        public static MongoClient client = new MongoClient("mongodb://localhost");
        public static IMongoDatabase db = client.GetDatabase(DB_NAME);
        public static IMongoCollection<ReadCounter> readCountCollection = db.GetCollection<ReadCounter>(READ_COUNT_COLLECTION);

        private static Dictionary<int, ReadCounter> readCount = new Dictionary<int, ReadCounter>();
        private static Dictionary<int, Dictionary<int, List<string>>> addReadCount = new Dictionary<int, Dictionary<int, List<string>>>();

        private static bool isWriting = false;
        private static object isWritingLockObj = new object();
        private static bool isChanged = false;
        private static object isChangedLockObj = new object();
        private static System.Timers.Timer timer = new System.Timers.Timer();


        public static void Initialize()
        {
            lock(isWritingLockObj)
            {
                isWriting = true;
            }
            timer.Interval = 100;
            timer.Elapsed += StartWriting;
            timer.AutoReset = true;
            timer.Enabled = true;

            try
            {
                db.CreateCollection(READ_COUNT_COLLECTION);
            }
            catch
            {

            }

            List<ReadCounter> readCounterListTemp = new List<ReadCounter>(readCountCollection.Find<ReadCounter>(Builders<ReadCounter>.Filter.Empty).ToList());
            lock (readCount)
            {
                foreach(ReadCounter rc in readCounterListTemp)
                {
                    readCount.Add(rc.roomId, rc);
                }
            }

            lock (isWritingLockObj)
            {
                isWriting = false;
            }
        }

        public static ReadCounterGetResult GetCounter(string sessionId, int roomId)
        {
            ReadCounterGetResult result = new ReadCounterGetResult();
            result.status = false;
            result.message = "謎失敗";
            result.result = null;

            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if (!asr.status)
            {
                result.message = asr.message;
                return result;
            }
            if (asr.result == null)
            {
                result.message = "謎失敗２";
                return result;
            }

            string userId = asr.result.userId;

            lock(readCount)
            {
                if(readCount.ContainsKey(roomId))
                {
                    result.message = "成功";
                    result.status = true;
                    result.result = readCount[roomId];
                    return result;
                }
                else
                {
                    result.message = "既読が一つもありません";
                    result.status = false;
                    result.result = null;
                    return result;
                }
            }
        }

        public static ReadCounterPostResult PostCounter(string sessionId, int roomId, int messageId)
        {
            ReadCounterPostResult result = new ReadCounterPostResult();
            result.status = false;
            result.message = "謎失敗";

            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if (!asr.status)
            {
                result.message = asr.message;
                return result;
            }
            if (asr.result == null)
            {
                result.message = "謎失敗２";
                return result;
            }

            string userId = asr.result.userId;

            lock(addReadCount)
            {
                //if spaghetti
                if (addReadCount.ContainsKey(roomId))
                {
                    if(addReadCount[roomId].ContainsKey(messageId))
                    {
                        if (!addReadCount[roomId][messageId].Contains(userId))
                        {
                            addReadCount[roomId][messageId].Add(userId);
                        }
                    }
                    else
                    {
                        addReadCount[roomId].Add(messageId, new List<string>());
                        addReadCount[roomId][messageId].Add(userId);
                    }
                }
                else
                {
                    addReadCount.Add(roomId, new Dictionary<int, List<string>>());
                    addReadCount[roomId].Add(messageId, new List<string>());
                    addReadCount[roomId][messageId].Add(userId);
                }
            }
            lock (isChangedLockObj)
            {
                isChanged = true;
            }

            result.message = "成功";
            result.status = true;
            return result;
        }

        public static void StartWriting(Object source, ElapsedEventArgs e)
        {
            lock (isWritingLockObj)
            {
                if(isWriting) { return; }
                isWriting = true;
            }

            LockedWriteReadCount();

            lock (isWritingLockObj)
            {
                isWriting = false;
            }
        }

        private static void LockedWriteReadCount()
        {
            lock (isChangedLockObj)
            {
                if (!isChanged)
                {
                    return;
                }
                isChanged = false;
            }
            Dictionary<int, Dictionary<int, List<string>>> addReadCountTemp;
            lock (addReadCount)
            {
                addReadCountTemp = new Dictionary<int, Dictionary<int, List<string>>>(addReadCount);
                addReadCount.Clear();
            }

            foreach(int i in addReadCountTemp.Keys)
            {
                ReadCounter rc;
                lock (readCount)
                {
                    if (readCount.ContainsKey(i))
                    {
                        rc = new ReadCounter(readCount[i]);
                    }
                    else
                    {
                        rc = new ReadCounter();
                        rc.roomId = i;
                        readCountCollection.InsertOne(rc);
                    }
                }
                foreach(int j in addReadCountTemp[i].Keys)
                {
                    foreach(string user in addReadCountTemp[i][j])
                    {
                        if(rc.counts.ContainsKey(j))
                        {
                            if (!rc.counts[j].Contains(user))
                            {
                                rc.counts[j].Add(user);
                            }
                        }
                        else
                        {
                            rc.counts.Add(j, new List<string>());
                            rc.counts[j].Add(user);
                        }
                    }
                }
                lock(readCount)
                {
                    readCount[i] = new ReadCounter(rc);
                }
                readCountCollection.UpdateOne(Builders<ReadCounter>.Filter.Eq("room_id", i), Builders<ReadCounter>.Update.Set("counts", rc.counts));
            }
        }
    }

    public class ReadCounter
    {
        [BsonId]
        public ObjectId _id;
        [BsonElement("room_id")]
        public int roomId { get; set; }
        [BsonElement("counts")]
        [BsonDictionaryOptions(Representation = DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, List<string>> counts { get; set; }
        public ReadCounter(ReadCounter readCounter)
        {
            roomId = readCounter.roomId;
            counts = new Dictionary<int, List<string>>(readCounter.counts);
        }
        public ReadCounter()
        {
            counts = new Dictionary<int, List<string>>();
        }
    }

    public class ReadCounterGetResult
    {
        public string message { get; set; }
        public bool status { get; set; }
        public ReadCounter? result { get; set; }
    }
    public class ReadCounterPostResult
    {
        public string message { get; set; }
        public bool status { get; set; }
    }
}
