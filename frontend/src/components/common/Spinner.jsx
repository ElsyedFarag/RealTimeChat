export default function Spinner({ size = 'md', white = false }) {
  const sizes = { sm: 'w-4 h-4', md: 'w-6 h-6', lg: 'w-10 h-10' }
  const color = white ? 'border-white border-t-transparent' : 'border-brand-500 border-t-transparent dark:border-brand-400'
  return (
    <div className={`${sizes[size]} border-2 ${color} rounded-full animate-spin`} />
  )
}
