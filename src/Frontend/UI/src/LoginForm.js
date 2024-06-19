//Reactフレームワーク
import React, {  useState,useEffect } from 'react';
//パッケージ
import { useNavigate,Link } from 'react-router-dom';
import { useCookies } from 'react-cookie';
//外部ライブラリ
import CryptoJS from 'crypto-js';
import {motion} from "framer-motion"
//CSS
import "./LoginForm.css";

const LoginForm = () => {
  const [name,setname]=useState("");
  const [password,setpassword]=useState("");
  const [dat,setDat]=useState(null);
  const navigate = useNavigate();
  const [cookies,setCookie]=useCookies(['session_id']);

  const ChangeHas=()=>{
    const CHname=name;
    const CHpass=CryptoJS.SHA256(password).toString(CryptoJS.enc.Hex).toUpperCase();
    handleSession(CHname,CHpass);
  }

  const handleSession=(handName,handPass)=>{
    fetch(`https://localhost:7038/api/ChatSessionCtl?userId=${handName}&password=${handPass}`,{
      method:'POST',
      credentials:'include',
      headers:{
        "Content-Type": "application/json",
      }
    })
    .then((response) => response.json())
    .then((data) => {
      console.log(data);
      setname("");      //入力した文字を消す
      setpassword("");  //入力した文字を消す
      setDat(data.message);     //dataがあるか確認するテスト
      setCookie('session_id',data.result.sessionId,{path:'/',sameSite:'none'});
      //session_idの名前、session_idの値、オプションでどこでも使えるようにした      

      //ページ遷移を行う
      navigate("/Room");
    })
    .catch((error) => {
      console.error("Error during login:", error);
    });
  }

  return (
    <div className='logininput'>
      <input 
      type='text'
      value={name}
      placeholder='ユーザ名'
      onChange={(e)=>setname(e.target.value)}
      />
      <input
      type='password'
      value={password}
      placeholder='パスワード'
      onChange={(e)=>setpassword(e.target.value)}
      />
      <button onClick={ChangeHas}>Send</button>

      <div>
        新規作成はこちら<Link to={'/session'}>こちら</Link>
      </div>

      <div>
        {dat ? (
          <p>{dat}</p>
        ):(
          <></>
        )}
      </div>
    </div>
  )
}
export default LoginForm