import "./App.css";
import ClassList from "./components/ClassList";
import MyHolds from "./components/MyHolds"; // import MyHolds 

function App() {
  return (
    <div style={{ padding: "20px", fontFamily: "Arial" }}>
      <h1>School Admission System</h1>
      <hr style={{ marginBottom: "20px" }} />

      {/* Class list and application section */}
      <ClassList />

      {/* Section to check and confirm your application status */}
      <MyHolds />
    </div>
  );
}

export default App;
