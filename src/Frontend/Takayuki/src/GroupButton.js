import React,{useState} from "react";
//import { Link } from 'react-router-dom;'
import "./Gruup.css";

const GroupButton = () => {
    const [Group,setGroup]=useState("");
    const ClickHandle=(e)=>{
      setGroup(e.target.value);
    }
    return (
      <div className="Gruup">
        <div className="GrupDis">
            <p>グループ一覧</p>
        </div>

        <div className="NameDisplay">
            <p>{Group}</p>
        </div>


        <div className="memberDis">
            <input className="GrupBtn1" type='button' value='Name0' />
            <input className="GrupBtn2" type='button' value='Name1' onClick={ClickHandle}/>
            <input className="GrupBtn3" type='button' value='Name2' onClick={ClickHandle}/>
        </div>
      </div>
    );
}
export default GroupButton