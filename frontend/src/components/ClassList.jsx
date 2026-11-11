import { useState, useEffect } from "react";
import api from "../services/api";

function ClassList() {
  const [classes, setClasses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [studentId, setStudentId] = useState("");
  const [inputError, setInputError] = useState(false);

  useEffect(() => {
    const fetchClasses = async () => {
      try {
        const response = await api.get("/classes");
        setClasses(response.data);
      } catch (err) {
        setError("Error loading data. Is your backend running?");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchClasses();
  }, []);

  const handleInputChange = (e) => {
    const value = e.target.value;
    // Allow only numeric input and restrict to a maximum of 17 characters
    if (/^\d*$/.test(value) && value.length <= 17) {
      setStudentId(value);
      setInputError(false); // Clear error on valid input change
    }
  };

  const handleApply = async (classId) => {
    // Validate length before applying
    if (studentId.length !== 17) {
      setInputError(true);
      alert("please enter a valid 17-digit student ID!");
      return;
    }

    try {
      const response = await api.post(
        `/classes/${classId}/holds`,
        {},
        {
          headers: {
            "X-Student-Id": studentId,
          },
        },
      );

      alert(
        `Success! Your seat has been held successfully.\nHold ID: ${response.data.holdId}`,
      );
      window.location.reload();
    } catch (err) {
      alert(err.response?.data?.message || "Failed to hold seat!");
      console.error(err);
    }
  };

  if (loading) return <div>Loading classes...</div>;
  if (error) return <div style={{ color: "red" }}>{error}</div>;

  return (
    <div>
      <h2>Available Classes</h2>

      <div
        style={{
          marginBottom: "20px",
          padding: "15px",
          backgroundColor: "#f8f9fa",
          borderRadius: "8px",
          display: "inline-block",
        }}
      >
        <label>
          <strong>Enter your 17-digit Student ID: </strong>
        </label>
        <input
          type="text"
          placeholder="e.g. 12345678901234567"
          value={studentId}
          onChange={handleInputChange}
          style={{
            padding: "8px",
            width: "200px",
            marginLeft: "10px",
            borderRadius: "4px",
            border: inputError ? "2px solid red" : "1px solid #ccc", // Apply red border on error
            outline: "none",
          }}
        />
      </div>

      <div style={{ display: "flex", gap: "20px", flexWrap: "wrap" }}>
        {classes.map((cls) => (
          <div
            key={cls.id}
            style={{
              border: "1px solid #ccc",
              padding: "15px",
              borderRadius: "8px",
              minWidth: "250px",
            }}
          >
            <h3>{cls.name}</h3>
            <p>
              <strong>Total Seats:</strong> {cls.totalSeats}
            </p>
            <p>
              <strong>Available Seats:</strong> {cls.availableSeats}
            </p>
            <button
              onClick={() => handleApply(cls.id)}
              style={{
                padding: "10px 15px",
                cursor: "pointer",
                backgroundColor: "#007bff",
                color: "white",
                border: "none",
                borderRadius: "4px",
              }}
            >
              Apply (Hold Seat)
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}

export default ClassList;
