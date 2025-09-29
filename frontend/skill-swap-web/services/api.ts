import axios from "axios";

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:5095",
  withCredentials: false
});

api.interceptors.request.use(config => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export const getOffers = async (page: number = 1, pageSize: number = 10) => {
  const response = await api.get("/api/offers", { params: { page, pageSize } });
  return response.data;
};

export default api;
