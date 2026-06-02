import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { usersApi } from '../../api/usersApi'
import { useChat } from '../../contexts/ChatContext'
import useDebounce from '../../hooks/useDebounce'
import Avatar from '../common/Avatar'
import Spinner from '../common/Spinner'
import Modal from '../common/Modal'
import toast from 'react-hot-toast'

export default function NewChatModal({ onClose }) {
  const { t } = useTranslation()
  const [query, setQuery] = useState('')
  const [results, setResults] = useState([])
  const [loading, setLoading] = useState(false)
  const debounced = useDebounce(query, 300)
  const { createPrivateChat, setActiveChat } = useChat()
  const navigate = useNavigate()

  useEffect(() => {
    if (!debounced.trim()) { setResults([]); return }
    let cancelled = false
    setLoading(true)
    usersApi.search(debounced)
      .then(({ data }) => { if (!cancelled && data.success) setResults(data.data || []) })
      .catch(() => {})
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [debounced])

  const handleSelect = async (targetUser) => {
    try {
      const chat = await createPrivateChat(targetUser.id, targetUser)
      if (chat) {
        await setActiveChat(chat.id)
        navigate(`/chat/${chat.id}`)
      }
      onClose()
    } catch {
      toast.error('Failed to start chat')
    }
  }

  return (
    <Modal title={t('newChat.title')} onClose={onClose}>
      <div className="relative mb-3">
        <Search size={15} className="absolute start-3 top-1/2 -translate-y-1/2 text-gray-400" />
        <input
          autoFocus
          value={query}
          onChange={e => setQuery(e.target.value)}
          placeholder={t('newChat.searchPlaceholder')}
          className="w-full ps-9 pe-3 py-2.5 border border-gray-200 dark:border-gray-600 rounded-xl text-sm outline-none focus:ring-2 focus:ring-brand-300 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500"
        />
      </div>
      <div className="mt-1 max-h-64 overflow-y-auto scrollbar-thin">
        {loading ? (
          <div className="flex justify-center py-6"><Spinner /></div>
        ) : results.length === 0 && debounced ? (
          <p className="text-center text-sm text-gray-400 dark:text-gray-500 py-6">{t('newChat.noUsers')}</p>
        ) : (
          results.map(u => (
            <button
              key={u.id}
              onClick={() => handleSelect(u)}
              className="w-full flex items-center gap-3 p-3 hover:bg-gray-50 dark:hover:bg-gray-700 rounded-xl transition-colors text-start"
            >
              <Avatar name={u.fullName || u.userName} size="md" />
              <div>
                <p className="font-medium text-sm text-gray-900 dark:text-gray-100">{u.fullName || u.userName}</p>
                <p className="text-xs text-gray-400 dark:text-gray-500">@{u.userName}</p>
              </div>
            </button>
          ))
        )}
      </div>
    </Modal>
  )
}
