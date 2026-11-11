import { useState } from "react";
import api from "../services/api";

function MyHolds() {
  const [studentId, setStudentId] = useState("");
  const [holds, setHolds] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // Fetching applications based on the student's ID
  const fetchMyHolds = async () => {
    if (studentId.length !== 17) {
      alert("Please enter a valid 17-digit student ID!");
      return;
    }

    setLoading(true);
    setError("");

    try {
      const response = await api.get("/me/holds", {
        headers: { "X-Student-Id": studentId },
      });
      setHolds(response.data);
    } catch (err) {
      setError(
        err.response?.data?.message ||
          "Error fetching data! / Failed to retrieve information!",
      );
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // Confirm seat (Admission)
  const handleConfirm = async (holdId) => {
    try {
      await api.post(
        `/holds/${holdId}/confirm`,
        {},
        {
          headers: { "X-Student-Id": studentId },
        },
      );
      alert("Congratulations! Your admission is complete (CONFIRMED)");
      fetchMyHolds(); // Refreshing the list to show status updates.
    } catch (err) {
      alert(
        err.response?.data?.message ||
          "Failed to confirm admission or the session has expired!",
      );
    }
  };

  return (
    <div
      style={{
        marginTop: "40px",
        padding: "20px",
        backgroundColor: "#e9ecef",
        borderRadius: "8px",
      }}
    >
      <h2>My Dashboard (Check Status & Confirm)</h2>

      <div style={{ marginBottom: "20px" }}>
        <input
          type="text"
          placeholder="Enter your 17-digit ID"
          value={studentId}
          onChange={(e) => {
            if (/^\d*$/.test(e.target.value) && e.target.value.length <= 17) {
              setStudentId(e.target.value);
            }
          }}
          style={{
            padding: "8px",
            width: "250px",
            borderRadius: "4px",
            border: "1px solid #ccc",
            outline: "none",
          }}
        />
        <button
          onClick={fetchMyHolds}
          style={{
            padding: "9px 15px",
            marginLeft: "10px",
            backgroundColor: "#28a745",
            color: "white",
            border: "none",
            borderRadius: "4px",
            cursor: "pointer",
          }}
        >
          Check My Status
        </button>
      </div>

      {loading && <p>Loadinggg...</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}

      {/* Apply list */}
      {holds.length > 0 && (
        <table
          style={{
            width: "100%",
            borderCollapse: "collapse",
            marginTop: "10px",
            backgroundColor: "white",
          }}
        >
          <thead>
            <tr
              style={{
                backgroundColor: "#343a40",
                color: "white",
                textAlign: "left",
              }}
            >
              <th style={{ padding: "10px", border: "1px solid #ddd" }}>
                Class
              </th>
              <th style={{ padding: "10px", border: "1px solid #ddd" }}>
                Status
              </th>
              <th style={{ padding: "10px", border: "1px solid #ddd" }}>
                Expires At
              </th>
              <th style={{ padding: "10px", border: "1px solid #ddd" }}>
                Action
              </th>
            </tr>
          </thead>
          <tbody>
            {holds.map((hold) => (
              <tr key={hold.holdId}>
                <td style={{ padding: "10px", border: "1px solid #ddd" }}>
                  {hold.className}
                </td>
                <td
                  style={{
                    padding: "10px",
                    border: "1px solid #ddd",
                    fontWeight: "bold",
                    color:
                      hold.status === "CONFIRMED"
                        ? "green"
                        : hold.status === "EXPIRED"
                          ? "red"
                          : "orange",
                  }}
                >
                  {hold.status}
                </td>
                <td style={{ padding: "10px", border: "1px solid #ddd" }}>
                  {new Date(hold.expiresAt).toLocaleString()}
                </td>
                <td style={{ padding: "10px", border: "1px solid #ddd" }}>
                  {hold.status === "HELD" && (
                    <button
                      onClick={() => handleConfirm(hold.holdId)}
                      style={{
                        padding: "6px 12px",
                        backgroundColor: "#007bff",
                        color: "white",
                        border: "none",
                        borderRadius: "4px",
                        cursor: "pointer",
                      }}
                    >
                      Confirm Admission
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

export default MyHolds;
