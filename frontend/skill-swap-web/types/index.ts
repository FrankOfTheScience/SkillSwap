export interface Offer {
  id: number;
  title: string;
  description: string;
  price: number;
  createdBy: string;
}

export interface User {
  id: string;  // This should be string because JWT tokens contain string values
  email: string;
  displayName: string;
  role: "User" | "Admin";
  token?: string;
}
