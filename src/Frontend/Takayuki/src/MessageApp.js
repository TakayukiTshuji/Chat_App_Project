import React, { useState,useEffect,useRef } from "react";
import "./Message.css";

function setCookie(name, value, days) {
  const expirationDate = new Date();
  expirationDate.setDate(expirationDate.getDate() + days);
  const cookie = `${name}=${value}; expires=${expirationDate.toUTCString()}; path=/`;
  document.cookie = cookie;
}

//--------
/*GETを行う*/
//--------
const MessageApp = () => {
  const [usermessages, setUserMessages] = useState([]);
  const [inputValue, setInputValue]     = useState("");
  const [sender, setSender]             = useState([]);
  const messagesEndRef = useRef(null);

  const scrollToBottom = () => {
    if (messagesEndRef.current) {  // messagesEndRef.current が null でないことを確認
      messagesEndRef.current.scrollIntoView({ behavior: "smooth", block: "end" });
    }
  };

  const getMessages= ()=>{
    fetch('https://localhost:7038/api/ChatCtl?room=0',{
      credentials:'include'
    })
    .then((response)=>{
      return response.json();
    })
    .then((data)=>{
      const stermessa=data.message;
      setUserMessages([...usermessages,...stermessa]);
      if (data.status && Array.isArray(data.result)) {
        console.log("stermessa:",data); // resultの中身をログに表示
      } else {
        console.error("Invalid data format:", data);
      }
      console.log("Response data:", data);
    })
    .catch((error)=>{console.log("Error feth",error)})
    .finally(() => scrollToBottom());
  };

  
  //
  //レンダリングを行う
  //
  useEffect(() => {
    /*const intervalId = setInterval(() => {
      getMessages();
    }, 10000);*/
  
    // 初回実行時にもgetMessagesを呼び出す
    getMessages();
  
    // コンポーネントがアンマウントされたときにクリーンアップ
    //return () => clearInterval(intervalId);
  },[]);

  //
  //cookieをいれる
  //
  const setSessionCookie = (sessionId) => {
    setCookie("session_id", sessionId, 30); // 有効期限は7日間
  };

  //----------
  /*POSTを行う*/
  //----------
  const handleMessageSend = () => {
    const newMessage = {
      "message":inputValue,
      "room":0
    };

    fetch(`https://localhost:7038/api/ChatCtl?message=${inputValue}&room=0`, {
      method: 'POST',
      credentials:'include',//sameoriginはクロスリジンには送信しない。
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json",
        //"Cookie": "session_id=<65cc897e-6225-4e14-b788-fdf7d2da24a2>",
        // 'Access-Control-Allow-Origin': 'http://localhost:7038',
        // 'Access-Control-Request-Method': 'POST',
        // 'Access-Control-Allow-Credentials':'true',
      },
      body: JSON.stringify(newMessage),
      
    })
    //TODO存在しないセッションIDと言われてる
    .then((response) => {
      // サーバーからのレスポンスヘッダーに含まれるCookieを取得
      const sessionCookie = response.headers.get('session_id');
      if (sessionCookie) {
        // クッキーが存在する場合、セッションクッキーを設定
        setSessionCookie(sessionCookie);
      }
      return response.json();
    })
    .then((data) => {
      console.log("data",data);
      const userName=data.result.user;
      setInputValue("");
      setSender(...sender,userName);
    })
    .catch((error) => {console.error("Error sending message:", error)})
  };

  //
  //表示させる
  //
  return (
    <div>
      <div className="message-container">
        {usermessages.map((data,index)=>(
              <div key={index} className="message">
                <strong>{sender}</strong> <p>{data}</p>
             </div>
        ))}
        <div ref={messagesEndRef} />
      </div>
      
      <div className="input-container">
        <input
          type="text"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          className="input-text"
        />
        <button onClick={handleMessageSend} className="send-button">
          Send
        </button>        
      </div>
    </div>
  );
};

export default MessageApp;
