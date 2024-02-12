import React, {  useState,useEffect } from 'react';
import { useNavigate,Link } from 'react-router-dom';
import { useCookies } from 'react-cookie';
import CryptoJS from 'crypto-js';
//import Cookies from 'react-cookie';
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
  }

  const handleSession=()=>{
    fetch(`https://localhost:7038/api/ChatSessionCtl?userId=${name}&password=${password}`,{
      method:'POST',
      credentials:'include',
      mode:'cors',
      headers:{
        "Content-Type": "application/json",
      }
    })
    .then((response) => response.json())
    .then((data) => {
      console.log("sessionId", data.result.sessionId);
      setname("");      //入力した文字を消す
      setpassword("");  //入力した文字を消す
      setDat(data);     //dataがあるか確認するテスト
      setCookie('session_id',data.result.sessionId,{path:'/',sameSite:'none',httpOnly:true});
      //session_idの名前、session_idの値、オプションでどこでも使えるようにした
      console.log('cookie:',cookies);
      testcoo();

      //ページ遷移を行う
      navigate("/Room");
    })
    .catch((error) => {
      console.error("Error during login:", error);
    });
  }
  const testcoo=()=>{
    fetch(`https://localhost:7038/api/ChatSessionCtl`,{
      method:'GET',
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
        console.log('login,cookie:',cookies);
        console.log('login:',data);
        //setCookie('session_id',data.result.sessionId);
    })
  }
  useEffect(()=>{
    testcoo()
  },[])

  return (
    <div className='logininput'>
      <input 
      type='text'
      value={name}
      onChange={(e)=>setname(e.target.value)}
      />
      <input
      type='text'
      value={password}
      onChange={(e)=>setpassword(e.target.value)}
      />
      <button onClick={handleSession}>Send</button>

      <div>
        新規作成はこちら<Link to={'/session'}>こちら</Link>
      </div>

      {dat ? (
        <div>
          <p>成功</p>
        </div>
        
      ):(
        <p>データ取得中...</p>
      )}
    </div>
  )
}
export default LoginForm