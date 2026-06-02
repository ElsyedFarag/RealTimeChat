import { Outlet, useLocation } from 'react-router-dom'
import ChatSidebar from '../chat/ChatSidebar'

export default function AppLayout() {
  const location = useLocation()
  const inChat = location.pathname.startsWith('/chat/')

  return (
    <div className="h-screen flex overflow-hidden bg-gray-50 dark:bg-gray-900">
      {/* Sidebar — hidden on mobile when inside a chat */}
      <div className={`flex-shrink-0 transition-all duration-300 ease-in-out ${inChat ? 'hidden md:flex' : 'flex'} w-full md:w-80`}>
        <ChatSidebar />
      </div>

      {/* Main content */}
      <main className={`flex-1 flex flex-col overflow-hidden ${!inChat ? 'hidden md:flex' : 'flex'}`}>
        <Outlet />
      </main>
    </div>
  )
}
