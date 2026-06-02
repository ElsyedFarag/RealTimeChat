import { useEffect, useRef, useCallback } from 'react'
import { useTranslation } from 'react-i18next'
import { useChat } from '../../contexts/ChatContext'
import { useAuth } from '../../contexts/AuthContext'
import MessageBubble from './MessageBubble'
import Spinner from '../common/Spinner'

export default function MessageList({ chatId }) {
  const { t } = useTranslation()
  const { messages, loadingMessages, loadMoreMessages, pagination, markSeen } = useChat()
  const { user } = useAuth()
  const bottomRef = useRef(null)
  const containerRef = useRef(null)
  const msgs = messages[chatId] || []
  const loading = loadingMessages[chatId]
  const hasMore = pagination[chatId]?.hasMore

  // Scroll to bottom on new messages
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [msgs.length])

  // Mark last message as seen
  useEffect(() => {
    const lastMsg = msgs[msgs.length - 1]
    if (lastMsg && lastMsg.senderId !== user?.id && lastMsg.status !== 'Seen') {
      markSeen(lastMsg.id, chatId)
    }
  }, [msgs, chatId, user, markSeen])

  const onScroll = useCallback(() => {
    if (containerRef.current?.scrollTop === 0 && hasMore && !loading) {
      loadMoreMessages(chatId)
    }
  }, [hasMore, loading, loadMoreMessages, chatId])

  return (
    <div
      ref={containerRef}
      onScroll={onScroll}
      className="flex-1 overflow-y-auto px-4 py-3 space-y-1 bg-gray-50 dark:bg-gray-900 scrollbar-thin"
    >
      {loading && (
        <div className="flex justify-center py-3"><Spinner size="sm" /></div>
      )}
      {msgs.length === 0 && !loading && (
        <div className="flex flex-col items-center justify-center h-full text-gray-400 dark:text-gray-500">
          <p className="text-sm">{t('chat.noMessages')}</p>
        </div>
      )}
      {msgs.map((msg, i) => {
        const isOwn = msg.senderId === user?.id
        const prev = msgs[i - 1]
        const showSender = !isOwn && (!prev || prev.senderId !== msg.senderId)
        return (
          <MessageBubble
            key={msg.id}
            message={msg}
            isOwn={isOwn}
            showSender={showSender}
            chatId={chatId}
          />
        )
      })}
      <div ref={bottomRef} />
    </div>
  )
}
