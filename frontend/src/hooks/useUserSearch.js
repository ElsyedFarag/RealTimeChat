import { useCallback, useState } from 'react'
import { usersApi } from '../api/usersApi'
import { useDebounce } from './useDebounce'
import { useEffect } from 'react'

export function useUserSearch() {
  const [query, setQuery] = useState('')
  const [results, setResults] = useState([])
  const [loading, setLoading] = useState(false)
  const debounced = useDebounce(query, 400)

  useEffect(() => {
    if (!debounced.trim()) { setResults([]); return }
    let cancelled = false
    setLoading(true)
    usersApi
      .search(debounced)
      .then(({ data }) => {
        if (!cancelled && data.success) setResults(data.data || [])
      })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [debounced])

  const clear = useCallback(() => { setQuery(''); setResults([]) }, [])

  return { query, setQuery, results, loading, clear }
}
