import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import { authApi } from '../api/authApi'
import { usersApi } from '../api/usersApi'
import { signalRService } from '../services/signalrService'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)

  const logout = useCallback(async () => {
    const rt = localStorage.getItem('refreshToken')
    if (rt) {
      try { await authApi.revokeToken(rt) } catch { /* ignore */ }
    }
    await signalRService.disconnect()
    localStorage.clear()
    setUser(null)
  }, [])

  const connectHub = useCallback(async (token) => {
    try {
      await signalRService.connect(token)
    } catch (err) {
      console.error('SignalR connection error:', err)
    }
  }, [])

  // Restore session on mount
  useEffect(() => {
    const init = async () => {
      const token = localStorage.getItem('accessToken')
      if (!token) { setLoading(false); return }

      try {
        const { data } = await usersApi.getMe()
        if (data.success) {
          setUser(data.data)
          await connectHub(token)
        }
      } catch {
        localStorage.clear()
      } finally {
        setLoading(false)
      }
    }
    init()
  }, [connectHub])

  const login = useCallback(async (dto) => {
    const { data } = await authApi.login(dto)
    if (!data.success) throw new Error(data.message)

    const authData = data.data
    localStorage.setItem('accessToken', authData.token)
    localStorage.setItem('refreshToken', authData.refreshToken)

    const meRes = await usersApi.getMe()
    setUser(meRes.data.data)
    await connectHub(authData.token)

    return authData
  }, [connectHub])

  const register = useCallback(async (dto) => {
    const { data } = await authApi.register(dto)
    if (!data.success) throw new Error(data.message)

    const authData = data.data
    localStorage.setItem('accessToken', authData.token)
    localStorage.setItem('refreshToken', authData.refreshToken)

    const meRes = await usersApi.getMe()
    setUser(meRes.data.data)
    await connectHub(authData.token)

    return authData
  }, [connectHub])

  const updateProfile = useCallback((updated) => {
    setUser((prev) => ({ ...prev, ...updated }))
  }, [])

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout, updateProfile }}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
