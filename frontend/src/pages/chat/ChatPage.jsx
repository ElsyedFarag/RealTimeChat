import { useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { MessageSquare } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useChat } from '../../contexts/ChatContext'
import ChatHeader from '../../components/chat/ChatHeader'
import MessageList from '../../components/messages/MessageList'
import MessageInput from '../../components/messages/MessageInput'

export default function ChatPage() {
  const { t } = useTranslation()
  const { chatId } = useParams()
  const { setActiveChat, activeChatId } = useChat()

  useEffect(() => {
    if (chatId && chatId !== activeChatId) {
      setActiveChat(chatId)
    }
  }, [chatId]) // eslint-disable-line react-hooks/exhaustive-deps

  if (!chatId) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center bg-gray-50 dark:bg-gray-900 text-center p-6">
        <div className="w-24 h-24 rounded-full bg-brand-50 dark:bg-brand-900/20 flex items-center justify-center mb-4">
          <MessageSquare size={40} className="text-brand-400 dark:text-brand-500" />
        </div>
        <h2 className="text-xl font-semibold text-gray-700 dark:text-gray-300 mb-2">
          {t('chat.welcomeTitle')}
        </h2>
        <p className="text-sm text-gray-400 dark:text-gray-500 max-w-xs">
          {t('chat.selectChat')}
        </p>
      </div>
    )
  }

  return (
    <div className="flex-1 flex flex-col overflow-hidden bg-white dark:bg-gray-800">
      <ChatHeader chatId={chatId} />
      <MessageList chatId={chatId} />
      <MessageInput chatId={chatId} />
    </div>
  )
}
