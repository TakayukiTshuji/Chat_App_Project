import React, {useState,useRef, useEffect}from 'react';
import MessageApp from './MessageApp';
import { useCookies } from 'react-cookie';


const Room = () => {
    const messagesEndRef = useRef(null);
    const [room,setroom]=useState([]);
    const [selectroom,setselectroom]=useState(null);
    const [cookies,setCookie,removeCookie]=useCookies(['session_id']);
    //document.cookie=cookies;

    const scrollToBottom = () => {
        if (messagesEndRef.current) {  // messagesEndRef.current が null でないことを確認
            messagesEndRef.current.scrollIntoView({ behavior: "smooth", block: "end" });
        }
    };

    useEffect(()=>{
        //テスト用
        fetch('https://localhost:7038/api/ChatSessionCtl',{
            mode:'cors',
            credentials:'include',
            headers:{
                'Content-Type':'application/json',
                //'Cookie':docucookie
            }
        })
        .then((response)=>{
            const cookiehead=response.headers.get('Set-Cookie');
            console.log(cookiehead);
            return response.json();
        })
        .then((data)=>{
            console.log('sxk,cookie:',cookies);
            console.log('sck',data);
            //setCookie('session_id',data.result.sessionId);
        })
    })

    const getRoom=()=>{
        fetch(`https://localhost:7038/api/ChatRoomCtl`,{
            credentials:'include',
            headers:{
                'Content-Type':'application/json',
                //'Cookie':docucookie
            }
        })
        .then((response)=>{
            return response.json();
        })
        .then((data)=>{
            console.log("successRoomData:",data);
            const roomid=data.result.map(e=>e.name);
            setroom([...roomid]);
            console.log(room);
        })
        .catch((error)=>{console.log("error:",error)})
    }

    const handleRoomClick=(roomid)=>{
        setselectroom(roomid);
    }
    
    return (
        <div>
            <h1>Room</h1>
            <button onClick={getRoom}>更新</button>
            <div>
            {room.map((data, index) => (
                <button key={index} onClick={() => handleRoomClick(data)}>{data}</button>
            ))}
            </div>
            {selectroom && <MessageApp room={selectroom}/>}
            <div ref={messagesEndRef}></div>
        </div>
    )
}

export default Room