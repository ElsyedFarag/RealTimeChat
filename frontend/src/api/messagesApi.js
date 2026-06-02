import api from './axiosInstance'
export const messagesApi = {
  getByChatId: (chatId, pageNumber = 1, pageSize = 20) =>
    api.get(`/messages/chat/${chatId}`, { params: { pageNumber, pageSize } }),
  getById: (id) => api.get(`/messages/${id}`),
  send: (dto) => api.post('/messages', dto),
  update: (id, dto) => api.put(`/messages/${id}`, dto),
  delete: (id) => api.delete(`/messages/${id}`),
  markSeen: (id) => api.post(`/messages/${id}/seen`),
  getReceipts: (id) => api.get(`/messages/${id}/receipts`),
}
