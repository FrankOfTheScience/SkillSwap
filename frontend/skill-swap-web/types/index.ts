export interface Offer {
  id: string;
  title: string;
  description: string;
  price: number;
  createdBy: string;
}

export interface User {
  id: string;
  email: string;
  role: "User" | "Admin";
  token?: string;
}
