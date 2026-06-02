import api from './axiosInstance'
export const authApi = {
  register: (dto) => api.post('/auth/register', dto),
  login: (dto) => api.post('/auth/login', dto),
  refreshToken: (refreshToken) => api.post('/auth/refresh-token', { refreshToken }),
  revokeToken: (token) => api.post('/auth/revoke-token', { token }),
}
