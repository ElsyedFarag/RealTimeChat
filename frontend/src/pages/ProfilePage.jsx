import { useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import { yupResolver } from '@hookform/resolvers/yup'
import * as yup from 'yup'
import { ArrowLeft, Camera, Mail, User } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../contexts/AuthContext'
import { usersApi } from '../api/usersApi'
import Avatar from '../components/common/Avatar'
import Input from '../components/common/Input'
import Button from '../components/common/Button'
import toast from 'react-hot-toast'
import { formatRelativeTime } from '../utils/index'

export default function ProfilePage() {
  const { t } = useTranslation()
  const { user, updateProfile } = useAuth()
  const navigate = useNavigate()
  const fileRef = useRef(null)
  const [preview, setPreview] = useState(null)
  const [file, setFile] = useState(null)

  const schema = yup.object({
    firstName: yup.string().required(t('auth.validation.firstNameRequired')),
    lastName: yup.string().required(t('auth.validation.lastNameRequired')),
  })

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm({
    resolver: yupResolver(schema),
    defaultValues: { firstName: user?.firstName, lastName: user?.lastName },
  })

  const handleFileChange = (e) => {
    const f = e.target.files[0]
    if (!f) return
    setFile(f)
    setPreview(URL.createObjectURL(f))
  }

  const onSubmit = async (formData) => {
    try {
      const fd = new FormData()
      fd.append('firstName', formData.firstName)
      fd.append('lastName', formData.lastName)
      if (file) fd.append('profilePicture', file)

      const { data } = await usersApi.updateUser(user.id, fd)
      if (data.success) {
        updateProfile(data.data)
        toast.success(t('profile.updateSuccess'))
        setFile(null)
        setPreview(null)
      }
    } catch (err) {
      toast.error(err.message || t('profile.updateFailed'))
    }
  }

  return (
    <div className="flex-1 flex flex-col overflow-hidden bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <div className="flex items-center gap-3 px-6 py-4 bg-white dark:bg-gray-800 border-b border-gray-100 dark:border-gray-700 shadow-sm">
        <button
          onClick={() => navigate(-1)}
          className="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-500 dark:text-gray-400"
        >
          <ArrowLeft size={18} />
        </button>
        <h1 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{t('profile.title')}</h1>
      </div>

      <div className="flex-1 overflow-y-auto scrollbar-thin">
        <div className="max-w-lg mx-auto p-6 space-y-6">
          {/* Avatar card */}
          <div className="bg-white dark:bg-gray-800 rounded-2xl p-6 shadow-sm border border-gray-100 dark:border-gray-700 text-center">
            <div className="relative inline-block mb-4">
              <Avatar name={user?.fullName} url={preview || user?.profilePictureUrl} size="xl" />
              <button
                onClick={() => fileRef.current?.click()}
                className="absolute bottom-0 end-0 p-1.5 bg-brand-600 text-white rounded-full shadow hover:bg-brand-700 transition"
              >
                <Camera size={14} />
              </button>
              <input ref={fileRef} type="file" accept="image/*" className="hidden" onChange={handleFileChange} />
            </div>
            <h2 className="text-xl font-bold text-gray-900 dark:text-gray-100">{user?.fullName}</h2>
            <p className="text-gray-400 dark:text-gray-500 text-sm">@{user?.userName}</p>
            {user?.lastSeenAt && (
              <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">
                {t('profile.lastSeen')} {formatRelativeTime(user.lastSeenAt)}
              </p>
            )}
          </div>

          {/* Account info */}
          <div className="bg-white dark:bg-gray-800 rounded-2xl p-6 shadow-sm border border-gray-100 dark:border-gray-700">
            <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-4">{t('profile.accountInfo')}</h3>
            <div className="space-y-3">
              <div className="flex items-center gap-3 text-sm text-gray-600 dark:text-gray-400">
                <Mail size={16} className="text-gray-400 dark:text-gray-500 flex-shrink-0" />
                {user?.email}
              </div>
              <div className="flex items-center gap-3 text-sm text-gray-600 dark:text-gray-400">
                <User size={16} className="text-gray-400 dark:text-gray-500 flex-shrink-0" />
                @{user?.userName}
              </div>
            </div>
          </div>

          {/* Edit form */}
          <div className="bg-white dark:bg-gray-800 rounded-2xl p-6 shadow-sm border border-gray-100 dark:border-gray-700">
            <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-4">{t('profile.editProfile')}</h3>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <Input
                  label={t('auth.firstName')}
                  error={errors.firstName?.message}
                  {...register('firstName')}
                />
                <Input
                  label={t('auth.lastName')}
                  error={errors.lastName?.message}
                  {...register('lastName')}
                />
              </div>
              <Button type="submit" loading={isSubmitting} className="w-full">
                {t('profile.saveChanges')}
              </Button>
            </form>
          </div>
        </div>
      </div>
    </div>
  )
}
