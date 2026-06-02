/**
 * @typedef {Object} AuthResponseDto
 * @property {boolean} isAuthenticated
 * @property {string}  message
 * @property {string}  token
 * @property {string}  refreshToken
 * @property {string}  expiresAt
 * @property {string}  userId
 * @property {string}  email
 * @property {string}  userName
 */

/**
 * @typedef {Object} AppUserDto
 * @property {string}   id
 * @property {string}   userName
 * @property {string}   email
 * @property {string}   firstName
 * @property {string}   lastName
 * @property {string}   fullName
 * @property {string|null} profilePictureUrl
 * @property {boolean}  isOnline
 * @property {string}   createdAt
 * @property {string|null} lastSeenAt
 */

/**
 * @typedef {Object} ChatListDto
 * @property {string}  id
 * @property {string|null} name
 * @property {boolean} isGroup
 * @property {string|null} imageUrl
 * @property {string}  createdAt
 * @property {number}  unreadCount
 * @property {string|null} lastMessage
 * @property {string|null} lastMessageAt
 * @property {string|null} otherUserId
 * @property {string|null} otherUserName
 */

/**
 * @typedef {Object} ChatDetailsDto
 * @property {string} id
 * @property {string|null} name
 * @property {string} type
 * @property {string|null} groupImageUrl
 * @property {string} createdAt
 * @property {number} participantsCount
 * @property {ChatParticipantDto[]} participants
 */

/**
 * @typedef {Object} ChatParticipantDto
 * @property {string} userId
 * @property {string} userName
 * @property {string} fullName
 * @property {string|null} profilePictureUrl
 * @property {boolean} isOnline
 */

/**
 * @typedef {Object} MessageDto
 * @property {string}  id
 * @property {string}  chatId
 * @property {string}  senderId
 * @property {string}  senderName
 * @property {string}  message
 * @property {string}  type
 * @property {string}  sentAt
 * @property {boolean} isEdited
 * @property {boolean} isDeleted
 * @property {string}  status
 */

/**
 * @typedef {Object} ApiResponse
 * @property {boolean} success
 * @property {string}  message
 * @property {*}       data
 * @property {string[]|null} errors
 */
