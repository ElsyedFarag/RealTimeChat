import Spinner from './Spinner'

const variants = {
  primary: 'bg-brand-600 hover:bg-brand-700 text-white shadow-sm dark:bg-brand-500 dark:hover:bg-brand-600',
  secondary: 'bg-gray-100 hover:bg-gray-200 text-gray-800 dark:bg-gray-700 dark:hover:bg-gray-600 dark:text-gray-200',
  ghost: 'hover:bg-gray-100 text-gray-600 dark:hover:bg-gray-700 dark:text-gray-400',
  danger: 'bg-red-500 hover:bg-red-600 text-white dark:bg-red-600 dark:hover:bg-red-700',
}

export default function Button({ children, variant = 'primary', loading, className = '', ...props }) {
  return (
    <button
      className={`inline-flex items-center justify-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium transition disabled:opacity-50 disabled:cursor-not-allowed ${variants[variant]} ${className}`}
      disabled={loading || props.disabled}
      {...props}
    >
      {loading && <Spinner size="sm" white={variant === 'primary' || variant === 'danger'} />}
      {children}
    </button>
  )
}
