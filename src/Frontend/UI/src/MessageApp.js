//Reactフレームワーク
import React, { useState,useEffect,useRef } from "react";
//パッケージ
import { useNavigate } from "react-router-dom";
import { useCookies } from "react-cookie";
import { useLocation } from "react-router-dom";
//CSS
import "./Message.css";


const MessageApp = () => {
  const [usermessages, setUserMessages] = useState([]);
  const [inputValue, setInputValue]     = useState("");
  const messagesEndRef = useRef(null);
  const navigate = useNavigate();
  const [cookies,setCookie,removeCookie]=useCookies();
  const location = useLocation();
  const roomName = location.state.name;


  const scrollToBottom = () => {
    if (messagesEndRef.current) {  // messagesEndRef.current が null でないことを確認
      messagesEndRef.current.scrollIntoView({ behavior: "smooth", block: "end" });
    }
  };
  
  //--------
  /*GETを行う*/
  //--------
  const getMessages= ()=>{
    fetch(`https://localhost:7038/api/ChatCtl?room=${roomName}`,{
      method:'GET',
      credentials: 'include',
    })
    .then((response)=>{
      return response.json();
    })
    .then((data)=>{
      console.log("Response data:", data);
      const stermessa=data.result.map(e=>({
        message:e.message,
        user:e.user
      }));//resultの中のメッセージを取り出し
      setUserMessages([...stermessa]);//メッセージを表示するために上の変数をここに入れる。
    })
    .catch((error)=>{console.log("Error feth",error)})
    .finally(() => scrollToBottom());
  };

  
  //
  //レンダリングを行う
  //
  useEffect(() => {
    const timer = setInterval(()=>{
      getMessages();
    },3000);
    return ()=>{
      clearInterval(timer);
    }
  },[]);

  //----------
  /*POSTを行う*/
  //----------
  const handleMessageSend = () => {
    fetch(`https://localhost:7038/api/ChatCtl?message=${inputValue}&room=${roomName}`, {
      method: 'POST',
      credentials:'include',//sameoriginはクロスリジンには送信しない。
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json",
      },      
    })
    .then((response) => {
      return response.json();
    })
    .then((data) => {
      //console.log("data:",data);
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

  const BackRoom = () =>{
    navigate('/Room');
  }

  
  //
  //表示させる
  //
  return (
    <div>
      <div className="message-container">
        {usermessages.map((data,index)=>(
              <div key={index} className="message">
                <strong>{data.user}</strong> <p>{data.message}</p>
             </div>
        ))}
        <div ref={messagesEndRef} />
      </div>
      <div>
        <button onClick={getMessages}>更新</button>
        <button onClick={Logout}>ログアウト</button>
        <button onClick={BackRoom}>戻る</button>
      </div>
      
      <div className="input-container">
        <input
          type="text"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          placeholder="入力"
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