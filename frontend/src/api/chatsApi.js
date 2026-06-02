import api from './axiosInstance'

export const chatsApi = {
  /** GET /chats/my */
  getMy: () => api.get('/chats/my'),

  /** GET /chats */
  getAll: () => api.get('/chats'),

  /** GET /chats/:id */
  getById: (id) => api.get(`/chats/${id}`),

  /** POST /chats/private */
  createPrivate: (userId) => api.post('/chats/private', { userId }),

  /**
   * POST /chats/group
   * @param {{ name: string, memberIds: string[] }} dto
   */
  createGroup: (dto) => api.post('/chats/group', dto),

  /**
   * PATCH /chats/:id/members/add
   * @param {string} id  chatId
   * @param {string[]} userIds
   */
  addMembers: (id, userIds) => api.patch(`/chats/${id}/members/add`, { userIds }),

  /**
   * PATCH /chats/:id/members/remove
   * @param {string} id  chatId
   * @param {string} userId
   */
  removeMember: (id, userId) => api.patch(`/chats/${id}/members/remove`, { userId }),

  /** DELETE /chats/:id */
  delete: (id) => api.delete(`/chats/${id}`),
}