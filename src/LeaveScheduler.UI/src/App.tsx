import { useState, useEffect } from 'react'
import axios from 'axios'

const API_BASE = "http://localhost:5117/api/leave";

interface LeaveRequest {
  id: number;
  employeeId: number;
  startDate: string;
  endDate: string;
  status: string;
  employee?: { name: string; team: string };
}

function App() {
  const [calendar, setCalendar] = useState<LeaveRequest[]>([]);
  const [pending, setPending] = useState<LeaveRequest[]>([]);
  const [message, setMessage] = useState("");

  const [newReq, setNewReq] = useState({ 
    employeeId: 1, 
    startDate: '2026-07-10', // Friday
    endDate: '2026-07-15'   // Wednesday
});

  // Load data from the Backend
  const fetchData = async () => {
    try {
      const calRes = await axios.get(`${API_BASE}/calendar`);
      const pendRes = await axios.get(`${API_BASE}/pending`);
      setCalendar(calRes.data);
      setPending(pendRes.data);
    } catch (err) {
      console.error("Error fetching data", err);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const submitRequest = async (e: React.FormEvent) => {
  e.preventDefault();
  try {
    await axios.post(`${API_BASE}/request`, newReq);
    setMessage("Request submitted!");
    fetchData();
  } catch (err: any) {
    alert(err.response?.data?.message || "Error submitting request");
  }
};

  const handleStatus = async (id: number, status: string) => {
    try {
      await axios.post(`${API_BASE}/${id}/status`, `"${status}"`, {
        headers: { 'Content-Type': 'application/json' }
      });
      setMessage(`Request ${status}!`);
      fetchData();
    } catch (err: any) {
      alert(err.response?.data?.message || "Error updating status");
    }
  };

  return (
    <div style={{ padding: '20px', fontFamily: 'sans-serif' }}>
      <h1>Team Leave Scheduler</h1>
      {message && <p style={{ color: 'green', fontWeight: 'bold' }}>{message}</p>}

      <form onSubmit={submitRequest} style={{ marginBottom: '30px', padding: '15px', background: '#f9f9f9' }}>
  <h3>Submit New Request</h3>
  Employee ID: <input type="number" value={newReq.employeeId} onChange={e => setNewReq({...newReq, employeeId: parseInt(e.target.value)})} />
  Start: <input type="date" onChange={e => setNewReq({...newReq, startDate: e.target.value})} />
  End: <input type="date" onChange={e => setNewReq({...newReq, endDate: e.target.value})} />
  <button type="submit">Submit Request</button>
</form>

      <section>
        <h2>Approved Leave (Next 30 Days)</h2>
        <table border={1} cellPadding={10} style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ backgroundColor: '#f2f2f2' }}>
              <th>Employee</th>
              <th>Team</th>
              <th>Start Date</th>
              <th>End Date</th>
            </tr>
          </thead>
          <tbody>
            {calendar.length === 0 ? <tr><td colSpan={4}>No approved leave yet.</td></tr> : 
              calendar.map(req => (
                <tr key={req.id}>
                  <td>{req.employee?.name}</td>
                  <td>{req.employee?.team}</td>
                  <td>{new Date(req.startDate).toLocaleDateString('en-GB')}</td>
                  <td>{new Date(req.endDate).toLocaleDateString('en-GB')}</td>
                </tr>
              ))
            }
          </tbody>
        </table>
      </section>

      <section style={{ marginTop: '40px' }}>
        <h2>Pending Requests (Manager Queue)</h2>
        <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
          {pending.length === 0 ? <p>No pending requests.</p> : 
            pending.map(req => (
              <div key={req.id} style={{ border: '1px solid #ccc', padding: '15px', borderRadius: '8px' }}>
                <strong>{req.employee?.name}</strong> ({req.employee?.team})<br/>
                {new Date(req.startDate).toLocaleDateString('en-GB')} to {new Date(req.endDate).toLocaleDateString('en-GB')}
                <div style={{ marginTop: '10px' }}>
                  <button onClick={() => handleStatus(req.id, "Approved")} style={{ backgroundColor: 'green', color: 'white', marginRight: '5px' }}>Approve</button>
                  <button onClick={() => handleStatus(req.id, "Rejected")} style={{ backgroundColor: 'red', color: 'white' }}>Reject</button>
                </div>
              </div>
            ))
          }
        </div>
      </section>
    </div>
  );
}



export default App;