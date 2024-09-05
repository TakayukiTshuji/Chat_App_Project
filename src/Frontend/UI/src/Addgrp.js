//Reactフレームワーク
import React, { useState }from 'react'
//パッケージ
import { useNavigate } from 'react-router-dom';


const Addgrp = () => {
    const [inptext,setinptext]=useState("");
    const [trfal_P,settrfal_P]=useState("");
    const [trfal_D,settrfal_D]=useState("");
    const navigate=useNavigate();
    let listname;

    const handleChange=()=>{
        const textgrp={
            "grpName":inptext,
            "isPrivata":trfal_P,
            "isDm":trfal_D,
            "whitelist":[listname]
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
            navigate('/Room');
        })
        .catch((error)=>{console.log("Grp:Error sending message",error);})
    }

    const BackRoom=()=>{
        navigate('/Room')
    }

    return (
        <div>
            <button onClick={BackRoom}>戻る</button>
            <input 
                type='text'
                value={inptext}
                onChange={(e)=>setinptext(e.target.value)}
            />

            <label>
            <input
                type='radio'
                value={'閲覧するメンバーの制限'}
                checked={trfal_P === 'true'}
                onClick={()=>settrfal_P(true)}
            />isPrivate
            </label>

            <label>
            <input
                type='radio'
                value={'ダイレクトメールに変更'}
                checked={trfal_D === 'true'}
                onClick={()=>settrfal_D(true)}
            />isDm
            </label>

            <label>
            <input 
                type='button'
                onClick={()=>{
                    settrfal_D("");
                    settrfal_P("");
                }}
                value={'リセット'}
            />
            </label>

            {trfal_P ? (
                <input type='text' value={listname} placeholder='ユーザ名'/>
                
            ):(
                <></>
            )}

            <button
                onClick={handleChange}
            >Add</button>
        </div>
  )
}

export default Addgrp