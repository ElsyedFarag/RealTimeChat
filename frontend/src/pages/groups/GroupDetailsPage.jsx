import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, UserPlus, UserMinus, Users, Search, X, Check, Crown, MessageSquare } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { chatsApi } from '../../api/chatsApi'
import { usersApi } from '../../api/usersApi'
import { useChat } from '../../contexts/ChatContext'
import { useAuth } from '../../contexts/AuthContext'
import useDebounce from '../../hooks/useDebounce'
import Avatar from '../../components/common/Avatar'
import Spinner from '../../components/common/Spinner'
import Modal from '../../components/common/Modal'
import toast from 'react-hot-toast'

export default function GroupDetailsPage() {
  const { t } = useTranslation()
  const { groupId } = useParams()
  const navigate = useNavigate()
  const { user } = useAuth()
  const { addMembersToGroup, removeMemberFromGroup, loadChats } = useChat()
  const [group, setGroup] = useState(null)
  const [loading, setLoading] = useState(true)
  const [showAddModal, setShowAddModal] = useState(false)
  const [search, setSearch] = useState('')
  const [searchResults, setSearchResults] = useState([])
  const [toAdd, setToAdd] = useState([])
  const [searchLoading, setSearchLoading] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const debounced = useDebounce(search, 300)

  const fetchGroup = async () => {
    try {
      const { data } = await chatsApi.getById(groupId)
      if (data.success) setGroup(data.data)
    } catch {
      toast.error(t('groups.loadFailed'))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { fetchGroup() }, [groupId]) // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    if (!debounced.trim()) { setSearchResults([]); return }
    let cancelled = false
    setSearchLoading(true)
    usersApi.search(debounced)
      .then(({ data }) => {
        if (!cancelled && data.success) {
          const existingIds = new Set((group?.participants || []).map(p => p.userId))
          setSearchResults((data.data || []).filter(u => !existingIds.has(u.id)))
        }
      })
      .catch(() => {})
      .finally(() => { if (!cancelled) setSearchLoading(false) })
    return () => { cancelled = true }
  }, [debounced, group])

  const handleAdd = async () => {
    if (!toAdd.length) return
    setSubmitting(true)
    try {
      await addMembersToGroup(groupId, toAdd.map(u => u.id))
      toast.success(t('groups.addMembersSuccess'))
      setShowAddModal(false)
      setToAdd([])
      setSearch('')
      await fetchGroup()
      await loadChats()
    } catch {
      toast.error(t('groups.addMembersFailed'))
    } finally {
      setSubmitting(false)
    }
  }

  const handleRemove = async (userId, name) => {
    if (!window.confirm(t('groups.removeMemberConfirm', { name }))) return
    try {
      await removeMemberFromGroup(groupId, userId)
      toast.success(t('groups.removeMemberSuccess'))
      await fetchGroup()
      await loadChats()
    } catch {
      toast.error(t('groups.removeMemberFailed'))
    }
  }

  const isAdmin = group?.participants?.find(p => p.userId === user?.id)?.isAdmin

  if (loading) {
    return <div className="flex-1 flex items-center justify-center bg-gray-50 dark:bg-gray-900"><Spinner /></div>
  }
  if (!group) {
    return (
      <div className="flex-1 flex items-center justify-center bg-gray-50 dark:bg-gray-900 text-gray-400 dark:text-gray-500">
        {t('groups.notFound')}
      </div>
    )
  }

  return (
    <div className="flex-1 flex flex-col bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <div className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-4 py-4 flex items-center gap-3">
        <button
          onClick={() => navigate(-1)}
          className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-full transition-colors"
        >
          <ArrowLeft size={20} className="text-gray-600 dark:text-gray-400" />
        </button>
        <h1 className="font-semibold text-gray-900 dark:text-gray-100 flex-1">{t('groups.groupInfo')}</h1>
        <button
          onClick={() => navigate(`/chat/${groupId}`)}
          className="flex items-center gap-1.5 px-3 py-1.5 bg-brand-500 dark:bg-brand-600 text-white text-xs rounded-full hover:bg-brand-600 dark:hover:bg-brand-700 transition-colors"
        >
          <MessageSquare size={13} /> {t('groups.openChat')}
        </button>
      </div>

      <div className="flex-1 overflow-y-auto max-w-lg mx-auto w-full px-4 py-6 space-y-5 scrollbar-thin">
        {/* Group Avatar */}
        <div className="bg-white dark:bg-gray-800 rounded-2xl p-6 shadow-sm border border-gray-100 dark:border-gray-700 flex flex-col items-center text-center">
          <div className="w-20 h-20 rounded-full bg-gradient-to-br from-purple-500 to-indigo-500 flex items-center justify-center mb-3">
            <Users size={32} className="text-white" />
          </div>
          <h2 className="text-xl font-bold text-gray-900 dark:text-gray-100">{group.name}</h2>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            {t('groups.memberCount', { count: group.participantsCount })}
          </p>
          <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">
            {t('groups.created')} {new Date(group.createdAt).toLocaleDateString()}
          </p>
        </div>

        {/* Members List */}
        <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-100 dark:border-gray-700 overflow-hidden">
          <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100 dark:border-gray-700">
            <h3 className="font-medium text-gray-900 dark:text-gray-100 text-sm">
              {group.participants?.length} {t('groups.members')}
            </h3>
            {isAdmin && (
              <button
                onClick={() => setShowAddModal(true)}
                className="flex items-center gap-1.5 text-xs text-brand-600 dark:text-brand-400 font-medium hover:text-brand-700 dark:hover:text-brand-300"
              >
                <UserPlus size={14} /> {t('groups.addMembers')}
              </button>
            )}
          </div>
          <div>
            {(group.participants || []).map(p => (
              <div
                key={p.userId}
                className="flex items-center gap-3 px-5 py-3 border-b border-gray-50 dark:border-gray-700/50 last:border-0 hover:bg-gray-50 dark:hover:bg-gray-750 transition-colors"
              >
                <Avatar name={p.fullName || p.userName} size="md" />
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-1.5">
                    <p className="font-medium text-sm text-gray-900 dark:text-gray-100 truncate">
                      {p.fullName || p.userName}
                    </p>
                    {p.isAdmin && <Crown size={12} className="text-amber-500 flex-shrink-0" />}
                  </div>
                  <p className="text-xs text-gray-400 dark:text-gray-500">@{p.userName}</p>
                </div>
                {isAdmin && p.userId !== user?.id && (
                  <button
                    onClick={() => handleRemove(p.userId, p.fullName || p.userName)}
                    className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-full transition-colors"
                  >
                    <UserMinus size={15} />
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Add Members Modal */}
      {showAddModal && (
        <Modal title={t('groups.addMembers')} onClose={() => { setShowAddModal(false); setToAdd([]); setSearch('') }}>
          <div className="space-y-3">
            {toAdd.length > 0 && (
              <div className="flex flex-wrap gap-2">
                {toAdd.map(u => (
                  <span key={u.id} className="flex items-center gap-1 bg-brand-50 dark:bg-brand-900/20 text-brand-700 dark:text-brand-400 rounded-full px-3 py-1 text-xs font-medium">
                    {u.fullName || u.userName}
                    <button onClick={() => setToAdd(prev => prev.filter(x => x.id !== u.id))}>
                      <X size={11} />
                    </button>
                  </span>
                ))}
              </div>
            )}
            <div className="relative">
              <Search size={14} className="absolute start-3 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                value={search}
                onChange={e => setSearch(e.target.value)}
                autoFocus
                placeholder={t('groups.searchPlaceholder')}
                className="w-full ps-9 pe-3 py-2.5 border border-gray-200 dark:border-gray-600 rounded-xl text-sm outline-none focus:ring-2 focus:ring-brand-300 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500"
              />
            </div>
            <div className="max-h-52 overflow-y-auto scrollbar-thin">
              {searchLoading ? (
                <div className="flex justify-center py-4"><Spinner size="sm" /></div>
              ) : (
                searchResults.map(u => {
                  const isSelected = toAdd.some(x => x.id === u.id)
                  return (
                    <button
                      key={u.id}
                      onClick={() => setToAdd(prev => isSelected ? prev.filter(x => x.id !== u.id) : [...prev, u])}
                      className={`w-full flex items-center gap-3 p-3 rounded-xl transition-colors text-start ${isSelected ? 'bg-brand-50 dark:bg-brand-900/20' : 'hover:bg-gray-50 dark:hover:bg-gray-700'}`}
                    >
                      <Avatar name={u.fullName || u.userName} size="sm" />
                      <p className="text-sm font-medium text-gray-900 dark:text-gray-100 flex-1">{u.fullName || u.userName}</p>
                      {isSelected && <Check size={15} className="text-brand-500" />}
                    </button>
                  )
                })
              )}
            </div>
            <button
              onClick={handleAdd}
              disabled={!toAdd.length || submitting}
              className="w-full py-2.5 bg-brand-500 dark:bg-brand-600 text-white rounded-xl text-sm font-medium disabled:opacity-50 hover:bg-brand-600 dark:hover:bg-brand-700 transition-colors"
            >
              {submitting
                ? t('groups.adding')
                : `${t('groups.addMembersBtn')} (${toAdd.length})`
              }
            </button>
          </div>
        </Modal>
      )}
    </div>
  )
}
