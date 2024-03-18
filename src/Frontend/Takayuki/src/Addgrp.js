import React, { useState }from 'react'
import { useNavigate } from 'react-router-dom';


const Addgrp = () => {
    const [inptext,setinptext]=useState("");
    const [trfal_P,settrfal_P]=useState(false);
    const [trfal_D,settrfal_D]=useState(false);
    const navigate=useNavigate();

    const handleChange=()=>{
        const textgrp={
            "grpName":inptext,
            "isPrivata":trfal_P,
            "isDm":trfal_D,
            "whitelist":[]
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

  return (
    <div>
        <input 
            type='text'
            value={inptext}
            onChange={(e)=>setinptext(e.target.value)}
        />
        <input
            type='radio'
            value={'閲覧するメンバーの制限'}
            onClick={()=>settrfal_P(true)}
        />
        <label>isPrivate</label>
        <input
            type='radio'
            value={'ダイレクトメールに変更'}
            onClick={()=>settrfal_D(true)}
        />
        <label>isDm</label>
        <p>一回trueにすると元に戻せません</p>
        <button
            onClick={handleChange}
        >Add</button>
    </div>
  )
}

export default Addgrp