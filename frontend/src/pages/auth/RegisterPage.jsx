import { useForm } from 'react-hook-form'
import { yupResolver } from '@hookform/resolvers/yup'
import * as yup from 'yup'
import { Link, useNavigate } from 'react-router-dom'
import { MessageSquare } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../../contexts/AuthContext'
import Input from '../../components/common/Input'
import Button from '../../components/common/Button'
import ThemeLanguageToggle from '../../components/common/ThemeLanguageToggle'
import toast from 'react-hot-toast'

export default function RegisterPage() {
  const { t } = useTranslation()
  const { register: registerUser } = useAuth()
  const navigate = useNavigate()

  const schema = yup.object({
    firstName: yup.string().required(t('auth.validation.firstNameRequired')),
    lastName: yup.string().required(t('auth.validation.lastNameRequired')),
    userName: yup.string().min(3, t('auth.validation.usernameMin')).required(t('auth.validation.usernameRequired')),
    email: yup.string().email(t('auth.validation.emailInvalid')).required(t('auth.validation.emailRequired')),
    password: yup
      .string()
      .min(8, t('auth.validation.passwordMin8'))
      .matches(/[A-Z]/, t('auth.validation.passwordUppercase'))
      .matches(/[0-9]/, t('auth.validation.passwordNumber'))
      .required(t('auth.validation.passwordRequired')),
  })

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm({
    resolver: yupResolver(schema),
  })

  const onSubmit = async (data) => {
    try {
      await registerUser(data)
      toast.success(t('auth.accountCreated'))
      navigate('/')
    } catch (err) {
      toast.error(err.message || t('auth.registerFailed'))
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-brand-50 via-white to-indigo-50 dark:from-gray-900 dark:via-gray-900 dark:to-gray-800 p-4">
      <div className="fixed top-4 end-4 bg-brand-600 dark:bg-brand-700 rounded-full">
        <ThemeLanguageToggle />
      </div>

      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-brand-600 text-white mb-4 shadow-lg">
            <MessageSquare size={28} />
          </div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">{t('app.name')}</h1>
          <p className="text-gray-500 dark:text-gray-400 mt-1 text-sm">{t('app.taglineRegister')}</p>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl border border-gray-100 dark:border-gray-700 p-8">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100 mb-6">{t('auth.joinChatify')}</h2>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <Input
                label={t('auth.firstName')}
                placeholder={t('auth.placeholders.firstName')}
                error={errors.firstName?.message}
                {...register('firstName')}
              />
              <Input
                label={t('auth.lastName')}
                placeholder={t('auth.placeholders.lastName')}
                error={errors.lastName?.message}
                {...register('lastName')}
              />
            </div>
            <Input
              label={t('auth.username')}
              placeholder={t('auth.placeholders.username')}
              error={errors.userName?.message}
              {...register('userName')}
            />
            <Input
              label={t('auth.email')}
              type="email"
              placeholder={t('auth.placeholders.email')}
              error={errors.email?.message}
              {...register('email')}
            />
            <Input
              label={t('auth.password')}
              type="password"
              placeholder={t('auth.placeholders.passwordHint')}
              error={errors.password?.message}
              {...register('password')}
            />
            <Button type="submit" loading={isSubmitting} className="w-full mt-2">
              {t('auth.createAccountBtn')}
            </Button>
          </form>

          <p className="text-center text-sm text-gray-500 dark:text-gray-400 mt-6">
            {t('auth.haveAccount')}{' '}
            <Link to="/login" className="text-brand-600 dark:text-brand-400 font-medium hover:underline">
              {t('auth.signInLink')}
            </Link>
          </p>
        </div>
      </div>
    </div>
  )
}
