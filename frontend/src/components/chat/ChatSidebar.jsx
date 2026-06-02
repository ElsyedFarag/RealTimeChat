import { useState, useMemo } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Search, Users, MessageSquare, LogOut } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../../contexts/AuthContext'
import { useChat } from '../../contexts/ChatContext'
import ChatListItem from './ChatListItem'
import NewChatModal from './NewChatModal'
import Avatar from '../common/Avatar'
import Spinner from '../common/Spinner'
import ThemeLanguageToggle from '../common/ThemeLanguageToggle'

export default function ChatSidebar() {
  const { t } = useTranslation()
  const { user, logout } = useAuth()
  const { chats, loadingChats, onlineUsers } = useChat()
  const navigate = useNavigate()
  const { chatId } = useParams()
  const [search, setSearch] = useState('')
  const [showNewChat, setShowNewChat] = useState(false)
  const [tab, setTab] = useState('all')

  const filtered = useMemo(() => {
    let list = chats
    if (tab === 'private') list = list.filter(c => !c.isGroup)
    if (tab === 'groups') list = list.filter(c => c.isGroup)
    if (search.trim()) {
      const q = search.toLowerCase()
      list = list.filter(c => {
        const name = (c.isGroup ? c.name : c.otherUserName) || ''
        return name.toLowerCase().includes(q)
      })
    }
    return [...list].sort((a, b) => {
      const at = a.lastMessageAt ? new Date(a.lastMessageAt) : new Date(a.createdAt)
      const bt = b.lastMessageAt ? new Date(b.lastMessageAt) : new Date(b.createdAt)
      return bt - at
    })
  }, [chats, search, tab])

  const tabs = [
    ['all', t('sidebar.tabs.all')],
    ['private', t('sidebar.tabs.private')],
    ['groups', t('sidebar.tabs.groups')],
  ]

  return (
    <div className="w-full md:w-80 flex flex-col bg-white dark:bg-gray-800 border-e border-gray-200 dark:border-gray-700 shadow-sm">
      {/* Header */}
      <div className="px-4 py-3 bg-brand-600 dark:bg-brand-700 flex items-center justify-between gap-2">
        <button onClick={() => navigate('/profile')} className="flex items-center gap-2 group min-w-0">
          <Avatar name={user?.fullName || user?.userName} size="sm" online />
          <span className="text-white font-semibold text-sm truncate max-w-[100px]">
            {user?.fullName || user?.userName}
          </span>
        </button>
        <div className="flex items-center gap-0.5 flex-shrink-0">
          <button
            onClick={() => navigate('/groups/create')}
            title={t('sidebar.newGroup')}
            className="p-2 rounded-full text-white hover:bg-white/20 transition-colors"
          >
            <Users size={17} />
          </button>
          <button
            onClick={() => setShowNewChat(true)}
            title={t('sidebar.newChat')}
            className="p-2 rounded-full text-white hover:bg-white/20 transition-colors"
          >
            <MessageSquare size={17} />
          </button>
          <ThemeLanguageToggle />
          <button
            onClick={logout}
            title={t('sidebar.logout')}
            className="p-2 rounded-full text-white hover:bg-white/20 transition-colors"
          >
            <LogOut size={17} />
          </button>
        </div>
      </div>

      {/* Search */}
      <div className="px-3 py-2 border-b border-gray-100 dark:border-gray-700">
        <div className="relative">
          <Search size={15} className="absolute start-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder={t('sidebar.search')}
            className="w-full ps-9 pe-3 py-2 bg-gray-100 dark:bg-gray-700 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500 rounded-full text-sm outline-none focus:ring-2 focus:ring-brand-300 transition"
          />
        </div>
      </div>

      {/* Tabs */}
      <div className="flex border-b border-gray-100 dark:border-gray-700">
        {tabs.map(([key, label]) => (
          <button
            key={key}
            onClick={() => setTab(key)}
            className={`flex-1 py-2 text-xs font-medium transition-colors
              ${tab === key
                ? 'text-brand-600 dark:text-brand-400 border-b-2 border-brand-600 dark:border-brand-400'
                : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300'
              }`}
          >
            {label}
          </button>
        ))}
      </div>

      {/* Chat List */}
      <div className="flex-1 overflow-y-auto scrollbar-thin">
        {loadingChats ? (
          <div className="flex justify-center py-8"><Spinner /></div>
        ) : filtered.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12 text-gray-400 dark:text-gray-500">
            <MessageSquare size={32} className="mb-2 opacity-40" />
            <p className="text-sm">{t('sidebar.empty')}</p>
            {tab === 'groups' && (
              <button
                onClick={() => navigate('/groups/create')}
                className="mt-3 text-brand-600 dark:text-brand-400 text-xs font-medium hover:underline"
              >
                + {t('sidebar.createGroup')}
              </button>
            )}
          </div>
        ) : (
          filtered.map(chat => (
            <ChatListItem
              key={chat.id}
              chat={chat}
              isActive={chat.id === chatId}
              isOnline={!chat.isGroup && !!onlineUsers[chat.otherUserId]}
              onClick={() => navigate(`/chat/${chat.id}`)}
            />
          ))
        )}
      </div>

      {showNewChat && <NewChatModal onClose={() => setShowNewChat(false)} />}
    </div>
  )
}
