import { Routes, Route, Link } from "react-router-dom";
import { useNavigate } from "react-router-dom";
import './Home.css';

function Home() {
    const navigate = useNavigate()
    const handleSessionForm = () => {
      navigate('/Session')
  }
  const handleLoginForm = () => {
        navigate('/Home')
  }
  return (
    <div>
      <div className="Head">
        <p className="chatapp">ChatApp</p>
        <p className="logn">ログイン/新規作成</p>
      </div>

      <div className="Body">
        <div className="item">
            <div className="text">新しく登録しましょう</div>
            <input type="button" onClick={handleSessionForm} value="新規登録"/>
        </div>
        <div className="item">
            <div classsName="text">もう作成した？ではログインしよう！</div>
            <input type="button"  onClick={handleLoginForm } value="ログイン"/>
            
        </div>
      </div>
    </div>
  )
}
export default Home;
