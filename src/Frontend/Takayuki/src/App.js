/*import GroupButton from "./GroupButton";*/
import { BrowserRouter, Routes, Route } from "react-router-dom";
//import GroupButton from "./GroupButton";
import LoginForm from "./LoginForm";
import MessageApp from "./MessageApp";

function App() {  
  return (
    <BrowserRouter>
      <Routes>
        <Route path={'/'} element={<LoginForm/>}/>
        <Route path={'/chat'} element={<MessageApp/>}/>
      </Routes>
     </BrowserRouter>
  );//コンポーネント化をするときは最初は大文字にする
  //<GroupButton/>
}

export default App;
