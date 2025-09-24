import { jwtDecode } from "jwt-decode";
import api from "./api";
import { User } from "../types";

export interface LoginData { email: string; password: string; }
export interface RegisterData { email: string; password: string; role: "User" | "Admin"; }

export const login = async (data: LoginData): Promise<User> => {
  const res = await api.post("/auth/login", data);
  localStorage.setItem("token", res.data.token);
  return getCurrentUser()!;
};

export const register = async (data: RegisterData) => {
  await api.post("/auth/register", data);
};

export const logout = () => localStorage.removeItem("token");

export const getCurrentUser = (): User | null => {
  const token = localStorage.getItem("token");
  if (!token) return null;
  try {
    const decoded: any = jwtDecode(token);
    return { id: decoded.sub, email: decoded.email, role: decoded.role, token };
  } catch {
    return null;
  }
};
