import { useEffect, useState } from 'react'
import { usersApi } from '../../api/usersApi'
import { useChat } from '../../contexts/ChatContext'
import Avatar from '../common/Avatar'

export default function OnlineUsers({ onStartChat }) {
  const [users, setUsers] = useState([])
  const { onlineUsers } = useChat()

  useEffect(() => {
    usersApi.getAll(1, 50).then(({ data }) => {
      if (data.success) setUsers(data.data || [])
    })
  }, [])

  const online = users.filter((u) => u.isOnline || onlineUsers[u.id])

  if (online.length === 0) return null

  return (
    <div className="px-4 py-3 border-b border-gray-100">
      <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-2">
        Online — {online.length}
      </p>
      <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-thin">
        {online.map((u) => (
          <button
            key={u.id}
            onClick={() => onStartChat(u)}
            className="flex flex-col items-center gap-1 shrink-0"
            title={u.fullName}
          >
            <Avatar name={u.fullName} url={u.profilePictureUrl} size="sm" online={true} />
            <span className="text-[10px] text-gray-500 truncate max-w-[40px]">
              {u.firstName}
            </span>
          </button>
        ))}
      </div>
    </div>
  )
}
