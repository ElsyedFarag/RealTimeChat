import { useState } from 'react'
import { format } from 'date-fns'
import { Check, CheckCheck, MoreVertical, Edit2, Trash2 } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useChat } from '../../contexts/ChatContext'

export default function MessageBubble({ message, isOwn, showSender, chatId }) {
  const { t } = useTranslation()
  const { editMessage, deleteMessage } = useChat()
  const [editing, setEditing] = useState(false)
  const [editContent, setEditContent] = useState(message.message)
  const [showMenu, setShowMenu] = useState(false)

  const handleEdit = () => {
    if (editContent.trim()) {
      editMessage(message.id, editContent.trim())
    }
    setEditing(false)
  }

  const handleDelete = () => {
    deleteMessage(message.id, chatId)
    setShowMenu(false)
  }

  const StatusIcon = () => {
    if (!isOwn) return null
    if (message.status === 'Seen') return <CheckCheck size={14} className="text-blue-400" />
    if (message.status === 'Delivered') return <CheckCheck size={14} className="text-gray-400 dark:text-gray-500" />
    return <Check size={14} className="text-gray-400 dark:text-gray-500" />
  }

  return (
    <div className={`flex ${isOwn ? 'justify-end' : 'justify-start'} group mb-0.5`}>
      <div className={`max-w-xs md:max-w-md lg:max-w-lg relative ${isOwn ? 'items-end' : 'items-start'} flex flex-col`}>
        {showSender && !isOwn && (
          <span className="text-xs font-medium text-brand-600 dark:text-brand-400 mb-0.5 ms-1">
            {message.senderName}
          </span>
        )}
        <div className="relative flex items-end gap-1">
          {isOwn && (
            <div className="opacity-0 group-hover:opacity-100 transition-opacity relative">
              <button
                onClick={() => setShowMenu(v => !v)}
                className="p-1 rounded-full hover:bg-gray-200 dark:hover:bg-gray-600"
              >
                <MoreVertical size={14} className="text-gray-500 dark:text-gray-400" />
              </button>
              {showMenu && (
                <div className="absolute end-6 bottom-0 z-50 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl shadow-lg py-1 w-36">
                  <button
                    onClick={() => { setEditing(true); setShowMenu(false) }}
                    className="w-full text-start px-3 py-2 text-xs text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 flex items-center gap-2"
                  >
                    <Edit2 size={12} /> {t('message.edit')}
                  </button>
                  <button
                    onClick={handleDelete}
                    className="w-full text-start px-3 py-2 text-xs text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 flex items-center gap-2"
                  >
                    <Trash2 size={12} /> {t('message.delete')}
                  </button>
                </div>
              )}
            </div>
          )}
          <div className={`px-3 py-2 rounded-2xl text-sm shadow-sm max-w-full break-words
            ${isOwn
              ? 'bg-brand-500 text-white rounded-br-sm dark:bg-brand-600'
              : 'bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 border border-gray-100 dark:border-gray-600 rounded-bl-sm'
            }
            ${message.isDeleted ? 'opacity-60 italic' : ''}`}
          >
            {editing ? (
              <div className="flex gap-2 items-center">
                <input
                  value={editContent}
                  onChange={e => setEditContent(e.target.value)}
                  onKeyDown={e => {
                    if (e.key === 'Enter') handleEdit()
                    if (e.key === 'Escape') setEditing(false)
                  }}
                  className="bg-transparent border-b border-white/60 outline-none flex-1 text-white text-sm min-w-0"
                  autoFocus
                />
                <button onClick={handleEdit} className="text-xs underline flex-shrink-0">
                  {t('message.save')}
                </button>
              </div>
            ) : (
              <p className="whitespace-pre-wrap break-words leading-relaxed">
                {message.isDeleted ? t('message.deleted') : message.message}
              </p>
            )}
            {message.isEdited && !message.isDeleted && (
              <span className={`text-xs opacity-60 ${isOwn ? 'text-white' : 'text-gray-400 dark:text-gray-500'}`}>
                {' '}{t('message.edited')}
              </span>
            )}
          </div>
        </div>
        <div className={`flex items-center gap-1 mt-0.5 px-1 ${isOwn ? 'flex-row-reverse' : ''}`}>
          <span className="text-xs text-gray-400 dark:text-gray-500">
            {message.sentAt ? format(new Date(message.sentAt), 'HH:mm') : ''}
          </span>
          <StatusIcon />
        </div>
      </div>
    </div>
  )
}
