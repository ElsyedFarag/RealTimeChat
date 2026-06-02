import { createContext, useCallback, useContext, useEffect, useReducer, useRef } from 'react'
import { chatsApi } from '../api/chatsApi'
import { messagesApi } from '../api/messagesApi'
import { signalRService } from '../services/signalrService'
import { useAuth } from './AuthContext'

const ChatContext = createContext(null)

const initialState = {
  chats: [],
  activeChatId: null,
  messages: {},
  typingUsers: {},
  onlineUsers: {},
  lastSeenAt: {},
  loadingChats: false,
  loadingMessages: {},
  pagination: {},
}

function reducer(state, action) {
  switch (action.type) {
    case 'SET_LOADING_CHATS':
      return { ...state, loadingChats: action.payload }
    case 'SET_CHATS':
      return { ...state, chats: action.payload }
    case 'ADD_OR_UPDATE_CHAT': {
      const exists = state.chats.find((c) => c.id === action.payload.id)
      if (exists) {
        return { ...state, chats: state.chats.map((c) => c.id === action.payload.id ? { ...c, ...action.payload } : c) }
      }
      return { ...state, chats: [action.payload, ...state.chats] }
    }
    case 'REMOVE_CHAT':
      return { ...state, chats: state.chats.filter((c) => c.id !== action.payload) }
    case 'SET_ACTIVE_CHAT':
      return { ...state, activeChatId: action.payload }
    case 'SET_MESSAGES': {
      const { chatId, messages, page, hasMore } = action.payload
      return {
        ...state,
        messages: { ...state.messages, [chatId]: messages },
        pagination: { ...state.pagination, [chatId]: { page, hasMore } },
        loadingMessages: { ...state.loadingMessages, [chatId]: false },
      }
    }
    case 'PREPEND_MESSAGES': {
      const { chatId, messages, page, hasMore } = action.payload
      const existing = state.messages[chatId] || []
      const existingIds = new Set(existing.map((m) => m.id))
      const newMsgs = messages.filter((m) => !existingIds.has(m.id))
      return {
        ...state,
        messages: { ...state.messages, [chatId]: [...newMsgs, ...existing] },
        pagination: { ...state.pagination, [chatId]: { page, hasMore } },
        loadingMessages: { ...state.loadingMessages, [chatId]: false },
      }
    }
    case 'SET_LOADING_MESSAGES':
      return { ...state, loadingMessages: { ...state.loadingMessages, [action.payload]: true } }
    case 'ADD_MESSAGE': {
      const { chatId, message } = action.payload
      const existing = state.messages[chatId] || []
      if (existing.some((m) => m.id === message.id)) return state
      return {
        ...state,
        messages: { ...state.messages, [chatId]: [...existing, message] },
        chats: state.chats.map((c) =>
          c.id === chatId
            ? { ...c, lastMessage: message.message, lastMessageAt: message.sentAt, unreadCount: c.id === state.activeChatId ? 0 : (c.unreadCount || 0) + 1 }
            : c
        ),
      }
    }
    case 'UPDATE_MESSAGE': {
      const { chatId, message } = action.payload
      const msgs = state.messages[chatId] || []
      return { ...state, messages: { ...state.messages, [chatId]: msgs.map((m) => m.id === message.id ? { ...m, ...message } : m) } }
    }
    case 'DELETE_MESSAGE': {
      const { chatId, messageId } = action.payload
      const msgs = state.messages[chatId] || []
      return { ...state, messages: { ...state.messages, [chatId]: msgs.map((m) => m.id === messageId ? { ...m, isDeleted: true, message: 'This message was deleted' } : m) } }
    }
    case 'UPDATE_MESSAGE_STATUS': {
      const { chatId, messageId, status } = action.payload
      const msgs = state.messages[chatId] || []
      return { ...state, messages: { ...state.messages, [chatId]: msgs.map((m) => m.id === messageId ? { ...m, status } : m) } }
    }
    case 'SET_TYPING': {
      const { chatId, userId, fullName } = action.payload
      return { ...state, typingUsers: { ...state.typingUsers, [chatId]: { ...(state.typingUsers[chatId] || {}), [userId]: fullName } } }
    }
    case 'CLEAR_TYPING': {
      const { chatId, userId } = action.payload
      const current = { ...(state.typingUsers[chatId] || {}) }
      delete current[userId]
      return { ...state, typingUsers: { ...state.typingUsers, [chatId]: current } }
    }
    case 'SET_USER_ONLINE':
      return { ...state, onlineUsers: { ...state.onlineUsers, [action.payload.userId]: true } }
    case 'SET_USER_OFFLINE':
      return { ...state, onlineUsers: { ...state.onlineUsers, [action.payload.userId]: false }, lastSeenAt: { ...state.lastSeenAt, [action.payload.userId]: action.payload.lastSeenAt } }
    case 'RESET_UNREAD':
      return { ...state, chats: state.chats.map((c) => c.id === action.payload ? { ...c, unreadCount: 0 } : c) }
    case 'ENSURE_CHAT_IN_LIST': {
      const { chatId, message } = action.payload
      const alreadyExists = state.chats.some((c) => c.id === chatId)
      if (alreadyExists) return state
      const placeholder = { id: chatId, name: message.senderName ?? null, isGroup: false, imageUrl: null, createdAt: message.sentAt, unreadCount: 1, lastMessage: message.message, lastMessageAt: message.sentAt, otherUserId: message.senderId, otherUserName: message.senderName }
      return { ...state, chats: [placeholder, ...state.chats] }
    }
    default:
      return state
  }
}

export function ChatProvider({ children }) {
  const { user } = useAuth()
  const [state, dispatch] = useReducer(reducer, initialState)
  const typingTimers = useRef({})
  const scrollLockRef = useRef(false)

  const loadChats = useCallback(async () => {
    dispatch({ type: 'SET_LOADING_CHATS', payload: true })
    try {
      const { data } = await chatsApi.getMy()
      if (data.success) dispatch({ type: 'SET_CHATS', payload: data.data || [] })
    } finally {
      dispatch({ type: 'SET_LOADING_CHATS', payload: false })
    }
  }, [])

  const loadMessages = useCallback(async (chatId, page = 1) => {
    dispatch({ type: 'SET_LOADING_MESSAGES', payload: chatId })
    try {
      const { data } = await messagesApi.getByChatId(chatId, page, 20)
      if (data.success) {
        const messages = (data.data || []).slice().reverse()
        const hasMore = (data.data || []).length === 20
        if (page === 1) {
          dispatch({ type: 'SET_MESSAGES', payload: { chatId, messages, page, hasMore } })
        } else {
          dispatch({ type: 'PREPEND_MESSAGES', payload: { chatId, messages, page, hasMore } })
        }
      }
    } catch { /* handled by interceptor */ }
  }, [])

  const loadMoreMessages = useCallback(async (chatId) => {
    if (scrollLockRef.current) return
    const pagination = state.pagination[chatId]
    if (!pagination?.hasMore || state.loadingMessages[chatId]) return
    scrollLockRef.current = true
    await loadMessages(chatId, pagination.page + 1)
    setTimeout(() => { scrollLockRef.current = false }, 500)
  }, [state.pagination, state.loadingMessages, loadMessages])

  const setActiveChat = useCallback(async (chatId) => {
    dispatch({ type: 'SET_ACTIVE_CHAT', payload: chatId })
    dispatch({ type: 'RESET_UNREAD', payload: chatId })
    if (chatId && !state.messages[chatId]) {
      await loadMessages(chatId)
    }
    if (chatId && signalRService.isConnected) {
      signalRService.joinChat(chatId)
    }
  }, [state.messages, loadMessages])

  const createPrivateChat = useCallback(async (userId, targetUser = null) => {
    const { data } = await chatsApi.createPrivate(userId)
    if (data.success) {
      const chatDto = data.data
      const chatListItem = {
        id: chatDto.id, name: chatDto.name ?? null, isGroup: chatDto.type === 'Group',
        imageUrl: chatDto.groupImageUrl ?? null, createdAt: chatDto.createdAt,
        unreadCount: 0, lastMessage: null, lastMessageAt: null,
        otherUserId: targetUser?.id ?? userId, otherUserName: targetUser?.fullName ?? targetUser?.userName ?? null,
      }
      dispatch({ type: 'ADD_OR_UPDATE_CHAT', payload: chatListItem })
      return chatDto
    }
  }, [])

  const createGroupChat = useCallback(async (dto) => {
    const { data } = await chatsApi.createGroup(dto)
    if (data.success) {
      const chatDetails = data.data
      const chatListItem = {
        id: chatDetails.id, name: chatDetails.name, isGroup: true,
        imageUrl: chatDetails.groupImageUrl ?? null, createdAt: chatDetails.createdAt,
        unreadCount: 0, lastMessage: null, lastMessageAt: null,
      }
      dispatch({ type: 'ADD_OR_UPDATE_CHAT', payload: chatListItem })
      if (signalRService.isConnected) {
        const memberIds = chatDetails.participants.map(p => p.userId)
        await signalRService.notifyGroupCreated(chatDetails.id, chatListItem, memberIds)
      }
      return chatDetails
    }
  }, [])

  const addMembersToGroup = useCallback(async (chatId, userIds) => {
    const { data } = await chatsApi.addMembers(chatId, userIds)
    if (data.success) {
      if (signalRService.isConnected) {
        for (const uid of userIds) {
          await signalRService.notifyMemberAdded(chatId, uid, { userId: uid })
        }
      }
      return data.data
    }
  }, [])

  const removeMemberFromGroup = useCallback(async (chatId, userId) => {
    const { data } = await chatsApi.removeMember(chatId, userId)
    if (data.success) {
      if (signalRService.isConnected) {
        await signalRService.notifyMemberRemoved(chatId, userId)
      }
      return data.data
    }
  }, [])

  const sendMessage = useCallback((chatId, message) => {
    signalRService.sendMessage({ chatId, message })
  }, [])

  const editMessage = useCallback((messageId, newContent) => {
    signalRService.editMessage(messageId, newContent)
  }, [])

  const deleteMessage = useCallback((messageId, chatId) => {
    signalRService.deleteMessage(messageId, chatId)
  }, [])

  const markSeen = useCallback((messageId, chatId) => {
    signalRService.markAsSeen(messageId, chatId)
  }, [])

  const notifyTyping = useCallback((chatId) => {
    if (!signalRService.isConnected) return
    signalRService.startTyping(chatId)
    const key = `${chatId}`
    clearTimeout(typingTimers.current[key])
    typingTimers.current[key] = setTimeout(() => {
      signalRService.stopTyping(chatId)
    }, 1500)
  }, [])

  // SignalR event listeners
  useEffect(() => {
    if (!user) return

    const onReceiveMessage = (message) => {
      dispatch({ type: 'ADD_MESSAGE', payload: { chatId: message.chatId, message } })
      dispatch({ type: 'ENSURE_CHAT_IN_LIST', payload: { chatId: message.chatId, message } })
    }
    const onMessageEdited = (message) => dispatch({ type: 'UPDATE_MESSAGE', payload: { chatId: message.chatId, message } })
    const onMessageDeleted = ({ messageId, chatId }) => dispatch({ type: 'DELETE_MESSAGE', payload: { chatId, messageId } })
    const onMessageDelivered = ({ messageId, chatId }) => dispatch({ type: 'UPDATE_MESSAGE_STATUS', payload: { chatId, messageId, status: 'Delivered' } })
    const onMessageSeen = ({ messageId, chatId }) => dispatch({ type: 'UPDATE_MESSAGE_STATUS', payload: { chatId, messageId, status: 'Seen' } })
    const onUserTyping = ({ chatId, userId, fullName }) => dispatch({ type: 'SET_TYPING', payload: { chatId, userId, fullName } })
    const onUserStoppedTyping = ({ chatId, userId }) => dispatch({ type: 'CLEAR_TYPING', payload: { chatId, userId } })
    const onUserOnline = (payload) => dispatch({ type: 'SET_USER_ONLINE', payload })
    const onUserOffline = (payload) => dispatch({ type: 'SET_USER_OFFLINE', payload })
    const onGroupCreated = (chatSummary) => {
      dispatch({ type: 'ADD_OR_UPDATE_CHAT', payload: { ...chatSummary, isGroup: true, unreadCount: 0 } })
    }
    const onMemberAdded = ({ chatId }) => {
      // Refresh chat details when a member is added
      chatsApi.getById(chatId).then(({ data }) => {
        if (data.success) dispatch({ type: 'ADD_OR_UPDATE_CHAT', payload: { id: chatId, ...data.data } })
      }).catch(() => {})
    }
    const onMemberRemoved = ({ chatId, userId: removedId }) => {
      if (removedId === user?.id) {
        dispatch({ type: 'REMOVE_CHAT', payload: chatId })
      }
    }

    signalRService.on('ReceiveMessage', onReceiveMessage)
    signalRService.on('MessageEdited', onMessageEdited)
    signalRService.on('MessageDeleted', onMessageDeleted)
    signalRService.on('MessageDelivered', onMessageDelivered)
    signalRService.on('MessageSeen', onMessageSeen)
    signalRService.on('UserTyping', onUserTyping)
    signalRService.on('UserStoppedTyping', onUserStoppedTyping)
    signalRService.on('UserOnline', onUserOnline)
    signalRService.on('UserOffline', onUserOffline)
    signalRService.on('GroupCreated', onGroupCreated)
    signalRService.on('MemberAdded', onMemberAdded)
    signalRService.on('MemberRemoved', onMemberRemoved)

    return () => {
      signalRService.off('ReceiveMessage', onReceiveMessage)
      signalRService.off('MessageEdited', onMessageEdited)
      signalRService.off('MessageDeleted', onMessageDeleted)
      signalRService.off('MessageDelivered', onMessageDelivered)
      signalRService.off('MessageSeen', onMessageSeen)
      signalRService.off('UserTyping', onUserTyping)
      signalRService.off('UserStoppedTyping', onUserStoppedTyping)
      signalRService.off('UserOnline', onUserOnline)
      signalRService.off('UserOffline', onUserOffline)
      signalRService.off('GroupCreated', onGroupCreated)
      signalRService.off('MemberAdded', onMemberAdded)
      signalRService.off('MemberRemoved', onMemberRemoved)
    }
  }, [user])

  useEffect(() => {
    if (user) loadChats()
  }, [user, loadChats])

  return (
    <ChatContext.Provider value={{
      ...state,
      loadChats, loadMessages, loadMoreMessages, setActiveChat,
      createPrivateChat, createGroupChat, addMembersToGroup, removeMemberFromGroup,
      sendMessage, editMessage, deleteMessage, markSeen, notifyTyping,
    }}>
      {children}
    </ChatContext.Provider>
  )
}

export const useChat = () => {
  const ctx = useContext(ChatContext)
  if (!ctx) throw new Error('useChat must be used within ChatProvider')
  return ctx
}
