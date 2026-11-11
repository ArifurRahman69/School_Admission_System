import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7089", // backend URL
});

export default api;
