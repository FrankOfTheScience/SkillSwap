import { jwtDecode } from "jwt-decode";
import api from "./api";
import { User } from "../types";
import { getErrorMessage } from "../utils/errorHandler";

export interface LoginData { email: string; password: string; }
export interface RegisterData { email: string; displayName: string; password: string; }

export const login = async (data: LoginData): Promise<User> => {
  try {
    const res = await api.post("/api/auth/login", data);
    localStorage.setItem("token", res.data.token);
    return getCurrentUser()!;
  } catch (error) {
    // Re-throw with friendly error message
    const friendlyMessage = getErrorMessage(error);
    throw new Error(friendlyMessage);
  }
};

export const register = async (data: RegisterData): Promise<User> => {
  try {
    const res = await api.post("/api/auth/register", data);
    // Auto-login after successful registration
    if (res.data.token) {
      localStorage.setItem("token", res.data.token);
      return getCurrentUser()!;
    } else {
      // If no token returned, perform login
      return await login({ email: data.email, password: data.password });
    }
  } catch (error) {
    // Re-throw with friendly error message
    const friendlyMessage = getErrorMessage(error);
    throw new Error(friendlyMessage);
  }
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
