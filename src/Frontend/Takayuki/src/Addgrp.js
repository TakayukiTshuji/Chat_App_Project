import React, { useState }from 'react'

function setCookie(name, value, days) {
    const expirationDate = new Date();
    expirationDate.setDate(expirationDate.getDate() + days);
    const cookie = `${name}=${value}; expires=${expirationDate.toUTCString()}; path=/`;
    document.cookie = cookie;
  }

const Addgrp = () => {

    //
    //cookieをいれる
    //
    const setSessionCookie = (sessionId) => {
        setCookie("session_id", sessionId, 7); // 有効期限は7日間
    };

    const [inptext,setinptext]=useState("");
    const [trfal_P,settrfal_P]=useState(false);
    const [trfal_D,settrfal_D]=useState(false);

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
            const sessionCookie = response.headers.get('session_id');
            if (sessionCookie) {
                // クッキーが存在する場合、セッションクッキーを設定
                setSessionCookie(sessionCookie);
                console.log(sessionCookie);
            }
            console.log("response;",response);
            return response.json();
        })
        .then((data)=>{
            console.log("Data:",data);
            settrfal_D(false);
            settrfal_P(false);
            setinptext("");
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