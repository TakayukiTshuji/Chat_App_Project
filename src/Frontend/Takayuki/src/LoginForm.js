import React, {  useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCookies } from 'react-cookie';
import "./LoginForm.css";

const LoginForm = () => {
  const [name,setname]=useState("");
  const [password,setpassword]=useState("");
  const [dat,setDat]=useState(null);
  const navigate = useNavigate();
  const [cookies,setCookie]=useCookies();

  const handleSession=()=>{
    const loginform={
      "userId":name,
      "password":password
    };
    fetch(`https://localhost:7038/api/ChatSessionCtl?userId=${name}&password=${password}`,{
      method:'POST',
      credentials:'include',
      headers:{
        "Content-Type": "application/json"
      },
      body:JSON.stringify(loginform),
    })
    .then((response)=>response.json())
    .then((data) => {
      console.log("data",data);
      setname("");
      setpassword("");
      setDat(data);
      navigate("/chat");
      setCookie("name",data);
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
      onChange={(e)=>setname(e.target.value)}
      />
      <input
      type='text'
      value={password}
      onChange={(e)=>setpassword(e.target.value)}
      />
      <button onClick={handleSession}>Send</button>

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