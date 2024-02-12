/*import GroupButton from "./GroupButton";*/
import { BrowserRouter, Routes, Route } from "react-router-dom";
//import GroupButton from "./GroupButton";
import LoginForm from "./LoginForm";
import MessageApp from "./MessageApp";
import CreateUser from "./CreateUser";
import Room from "./Room";
import Addgrp from "./Addgrp";

function App() {  
  return (
    <BrowserRouter>
      <Routes>
        <Route path={'/'} element={<LoginForm/>}/>
        <Route path={'/Room'} element={<Room/>}/>
        <Route path={'/AddGroup'} element={<Addgrp/>}/>
        <Route path={'/session'} element={<CreateUser/>}/>
        <Route path={'/chat'} element={<MessageApp/>}/>
      </Routes>
     </BrowserRouter>
  );//コンポーネント化をするときは最初は大文字にする
  //<GroupButton/>
}

export default App