import { api } from './client'

interface LoginRequest {
  email: string
  password: string
  tenantSubdomain: string
}

export interface AuthResponse {
  token: string
  agentId: string
  email: string
  firstName: string
  lastName: string
  role: string
  tenantSubdomain: string
}

export async function login(req: LoginRequest): Promise<AuthResponse> {
  // Auth endpoint requires the tenant header, set it manually before auth state is populated
  const res = await fetch('/api/v1/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Tenant-Subdomain': req.tenantSubdomain,
    },
    body: JSON.stringify({ email: req.email, password: req.password }),
  })

  if (!res.ok) throw new Error('Invalid credentials')
  return res.json() as Promise<AuthResponse>
}

async function refresh(): Promise<AuthResponse> {
  return api.post<AuthResponse>('/api/v1/auth/refresh')
}

export const authApi = { login, refresh }
