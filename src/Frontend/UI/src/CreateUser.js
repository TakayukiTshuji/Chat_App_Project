//Reactフレームワーク
import React,{useState} from 'react'
//パッケージ
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';

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
            
            <input 
            type='text'
            value={input}
            placeholder='ユーザ名'
            onChange={(e)=>setinput(e.target.value)}
            />
            <input
            type='text'
            value={inpPass}
            placeholder='パスワード'
            onChange={(e)=>setinpPass(e.target.value)}
            />
            <button onClick={create}>Create!</button>
            <div>
                <Link to={'/'}>戻る</Link>
            </div>
        </div>
    )
}

export default CreateUser