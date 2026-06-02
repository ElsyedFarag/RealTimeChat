import { formatDistanceToNow } from 'date-fns'
import { Users } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import Avatar from '../common/Avatar'

export default function ChatListItem({ chat, isActive, isOnline, onClick }) {
  const { t } = useTranslation()
  const displayName = chat.isGroup ? chat.name : (chat.otherUserName || chat.name || 'Unknown')
  const lastMsgTime = chat.lastMessageAt
    ? formatDistanceToNow(new Date(chat.lastMessageAt), { addSuffix: false })
    : ''

  return (
    <button
      onClick={onClick}
      className={`w-full flex items-center gap-3 px-4 py-3 hover:bg-gray-50 dark:hover:bg-gray-750 transition-colors border-b border-gray-50 dark:border-gray-700/50 text-start
        ${isActive ? 'bg-brand-50 dark:bg-brand-900/20 border-s-2 border-s-brand-500' : ''}`}
    >
      <div className="relative flex-shrink-0">
        {chat.isGroup ? (
          <div className="w-10 h-10 rounded-full bg-gradient-to-br from-purple-500 to-indigo-500 flex items-center justify-center">
            <Users size={18} className="text-white" />
          </div>
        ) : (
          <Avatar name={displayName} size="md" online={isOnline} />
        )}
      </div>
      <div className="flex-1 min-w-0">
        <div className="flex items-center justify-between">
          <span className="font-medium text-sm text-gray-900 dark:text-gray-100 truncate">{displayName}</span>
          {lastMsgTime && (
            <span className="text-xs text-gray-400 dark:text-gray-500 ms-1 flex-shrink-0">{lastMsgTime}</span>
          )}
        </div>
        <div className="flex items-center justify-between mt-0.5">
          <span className="text-xs text-gray-500 dark:text-gray-400 truncate">
            {chat.lastMessage || (chat.isGroup ? t('chat.groupChat') : t('chat.startConversation'))}
          </span>
          {chat.unreadCount > 0 && (
            <span className="ms-1 flex-shrink-0 bg-brand-500 text-white text-xs rounded-full w-4 h-4 flex items-center justify-center font-medium">
              {chat.unreadCount > 9 ? '9+' : chat.unreadCount}
            </span>
          )}
        </div>
      </div>
    </button>
  )
}
