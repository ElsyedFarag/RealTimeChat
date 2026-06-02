import * as signalR from '@microsoft/signalr'

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5113'
const HUB_URL = `${BASE_URL}/hubs/chat`

class SignalRService {
  constructor() {
    this.connection = null
    this.listeners = new Map()
  }

  async connect(token) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return
    if (this.connection?.state === signalR.HubConnectionState.Connecting) return

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => localStorage.getItem('accessToken') || token,
        skipNegotiation: false,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    this.connection.onreconnected(() => {
      console.log('[SignalR] Reconnected')
      this.listeners.forEach((handlers, event) => {
        handlers.forEach((handler) => this.connection.on(event, handler))
      })
    })

    this.connection.onclose((err) => {
      console.warn('[SignalR] Connection closed', err)
    })

    await this.connection.start()
    console.log('[SignalR] Connected')
  }

  async disconnect() {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
    }
    this.listeners.clear()
  }

  on(event, handler) {
    if (!this.connection) return
    this.connection.on(event, handler)
    if (!this.listeners.has(event)) this.listeners.set(event, new Set())
    this.listeners.get(event).add(handler)
  }

  off(event, handler) {
    if (!this.connection) return
    this.connection.off(event, handler)
    this.listeners.get(event)?.delete(handler)
  }

  _invoke(method, ...args) {
    if (this.connection?.state !== signalR.HubConnectionState.Connected) return Promise.resolve()
    return this.connection.invoke(method, ...args).catch((err) => {
      console.warn(`[SignalR] ${method} error:`, err)
    })
  }

  sendMessage(dto)                           { return this._invoke('SendMessage', dto) }
  editMessage(messageId, newContent)         { return this._invoke('EditMessage', messageId, newContent) }
  deleteMessage(messageId, chatId)           { return this._invoke('DeleteMessage', messageId, chatId) }
  markAsSeen(messageId, chatId)              { return this._invoke('MarkAsSeen', messageId, chatId) }
  startTyping(chatId)                        { return this._invoke('StartTyping', chatId) }
  stopTyping(chatId)                         { return this._invoke('StopTyping', chatId) }
  joinChat(chatId)                           { return this._invoke('JoinChat', chatId) }
  leaveChat(chatId)                          { return this._invoke('LeaveChat', chatId) }
  notifyGroupCreated(chatId, summary, ids)   { return this._invoke('NotifyGroupCreated', chatId, summary, ids) }
  notifyMemberAdded(chatId, userId, member)  { return this._invoke('NotifyMemberAdded', chatId, userId, member) }
  notifyMemberRemoved(chatId, userId)        { return this._invoke('NotifyMemberRemoved', chatId, userId) }

  get isConnected() {
    return this.connection?.state === signalR.HubConnectionState.Connected
  }
}

export const signalRService = new SignalRService()
