import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface AuthState {
  token: string | null
  agentId: string | null
  tenantSubdomain: string | null
  setAuth: (token: string, agentId: string, tenantSubdomain: string) => void
  clearAuth: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      agentId: null,
      tenantSubdomain: null,
      setAuth: (token, agentId, tenantSubdomain) =>
        set({ token, agentId, tenantSubdomain }),
      clearAuth: () => set({ token: null, agentId: null, tenantSubdomain: null }),
    }),
    { name: 'hubion-auth' },
  ),
)
