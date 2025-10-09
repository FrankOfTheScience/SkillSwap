"use client";
import Link from "next/link";
import { useEffect, useState } from "react";
import { getCurrentUser, logout } from "../services/auth";
import { User } from "../types";

export default function Navbar() {
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => setUser(getCurrentUser()), []);

  const handleLogout = () => {
    logout();
    setUser(null);
  };

  return (
    <nav className="bg-gray-800 text-white p-4 flex justify-between">
      <div>
        <Link href="/">Home</Link>
        {user && <Link href="/offers" className="ml-4">Offers</Link>}
        {user && <Link href="/profile" className="ml-4">Profile</Link>}
        {user && <Link href="/dashboard" className="ml-4">Dashboard</Link>}
        {user?.role === "Admin" && <Link href="/admin" className="ml-4">Admin</Link>}
        {user?.role === "Admin" && <Link href="/offers/create" className="ml-4">Create Offer</Link>}
      </div>
      <div>
        {!user ? (
          <>
            <Link href="/login" className="mr-2">Login</Link>
            <Link href="/register">Register</Link>
          </>
        ) : (
          <button onClick={handleLogout}>Logout</button>
        )}
      </div>
    </nav>
  );
}
