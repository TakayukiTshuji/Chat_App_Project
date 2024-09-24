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
  const [messageSent,setmessageSent] = useState(false);
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

  //-----------
  //画面遷移を行う関数
  //
  const Logout=()=>{
    removeCookie('sessionId');
    navigate('/');
  }
  const BackRoom = () =>{
    navigate('/Room');
  }
  //-----------
  
  //--------
  /*GETを行う*/
  //--------
  const getMessages = async()=>{
    try{
      const response = await fetch(`https://localhost:7038/api/ChatCtl?room=${roomName}`,{
        method:'GET',
        credentials: 'include',
      });
      const data = await response.json();
      console.log("Response data:", data);
      const stermessa=data.result.map(e=>({
        message:e.message,
        user:e.user
      }));//resultの中のメッセージを取り出し
      setUserMessages([...stermessa]);//メッセージを表示するために上の変数をここに入れる。
    }
    catch(error){
      console.log("Error feth",error);
    }
    finally{
      scrollToBottom();
    }
  };

  //----------
  /*POSTを行う*/
  //----------
  const handleMessageSend = async() => {
    try{
      const response = await fetch(`https://localhost:7038/api/ChatCtl?message=${inputValue}&room=${roomName}`, {
        method: 'POST',
        credentials:'include',//sameoriginはクロスリジンには送信しない。
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json",
        },      
      });
      const data = await response.json();
      //console.log("data:",data);
      //const userName=data.result.user;
      setInputValue("");
      setmessageSent(true);
      //setSender(...sender,userName);
      await getMessages();
    }
    catch(error){
      console.error("Error sending message:", error);
    }
  };

  //
  //レンダリングを行う
  //

  useEffect(() => {
    if(messageSent){
      getMessages();
      setmessageSent(false);
    }
  },[messageSent]);

  useEffect(() => {
    getMessages();
  },[])

  useEffect(() => {
    scrollToBottom();
  },[usermessages]);

  
  //
  //表示させる
  //
  return (
    <div>
      <div className="HeaderMessage">
        <h1 className="ChatName">Chat</h1>
        <button onClick={BackRoom} className="ChatBack">戻る</button>
        <button onClick={getMessages} className="Chatget">更新</button>
        <button onClick={Logout} className="ChatLogout">ログアウト</button>
      </div>

      <div className="message-container">
        {usermessages.map((data,index)=>(
              <div key={index} className="message">
                <strong>{data.user}</strong> <p>{data.message}</p>
             </div>
        ))}
        <div ref={messagesEndRef} />
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