const sizes = { sm: 'w-8 h-8 text-xs', md: 'w-10 h-10 text-sm', lg: 'w-12 h-12 text-base', xl: 'w-20 h-20 text-2xl' }
const dotSizes = { sm: 'w-2 h-2', md: 'w-2.5 h-2.5', lg: 'w-3 h-3', xl: 'w-4 h-4' }
const colors = [
  'bg-rose-400', 'bg-orange-400', 'bg-amber-400', 'bg-emerald-400',
  'bg-teal-400', 'bg-sky-400', 'bg-violet-400', 'bg-pink-400',
]

export default function Avatar({ name, url, size = 'md', online }) {
  const baseUrl = import.meta.env.VITE_API_BASE_URL
  const initials = (name || '?')
    .split(' ')
    .slice(0, 2)
    .map(w => w[0]?.toUpperCase() || '')
    .join('')

  const colorIdx = (name || '').charCodeAt(0) % colors.length
  const bgColor = colors[colorIdx]

  return (
    <div className="relative inline-flex flex-shrink-0">
      {url ? (
        <img
          src={url.startsWith('http') ? url : `${baseUrl}${url}`}
          alt={name || 'avatar'}
          className={`${sizes[size]} rounded-full object-cover`}
          onError={(e) => { e.target.style.display = 'none' }}
        />
      ) : (
        <div className={`${sizes[size]} ${bgColor} rounded-full flex items-center justify-center font-semibold text-white select-none`}>
          {initials || '?'}
        </div>
      )}
      {online !== undefined && (
        <span
          className={`absolute bottom-0 end-0 ${dotSizes[size]} rounded-full border-2 border-white dark:border-gray-800 ${online ? 'bg-emerald-400' : 'bg-gray-300 dark:bg-gray-600'}`}
        />
      )}
    </div>
  )
}
