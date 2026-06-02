import { forwardRef } from 'react'

const Input = forwardRef(({ label, error, className = '', ...props }, ref) => (
  <div className="w-full">
    {label && (
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
        {label}
      </label>
    )}
    <input
      ref={ref}
      className={`w-full rounded-xl border px-4 py-2.5 text-sm outline-none transition
        bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100
        placeholder-gray-400 dark:placeholder-gray-500
        focus:ring-2 focus:ring-brand-500 focus:border-brand-500
        ${error
          ? 'border-red-400 bg-red-50 dark:bg-red-900/20 dark:border-red-500'
          : 'border-gray-200 dark:border-gray-600 hover:border-gray-300 dark:hover:border-gray-500'
        }
        ${className}`}
      {...props}
    />
    {error && <p className="mt-1 text-xs text-red-500 dark:text-red-400">{error}</p>}
  </div>
))

Input.displayName = 'Input'
export default Input
