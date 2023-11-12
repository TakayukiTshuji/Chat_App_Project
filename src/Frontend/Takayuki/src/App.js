/*import GroupButton from "./GroupButton";*/
import GroupButton from "./GroupButton";
import MessageApp from "./MessageApp";
import LoginForm from "./LoginForm";

function App() {  
  
  return (
    <div>
      <LoginForm/>
      <GroupButton/>
      <MessageApp/>
    </div>
  );//コンポーネント化をするときは最初は大文字にする
}

export default App;
