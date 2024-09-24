using ChatAppTest.FunctionController.Session;
using ChatAppTest.FunctionController.User;
using MongoDB.Driver;
using Swashbuckle.AspNetCore.Swagger;

namespace ChatAppTest
{
    public class ChatWritingController
    {
        private static object isWritingLockObj = new object();
        private static bool isWriting = false;

        private static Dictionary<int, Dictionary<string, DateTime?>> writingDic = new Dictionary<int, Dictionary<string, DateTime?>>();

        public static WritingStartResult StartWriting(string sessionId, int roomId)
        {
            WritingStartResult wsr = new WritingStartResult();
            wsr.status = false;
            wsr.message = "謎失敗";

            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if (!asr.status)
            {
                wsr.message = asr.message;
                return wsr;
            }
            if (asr.result == null)
            {
                wsr.message = "謎失敗２";
                return wsr;
            }

            lock(writingDic)
            {
                if(writingDic.ContainsKey(roomId))
                {
                    if (writingDic[roomId].ContainsKey(asr.result.userId))
                    {
                        writingDic[roomId][asr.result.userId] = new DateTime(DateTime.Now.Ticks);
                    }
                    else
                    {
                        writingDic[roomId].Add(asr.result.userId, new DateTime(DateTime.Now.Ticks));
                    }
                }
                else
                {
                    writingDic.Add(roomId, new Dictionary<string, DateTime?>());
                    if (writingDic[roomId].ContainsKey(asr.result.userId))
                    {
                        writingDic[roomId][asr.result.userId] = new DateTime(DateTime.Now.Ticks);
                    }
                    else
                    {
                        writingDic[roomId].Add(asr.result.userId, new DateTime(DateTime.Now.Ticks));
                    }
                }
            }

            wsr.status = true;
            wsr.message = "成功";
            return wsr;
        }

        public static WritingStopResult StopWriting(string sessionId, int roomId)
        {
            WritingStopResult wsr = new WritingStopResult();
            wsr.status = false;
            wsr.message = "謎失敗";

            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if (!asr.status)
            {
                wsr.message = asr.message;
                return wsr;
            }
            if (asr.result == null)
            {
                wsr.message = "謎失敗２";
                return wsr;
            }

            lock (writingDic)
            {
                if (writingDic.ContainsKey(roomId))
                {
                    if (writingDic[roomId].ContainsKey(asr.result.userId))
                    {
                        writingDic[roomId][asr.result.userId] = null;
                    }
                    else
                    {
                        wsr.message = "あなたはその部屋には書き込んでいません";
                        return wsr;
                    }
                }
                else
                {
                    wsr.message = "あなたはその部屋には書き込んでいません";
                    return wsr;
                }
            }

            wsr.status = true;
            wsr.message = "成功";
            return wsr;

        }

        public static WritingGetResult GetWriting(string sessionId, int roomId)
        {
            WritingGetResult wgr = new WritingGetResult();
            wgr.status = false;
            wgr.message = "謎失敗";
            wgr.result = new List<string>();

            AuthSessionResult asr = ChatSessionController.AuthSession(sessionId);
            if (!asr.status)
            {
                wgr.message = asr.message;
                return wgr;
            }
            if (asr.result == null)
            {
                wgr.message = "謎失敗２";
                return wgr;
            }

            lock (writingDic)
            {
                if(writingDic.ContainsKey(roomId))
                {
                    if (writingDic[roomId].Count > 0)
                    {
                        wgr.result = new List<string>(writingDic[roomId].Keys);
                        foreach(string key in writingDic[roomId].Keys)
                        {
                            if (writingDic[roomId][key] == null)
                            {
                                wgr.result.Remove(key);
                                continue;
                            }
                            DateTime now = new DateTime(DateTime.Now.Ticks);
                            DateTime then = (DateTime)writingDic[roomId][key];
                            TimeSpan ts = now - then;
                            if (ts > TimeSpan.FromSeconds(30))
                            {
                                wgr.result.Remove(key);
                                continue;
                            }
                        }
                        if(wgr.result.Count <= 0)
                        {
                            wgr.message = "誰も書き込んでいません";
                            return wgr;
                        }
                        wgr.status = true;
                        wgr.message = "成功";
                        return wgr;
                    }
                    else
                    {
                        wgr.message = "誰も書き込んでいません";
                        return wgr;
                    }
                }
                else
                {
                    wgr.message = "誰も書き込んでいません";
                    return wgr;
                }
            }
        }
    }

    public class WritingStartResult
    {
        public bool status { get; set; }
        public string message { get; set; }
    }
    public class WritingStopResult
    {
        public bool status { get; set; }
        public string message { get; set; }
    }
    public class WritingGetResult
    {
        public bool status { get; set; }
        public string message { get; set; }
        public List<string>? result { get; set; }
    }
}
