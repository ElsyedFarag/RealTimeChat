import api from './axiosInstance'
export const usersApi = {
  getAll: (pageNumber = 1, pageSize = 20) => api.get('/users', { params: { pageNumber, pageSize } }),
  getMe: () => api.get('/users/me'),
  getById: (id) => api.get(`/users/${id}`),
  search: (query) => api.get('/users/search', { params: { query } }),
  updateUser: (id, formData) => api.put(`/users/${id}`, formData, { headers: { 'Content-Type': 'multipart/form-data' } }),
}
