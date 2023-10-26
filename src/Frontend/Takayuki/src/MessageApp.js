import React, { useState, useEffect } from "react";
import "./Message.css";


const MessageApp = ({activeMember}) => {
  const [messages, setMessages] = useState([]);
  const [inputValue, setInputValue] = useState("");
  const [sender, setSender] = useState("Me");

  useEffect(() => {
    fetch("https://localhost:7038/api/ChatCtl")
      .then((response) => response.json())
      .then((data) => setMessages(data))
      .catch((error) => console.error("Error fetching messages:", error));
  }, []);

  const handleMessageSend = () => {
    if (inputValue.trim() !== "") {
      const newMessage = {
        text: inputValue,
        sender: sender,
      };
      fetch("https://localhost:7038/api/ChatCtl", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(newMessage),
      })
        .then((response) => response.json())
        .then((data) => {
          setMessages([...messages, data]);
          setInputValue("");
          if (sender === "Me") {
            setSender("You");
          } else {
            setSender("Me");
          }
        })
        .catch((error) => console.error("Error sending message:", error));
    }
  };

  return (
    <div>
      <div className="message-container">
        {messages.map((message, index) => (
          <div key={index} className="message">
            <strong>{message.sender}: </strong> {message.text}
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
        <button onClick={handleMessageSend} className="send-button">Send</button>
      </div>
    </div>
  );
};

export default MessageApp;
