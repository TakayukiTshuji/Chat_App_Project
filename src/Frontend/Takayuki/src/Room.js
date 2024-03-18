import React, {useState,useRef, useEffect}from 'react';
//import MessageApp from './MessageApp';
import { useCookies } from 'react-cookie';
import { useNavigate } from 'react-router-dom';


const Room = () => {
    const messagesEndRef = useRef(null);
    const [room,setroom]=useState([]);
    const [unionRoom,setunionRoom]=useState([]);
    const [cookies,setCookie,removeCookie]=useCookies(['session_id']);
    const navigate = useNavigate();

    let roomNameID=[];


    const scrollToBottom = () => {
        if (messagesEndRef.current) {  // messagesEndRef.current が null でないことを確認
            messagesEndRef.current.scrollIntoView({ behavior: "smooth", block: "end" });
        }
    };

    useEffect(()=>{
        setTimeout(()=>{
            getRoom();
        },5000);
        //テスト用
    },[])

    const getRoom=()=>{
        fetch(`https://localhost:7038/api/ChatRoomCtl`,{
            credentials:'include',
            headers:{
                'Content-Type':'application/json',
            }
        })
        .then((response)=>{
            return response.json();
        })
        .then((data)=>{
            console.log("successRoomData:",data);
            const roomName=data.result.map(e=>e.name);
            roomNameID=data.result.map(e=>e.room_id);
            //console.log(roomNameID);
            setroom([...roomName]);
            setunionRoom([...roomName]);
        })
        .catch((error)=>{console.log("error:",error)})
        .finally(()=>scrollToBottom());
    }

    const handleRoomClick=(roomid)=>{
        const Romnum=unionRoom.indexOf(roomid);
        if((Romnum!==-1)){
            console.log("成功")
        }else{
            console.log("ありませんでした。");
        }
        
        //roomidは名前だけ取っているので、これがどこの番号か知りたい
        console.log(roomid);
        navigate('/chat',{state:{name:Romnum}});
    }
    const changeAddRom=()=>{
        navigate('/AddGroup');
    }
    const Logout=()=>{
        removeCookie('sessionId');
        navigate('/');
      }
    
    return (
        <div>
            <h1>Room</h1>
            <button onClick={Logout}>ログアウト</button>
            <button onClick={changeAddRom}>追加</button>
            <button onClick={getRoom}>更新</button>
            <div>
            {room.map((data, index) => (
                <button key={index} onClick={() => handleRoomClick(data)}>{data}</button>
            ))}
            </div>
        </div>
    )
}

export default Room