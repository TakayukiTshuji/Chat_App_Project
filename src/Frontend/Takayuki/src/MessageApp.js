import React, { useState } from "react";
import "./Message.css";

function setCookie(name, value, days) {
  const expirationDate = new Date();
  expirationDate.setDate(expirationDate.getDate() + days);
  const cookie = `${name}=${value}; expires=${expirationDate.toUTCString()}; path=/`;
  document.cookie = cookie;
}

const MessageApp = () => {
  const [messages, setMessages] = useState([]);
  const [inputValue, setInputValue] = useState("");
  const [sender, setSender] = useState("Me");

  const setSessionCookie = (sessionId) => {
    setCookie("session_id", sessionId, 7); // 有効期限は7日間
  };
  const handleMessageSend = () => {
    const newMessage = {
      "message":inputValue,
      "room":0
    };
    /*axios.post(`https://localhost:7038/api/ChatCtl?message=${inputValue}&room=0`,newMessage,{
      headers:{
        "Content-Type": "application/json",
        "Accept": "application/json",
        "Cookie": "session_id=<5340326a-d1ff-4ee2-80c5-24d5214ae8a6>",
      },
    })*/
    fetch(`https://localhost:7038/api/ChatCtl?message=${inputValue}&room=0`, {
      method: 'POST',
      credentials:'include',//sameoriginはクロスリジンには送信しない。
      //mode:'cors',
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
      const extractedMessage = data.result.message;
      setMessages([...messages,extractedMessage]);
      setInputValue("");
      setSender("Me");
      console.table(data);
    })
    .catch((error) => {console.error("Error sending message:", error)});
  };
  return (
    <div>
      <div className="message-container">
        {messages.map((message,index) => (
            <div key={index} className="message">
              <strong>{sender}: </strong> <p>{message}</p>
            </div>
        ))}
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
