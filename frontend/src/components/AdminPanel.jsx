import { useState } from 'react';
import api from '../services/api';

function AdminPanel() {
  const [className, setClassName] = useState('');
  const [totalSeats, setTotalSeats] = useState('');
  
  const [holdId, setHoldId] = useState('');
  const [holdDetails, setHoldDetails] = useState(null);

  const handleCreateClass = async (e) => {
    e.preventDefault();
    try {
      await api.post('/classes', {
        name: className,
        totalSeats: parseInt(totalSeats)
      });
      alert('New class created successfully!');
      setClassName('');
      setTotalSeats('');
      window.location.reload(); 
    } catch (err) {
      alert('Error creating class!');
      console.error(err);
    }
  };

  const handleCheckHold = async (e) => {
    e.preventDefault();
    try {
      const response = await api.get(`/holds/${holdId}`);
      setHoldDetails(response.data);
    } catch (err) {
      console.error(err);
      alert('No application found for this ID!');
      setHoldDetails(null);
    }
  };

  return (
    <div style={{ padding: '20px', backgroundColor: '#fff3cd', borderRadius: '8px', marginBottom: '20px' }}>
      <h2>Admin Panel</h2>
      
      <div style={{ display: 'flex', gap: '40px', flexWrap: 'wrap' }}>
        <div style={{ flex: 1, minWidth: '300px', backgroundColor: 'white', padding: '15px', borderRadius: '5px' }}>
          <h3>Create New Class</h3>
          <form onSubmit={handleCreateClass}>
            <input 
              type="text" placeholder="Class Name (e.g. Class 7 - Day Shift)" value={className}
              onChange={(e) => setClassName(e.target.value)} required
              style={{ display: 'block', width: '90%', padding: '8px', marginBottom: '10px' }}
            />
            <input 
              type="number" placeholder="Total Seats" value={totalSeats}
              onChange={(e) => setTotalSeats(e.target.value)} required
              style={{ display: 'block', width: '90%', padding: '8px', marginBottom: '10px' }}
            />
            <button type="submit" style={{ padding: '8px 15px', backgroundColor: '#17a2b8', color: 'white', border: 'none', cursor: 'pointer' }}>
              Create Class
            </button>
          </form>
        </div>

        <div style={{ flex: 1, minWidth: '300px', backgroundColor: 'white', padding: '15px', borderRadius: '5px' }}>
          <h3>Check Hold Status</h3>
          <form onSubmit={handleCheckHold}>
            <input 
              type="text" placeholder="Enter Hold ID (GUID)" value={holdId}
              onChange={(e) => setHoldId(e.target.value)} required
              style={{ display: 'block', width: '90%', padding: '8px', marginBottom: '10px' }}
            />
            <button type="submit" style={{ padding: '8px 15px', backgroundColor: '#6c757d', color: 'white', border: 'none', cursor: 'pointer' }}>
              Check Status
            </button>
          </form>
          {holdDetails && (
            <div style={{ marginTop: '10px', fontSize: '14px' }}>
              <p><strong>Status:</strong> {holdDetails.status}</p>
              <p><strong>Expires:</strong> {new Date(holdDetails.expiresAt).toLocaleString()}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default AdminPanel;