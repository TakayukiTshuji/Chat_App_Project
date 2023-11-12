import React, { useState } from 'react';
import "./LoginForm.css";

const LoginForm = () => {
  const [name,setname]=useState("");
  const [password,setpassword]=useState("");
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
      console.table(data);
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
    </div>
  )
}

export default LoginForm