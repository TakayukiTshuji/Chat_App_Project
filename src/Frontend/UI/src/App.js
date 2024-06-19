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
    //<BrowserRouter>ブラウザのアドレスバーの現在地を設定
    //<Routes>複数の遷移を作る際に使われる
    //<Route>各遷移するアドレスバー名と、遷移するコンポーネントを作成
    <BrowserRouter>
      <Routes>
        <Route path={'/'} element={<LoginForm/>}/>
        <Route path={'/Home'} element={<Home/>}/>
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