import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { MoreVertical, Users, Settings, ArrowLeft } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useChat } from '../../contexts/ChatContext'
import { useAuth } from '../../contexts/AuthContext'
import Avatar from '../common/Avatar'

export default function ChatHeader({ chatId }) {
  const { t } = useTranslation()
  const { chats, typingUsers, onlineUsers } = useChat()
  const { user } = useAuth()
  const navigate = useNavigate()
  const [showMenu, setShowMenu] = useState(false)

  const chat = chats.find(c => c.id === chatId)
  if (!chat) return null

  const displayName = chat.isGroup ? chat.name : (chat.otherUserName || chat.name || 'Unknown')
  const isOnline = !chat.isGroup && !!onlineUsers[chat.otherUserId]
  const typingList = Object.values(typingUsers[chatId] || {})
  const isTyping = typingList.length > 0

  const getSubtitle = () => {
    if (isTyping) {
      const names = typingList[0]
      const extra = typingList.length > 1 ? ` +${typingList.length - 1}` : ''
      return `${names}${extra} ${t('chat.typing')}`
    }
    if (chat.isGroup) return t('chat.groupChat')
    return isOnline ? t('chat.online') : t('chat.offline')
  }

  return (
    <div className="flex items-center justify-between px-4 py-3 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 shadow-sm">
      <div className="flex items-center gap-3">
        <button
          onClick={() => navigate(-1)}
          className="md:hidden p-1 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-full"
        >
          <ArrowLeft size={20} className="text-gray-600 dark:text-gray-400" />
        </button>
        {chat.isGroup ? (
          <div className="w-10 h-10 rounded-full bg-gradient-to-br from-purple-500 to-indigo-500 flex items-center justify-center flex-shrink-0">
            <Users size={18} className="text-white" />
          </div>
        ) : (
          <Avatar name={displayName} size="md" online={isOnline} />
        )}
        <div>
          <h2 className="font-semibold text-sm text-gray-900 dark:text-gray-100">{displayName}</h2>
          <p className={`text-xs ${isTyping ? 'text-brand-500 dark:text-brand-400' : 'text-gray-500 dark:text-gray-400'}`}>
            {getSubtitle()}
          </p>
        </div>
      </div>

      <div className="flex items-center gap-1">
        {chat.isGroup && (
          <button
            onClick={() => navigate(`/groups/${chatId}`)}
            className="p-2 rounded-full hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            title={t('chat.groupInfo')}
          >
            <Settings size={18} className="text-gray-500 dark:text-gray-400" />
          </button>
        )}
        <div className="relative">
          <button
            onClick={() => setShowMenu(v => !v)}
            className="p-2 rounded-full hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
          >
            <MoreVertical size={18} className="text-gray-500 dark:text-gray-400" />
          </button>
          {showMenu && (
            <div className="absolute end-0 top-10 z-50 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl shadow-lg py-1 w-44">
              {chat.isGroup && (
                <button
                  onClick={() => { navigate(`/groups/${chatId}`); setShowMenu(false) }}
                  className="w-full text-start px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 flex items-center gap-2"
                >
                  <Users size={14} /> {t('chat.groupInfo')}
                </button>
              )}
              <button
                onClick={() => setShowMenu(false)}
                className="w-full text-start px-4 py-2 text-sm text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20"
              >
                {t('chat.clearChat')}
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
