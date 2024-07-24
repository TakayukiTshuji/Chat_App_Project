//Reactフレームワーク
import React,{useState} from 'react'
//パッケージ
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';
//CSS
import './CreateUser.css';

const CreateUser = () => {
    const [input,setinput]=useState("");
    const [inpPass,setinpPass]=useState("");
    const navigate = useNavigate();

    const create=()=>{
        fetch(`https://localhost:7038/api/ChatUserCtl?userId=${input}&familyName=${input}&firstName=${input}&language=${input}&studentId=${input}&password=${inpPass}&nickname_ja=${input}&nickname_en=${input}`,{
            method:'POST',
            credentials:'include',
            headers:{
                "Content-Type": "application/json"
            }
        })
        .then((response)=>response.json())
        .then((data)=>{
            console.log("successData:",data);
            navigate("/");
        })
        .catch((err)=>{console.log(err);})
    }

    return (
        <div>
            <div className="Header">
                <p className="chatapp">ChatApp</p>
            </div>

            <div className='inpfield'>
                <input 
                type='text'
                value={input}
                placeholder='ユーザ名'
                onChange={(e)=>setinput(e.target.value)}
                className='inptext'
                />
            </div>

            <div className='inpfield'>
                <input
                type='text'
                value={inpPass}
                placeholder='パスワード'
                onChange={(e)=>setinpPass(e.target.value)}
                className='inppassword'
                />
            </div>

            <button
            onClick={create}
            className='clickbu'>Create!</button>
            <div className='linkCreateUser'>
                <Link to={'/'}>戻る</Link>
            </div>
        </div>
    )
}

export default CreateUser