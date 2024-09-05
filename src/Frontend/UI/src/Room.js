//Reactフレームワーク
import React, {useState,useRef, useEffect}from 'react';
//パッケージ
import { useCookies } from 'react-cookie';
import { json, useNavigate } from 'react-router-dom';
//外部フレームワーク
import { motion,AnimatePresence } from 'framer-motion';
//CSS
import './Room.css'

const Room = () => {
    const messagesEndRef = useRef(null);
    const [room,setroom]=useState([]);
    const [unionRoom,setunionRoom]=useState([]);
    const [cookies,setCookie,removeCookie]=useCookies(['session_id']);
    const navigate = useNavigate();
    let roomNameID=[];


    const scrollToBottom = () => {
        if (messagesEndRef.current) {
            messagesEndRef.current.scrollIntoView({ behavior: "smooth", block: "end" });
        }
    };

    useEffect(()=>{
        getRoom();
        setTimeout(()=>{
            getRoom();
        },5000);
    },[])

    const getRoom=async()=>{
        await fetch(`https://localhost:7038/api/ChatRoomCtl`,{
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
    //Delete処理（恐らくバックエンドでの権限が必要、現時点では実装が難しい）
    /*const DeleteRoom=()=>{

        fetch("https://localhost:7038/api/ChatRoomCtl/Delete",{
            method:'DELETE',
            credentials:'include',
            headers:{
                'Content-Type':'application/json'
            }
        })
        .then((response)=>{
            console.log("削除",response);
            return json(response);
        })


    }*/
    
    return (
        <div>
            <div className='HeaderRoom'>
                <h1>Room</h1>
                <div className='RoomMenu'>
                <button onClick={changeAddRom} className='RoomAdd'>追加</button>
                <button onClick={getRoom} className='Roomget'>更新</button>
                <button onClick={Logout} className='RoomLogout'>ログアウト</button>
                </div>
            </div>
            
            <div className='RoomList'>
                <motion.section
                    initial={{ opacity: 0 }} // 初期状態
                    animate={{ opacity: 1 }} // マウント時
                    exit={{ opacity: 0 }}    // アンマウント時
                >
                    {room.map((data, index) => (
                        <button 
                            key={index} 
                            onClick={() => handleRoomClick(data)}
                            className='Roomid'>{data}</button>
                    ))}
                </motion.section>            
            </div>
        </div>
    )
}

export default Room