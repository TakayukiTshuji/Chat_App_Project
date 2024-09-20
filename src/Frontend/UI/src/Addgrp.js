//Reactフレームワーク
import React, { useState }from 'react'
//パッケージ
import { useNavigate } from 'react-router-dom';

import "./Addgrp.css"


const Addgrp = () => {
    const [inptext,setinptext]=useState("");
    const [trfal_P,settrfal_P]=useState("");
    const [trfal_D,settrfal_D]=useState("");
    const [usr,setusr]=useState([]);
    const [listname, setlistname] = useState("");
    const navigate=useNavigate();

    const BackRoom=()=>{
        navigate('/Room')
    }

    const handleChange=()=>{
        const textgrp={
            "grpName":inptext,
            "isPrivata":trfal_P,
            "isDm":trfal_D,
            "whitelist": usr
        };

        fetch(`https://localhost:7038/api/ChatRoomCtl?name=${textgrp.grpName}&isPrivate=${textgrp.isPrivata}&isDm=${textgrp.isDm}`,{
            method:'POST',
            credentials:'include',
            headers:{
                'Content-Type': 'application/json',
            },
            body:JSON.stringify(textgrp.whitelist)
            //欲しいのはホワイトリスト（指名者）
        })
        .then((response)=>{
            return response.json();
        })
        .then((data)=>{
            console.log("Data:",data);
            settrfal_D(false);
            settrfal_P(false);
            setinptext("");
            setusr([]);
            navigate('/Room');
        })
        .catch((error)=>{console.log("Grp:Error sending message",error);})
    }

    const addUser=()=>{
        if(listname.trim()!==""){
            setusr([...usr, listname]);
            setlistname("");   
        }
    }

    return (
        <div>
            <div className='HeaderAdd'>
                <h1>部屋の新規作成</h1>
                <button onClick={BackRoom} className='AddBack'>戻る</button>
            </div>
            

            <input
                className='inpRoom'
                type='text'
                value={inptext}
                placeholder='新規部屋名'
                onChange={(e)=>setinptext(e.target.value)}
            />

            <label className='labelP'>
            <input
                className='radioPrivate'
                type='radio'
                checked={trfal_P}
                onClick={()=>{
                    settrfal_P(true);
                    settrfal_D(false);
                }}
            />グループ化
            </label>

            <label className='labelD'>
            <input
                className='radioDm'
                type='radio'
                checked={trfal_D}
                onClick={()=>{
                    settrfal_D(true);
                    settrfal_P(false);
                }}
            />ダイレクトメール化
            </label>

            {trfal_P ? (
                <div className='AddPuser'>
                    {usr.map((usrname,index)=>(
                        <p key={index} className='usrList'>{usrname}</p>
                    ))}
                    <input 
                        type='text' 
                        value={listname} 
                        placeholder='参加するユーザ名' 
                        onChange={(e)=>setlistname(e.target.value)}
                    />
                    <button onClick={addUser}>新たにユーザを追加</button>
                </div>                
            ):null}
            {trfal_D ? (
                <div className='AddDuser'>
                    <input 
                        type='text' 
                        value={listname} 
                        placeholder='DMユーザ名'
                        onChange={(e)=>setlistname(e.target.value)}
                    />
                </div>
            ):null}
            
            <button
                onClick={handleChange} className='Decision'
            >追加</button>
        </div>
  )
}

export default Addgrp