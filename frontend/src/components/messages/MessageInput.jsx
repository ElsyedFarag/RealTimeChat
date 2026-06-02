import { useState, useRef } from 'react'
import { Send } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useChat } from '../../contexts/ChatContext'

export default function MessageInput({ chatId }) {
  const { t } = useTranslation()
  const [message, setMessage] = useState('')
  const { sendMessage, notifyTyping } = useChat()
  const inputRef = useRef(null)

  const handleSend = () => {
    const trimmed = message.trim()
    if (!trimmed || !chatId) return
    sendMessage(chatId, trimmed)
    setMessage('')
    // Reset height
    if (inputRef.current) {
      inputRef.current.style.height = 'auto'
    }
    inputRef.current?.focus()
  }

  const handleKeyDown = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  const handleChange = (e) => {
    setMessage(e.target.value)
    // Auto-resize
    e.target.style.height = 'auto'
    e.target.style.height = `${e.target.scrollHeight}px`
    if (e.target.value.trim()) notifyTyping(chatId)
  }

  return (
    <div className="px-4 py-3 bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700">
      <div className="flex items-end gap-2 bg-gray-100 dark:bg-gray-700 rounded-2xl px-4 py-2">
        <textarea
          ref={inputRef}
          rows={1}
          value={message}
          onChange={handleChange}
          onKeyDown={handleKeyDown}
          placeholder={t('chat.typeMessage')}
          className="flex-1 bg-transparent resize-none outline-none text-sm text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500 max-h-32 leading-relaxed"
          style={{ minHeight: '24px' }}
        />
        <button
          onClick={handleSend}
          disabled={!message.trim()}
          className={`p-2 rounded-full transition-all flex-shrink-0
            ${message.trim()
              ? 'bg-brand-500 text-white hover:bg-brand-600 shadow'
              : 'bg-gray-300 dark:bg-gray-600 text-gray-400 dark:text-gray-500 cursor-not-allowed'
            }`}
        >
          <Send size={16} />
        </button>
      </div>
    </div>
  )
}
