import React, { useState,useEffect,useRef } from "react";
import { useNavigate } from "react-router-dom";
import { useCookies } from "react-cookie";
import "./Message.css";

//--------
/*GETを行う*/
//--------
const MessageApp = ({room}) => {
  const [usermessages, setUserMessages] = useState([]);
  const [inputValue, setInputValue]     = useState("");
  const [sender, setSender]             = useState([]);
  const messagesEndRef = useRef(null);
  const navigate = useNavigate();
  const [cookies,setCookie,removeCookie]=useCookies();

  const scrollToBottom = () => {
    if (messagesEndRef.current) {  // messagesEndRef.current が null でないことを確認
      messagesEndRef.current.scrollIntoView({ behavior: "smooth", block: "end" });
    }
  };

  const getMessages= ()=>{
    fetch(`https://localhost:7038/api/ChatCtl?room=${room}`,{
      method:'GET',
      credentials: 'include',
    })
    .then((response)=>{
      return response.json();
    })
    .then((data)=>{
      console.log("Response data:", data);//ここでクッキーが存在しないと表示される
      const stermessa=data.result.map(e=>e.message);//resultの中のメッセージを取り出し
      setUserMessages([...stermessa]);//メッセージを表示するために上の変数をここに入れる。
    })
    .catch((error)=>{console.log("Error feth",error)})
    .finally(() => scrollToBottom());
  };

  
  //
  //レンダリングを行う
  //
  useEffect(() => {

    //テスト用
    fetch('https://localhost:7038/api/ChatSessionCtl',{
      mode:'cors',
      credentials:'include',
      headers:{
        'Content-Type':'application/json'
      }
    })
    .then((response)=>{
      return response.json();
    })
    .then((data)=>{
      console.log('sck',data);
      //setCookie('session_id',data.result.sessionId);
    })
    //const intervalId = setInterval(() => {
    //  getMessages();
    //}, 10000);
  
    // 初回実行時にもgetMessagesを呼び出す
    //getMessages();
  
    // コンポーネントがアンマウントされたときにクリーンアップ
    //return () => clearInterval(intervalId);
  },[]);

  //----------
  /*POSTを行う*/
  //----------
  const handleMessageSend = () => {
    fetch(`https://localhost:7038/api/ChatCtl?message=${inputValue}&room=${room}`, {
      method: 'POST',
      credentials:'include',//sameoriginはクロスリジンには送信しない。
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json",
        //"Cookie": "session_id=<65cc897e-6225-4e14-b788-fdf7d2da24a2>",
        //'Access-Control-Allow-Origin': 'http://localhost:7038',
        // 'Access-Control-Request-Method': 'POST',
        //'Access-Control-Allow-Credentials':'true',
      },      
    })
    .then((response) => {
      // サーバーからのレスポンスヘッダーに含まれるCookieを取得
      //const sessionCookie = response.headers.get('session_id');
      //if (sessionCookie) {
        // クッキーが存在する場合、セッションクッキーを設定
      //  setSessionCookie(sessionCookie);
      //}
      return response.json();
    })
    .then((data) => {
      console.log("data:",data);
      //const userName=data.result.user;
      setInputValue("");
      //setSender(...sender,userName);
    })
    .catch((error) => {console.error("Error sending message:", error)})
  };

  const Logout=()=>{
    removeCookie('sessionId');
    navigate('/');
  }

  
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
      <div>
        <button onClick={getMessages}>更新</button>
        <button onClick={Logout}>ログアウト</button>
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