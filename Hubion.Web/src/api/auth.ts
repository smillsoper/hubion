interface LoginRequest {
  email: string
  password: string
  tenantSubdomain: string
}

interface LoginResponse {
  token: string
  agentId: string
  expiresAt: string
}

export async function login(req: LoginRequest): Promise<LoginResponse> {
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
  return res.json() as Promise<LoginResponse>
}

export const authApi = { login }
