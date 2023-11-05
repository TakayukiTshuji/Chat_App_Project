import React, { useState } from "react";
import "./Message.css";

const MessageApp = () => {
  const [messages, setMessages] = useState([]);
  const [inputValue, setInputValue] = useState("");
  const [sender, setSender] = useState("Me");
  const [text,setText]=useState("");

  const handleMessageSend = () => {
    const newMessage = {
      text: inputValue,
      sender: sender,
    };    
    fetch(`https://localhost:7038/api/ChatCtl?message=${inputValue}&room=0`, {
      method: 'POST',
      //credentials: "include",
      mode:'cors',
      headers: {
        "Accept": "application/json",
        "Cookie": "session_id=416811df-f778-4714-bdf9-2af8bb693c6;",
        'Access-Control-Request-Method': 'POST'
      },
      body: JSON.stringify(newMessage),
    })
    .then((response) => response.json())
    .then((data) => {
      setMessages([...messages, data]);
      setInputValue("");
      setSender("Me");
    })
    .catch((error) => {console.error("Error sending message:", error)});
    setText(inputValue)
  };
  return (
    <div>
      <div className="message-container">
      {messages.map((message, index) => (
          <div key={index} className="message">
            <strong>{sender}: </strong> <p>{text}</p>
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
