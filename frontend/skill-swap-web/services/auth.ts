import { jwtDecode } from "jwt-decode";
import api from "./api";
import { User } from "../types";

export interface LoginData { email: string; password: string; }
export interface RegisterData { email: string; displayName: string; password: string; role: "User" | "Admin"; }

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
    console.log("Decoded token:", decoded); // For debugging
    
    // Extract role from various possible claim formats
    let role = "User";
    if (decoded.role) {
      role = decoded.role;
    } else if (decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]) {
      const roleClim = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      role = Array.isArray(roleClim) ? roleClim[0] : roleClim;
    } else if (decoded.roles) {
      role = Array.isArray(decoded.roles) ? decoded.roles[0] : decoded.roles;
    }
    
    const user = { 
      id: decoded.sub || decoded.jti, 
      email: decoded.email, 
      displayName: decoded.displayName || decoded.name || decoded.email,
      role: role as "User" | "Admin", 
      token 
    };
    
    console.log("Parsed user:", user); // For debugging
    return user;
  } catch (error) {
    console.error("Error decoding token:", error);
    return null;
  }
};
