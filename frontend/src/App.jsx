import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from './contexts/AuthContext'
import AppLayout from './components/layout/AppLayout'
import LoginPage from './pages/auth/LoginPage'
import RegisterPage from './pages/auth/RegisterPage'
import ChatPage from './pages/chat/ChatPage'
import ProfilePage from './pages/ProfilePage'
import CreateGroupPage from './pages/groups/CreateGroupPage'
import GroupDetailsPage from './pages/groups/GroupDetailsPage'
import ProtectedRoute from './components/layout/ProtectedRoute'
import Spinner from './components/common/Spinner'

export default function App() {
  const { loading } = useAuth()
  if (loading) {
    return (
      <div className="h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900">
        <Spinner size="lg" />
      </div>
    )
  }
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route index element={<Navigate to="/chat" replace />} />
          <Route path="chat" element={<ChatPage />} />
          <Route path="chat/:chatId" element={<ChatPage />} />
          <Route path="groups/create" element={<CreateGroupPage />} />
          <Route path="groups/:groupId" element={<GroupDetailsPage />} />
          <Route path="profile" element={<ProfilePage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/chat" replace />} />
    </Routes>
  )
}
