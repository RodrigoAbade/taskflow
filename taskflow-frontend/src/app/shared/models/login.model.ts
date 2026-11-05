
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  message: string;
  token: string;
  tokenType: string;
  expiresIn: number;
  user: unknown;
}
