import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { yupResolver } from '@hookform/resolvers/yup'
import * as yup from 'yup'
import { Search, X, Check, Users, ArrowLeft } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { usersApi } from '../../api/usersApi'
import { useChat } from '../../contexts/ChatContext'
import useDebounce from '../../hooks/useDebounce'
import Avatar from '../../components/common/Avatar'
import Spinner from '../../components/common/Spinner'
import Button from '../../components/common/Button'
import toast from 'react-hot-toast'

export default function CreateGroupPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { createGroupChat, setActiveChat } = useChat()
  const [search, setSearch] = useState('')
  const [searchResults, setSearchResults] = useState([])
  const [selected, setSelected] = useState([])
  const [searchLoading, setSearchLoading] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const debounced = useDebounce(search, 300)

  const schema = yup.object({
    name: yup.string().min(2, 'Name too short').max(50, 'Name too long').required(t('groups.groupName')),
  })

  const { register, handleSubmit, formState: { errors } } = useForm({ resolver: yupResolver(schema) })

  useEffect(() => {
    if (!debounced.trim()) { setSearchResults([]); return }
    let cancelled = false
    setSearchLoading(true)
    usersApi.search(debounced)
      .then(({ data }) => { if (!cancelled && data.success) setSearchResults(data.data || []) })
      .catch(() => {})
      .finally(() => { if (!cancelled) setSearchLoading(false) })
    return () => { cancelled = true }
  }, [debounced])

  const toggle = (user) => {
    setSelected(prev =>
      prev.find(u => u.id === user.id)
        ? prev.filter(u => u.id !== user.id)
        : [...prev, user]
    )
  }

  const onSubmit = async ({ name }) => {
    if (selected.length < 1) { toast.error(t('groups.addAtLeastOne')); return }
    setSubmitting(true)
    try {
      const chat = await createGroupChat({ name, memberIds: selected.map(u => u.id) })
      if (chat) {
        toast.success(t('groups.createdSuccess', { name }))
        await setActiveChat(chat.id)
        navigate(`/chat/${chat.id}`)
      }
    } catch {
      toast.error(t('groups.createFailed'))
    } finally {
      setSubmitting(false)
    }
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
        <div>
          <h1 className="font-semibold text-gray-900 dark:text-gray-100">{t('groups.create')}</h1>
          <p className="text-xs text-gray-500 dark:text-gray-400">{t('groups.createSubtitle')}</p>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto max-w-lg mx-auto w-full px-4 py-6 space-y-6 scrollbar-thin">
        {/* Group Name */}
        <div className="bg-white dark:bg-gray-800 rounded-2xl p-5 shadow-sm border border-gray-100 dark:border-gray-700">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            {t('groups.groupName')}
          </label>
          <input
            {...register('name')}
            placeholder={t('groups.groupNamePlaceholder')}
            className="w-full border border-gray-200 dark:border-gray-600 rounded-xl px-4 py-3 text-sm outline-none focus:ring-2 focus:ring-brand-300 transition bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500"
          />
          {errors.name && (
            <p className="text-xs text-red-500 dark:text-red-400 mt-1">{errors.name.message}</p>
          )}
        </div>

        {/* Selected Members */}
        {selected.length > 0 && (
          <div className="bg-white dark:bg-gray-800 rounded-2xl p-5 shadow-sm border border-gray-100 dark:border-gray-700">
            <p className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
              {t('groups.selectedMembers')} ({selected.length})
            </p>
            <div className="flex flex-wrap gap-2">
              {selected.map(u => (
                <div key={u.id} className="flex items-center gap-1.5 bg-brand-50 dark:bg-brand-900/20 text-brand-700 dark:text-brand-400 rounded-full px-3 py-1 text-xs font-medium">
                  <span>{u.fullName || u.userName}</span>
                  <button onClick={() => toggle(u)} className="hover:text-brand-900 dark:hover:text-brand-300">
                    <X size={12} />
                  </button>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Search Users */}
        <div className="bg-white dark:bg-gray-800 rounded-2xl p-5 shadow-sm border border-gray-100 dark:border-gray-700">
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
            {t('groups.addMembers')}
          </label>
          <div className="relative mb-3">
            <Search size={15} className="absolute start-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder={t('groups.searchUsers')}
              className="w-full ps-9 pe-3 py-2.5 border border-gray-200 dark:border-gray-600 rounded-xl text-sm outline-none focus:ring-2 focus:ring-brand-300 transition bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500"
            />
          </div>
          <div className="max-h-56 overflow-y-auto scrollbar-thin">
            {searchLoading ? (
              <div className="flex justify-center py-4"><Spinner size="sm" /></div>
            ) : searchResults.length === 0 && debounced ? (
              <p className="text-sm text-gray-400 dark:text-gray-500 text-center py-4">{t('groups.noUsers')}</p>
            ) : (
              searchResults.map(u => {
                const isSelected = selected.some(s => s.id === u.id)
                return (
                  <button
                    key={u.id}
                    onClick={() => toggle(u)}
                    className={`w-full flex items-center gap-3 p-3 rounded-xl transition-colors text-start
                      ${isSelected
                        ? 'bg-brand-50 dark:bg-brand-900/20'
                        : 'hover:bg-gray-50 dark:hover:bg-gray-700'
                      }`}
                  >
                    <Avatar name={u.fullName || u.userName} size="md" />
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-sm text-gray-900 dark:text-gray-100">{u.fullName || u.userName}</p>
                      <p className="text-xs text-gray-400 dark:text-gray-500">@{u.userName}</p>
                    </div>
                    {isSelected && <Check size={16} className="text-brand-500 flex-shrink-0" />}
                  </button>
                )
              })
            )}
          </div>
        </div>

        {/* Submit */}
        <Button
          onClick={handleSubmit(onSubmit)}
          disabled={submitting || selected.length === 0}
          loading={submitting}
          className="w-full py-3"
        >
          <Users size={16} /> {t('groups.createBtn')}
        </Button>
      </div>
    </div>
  )
}
