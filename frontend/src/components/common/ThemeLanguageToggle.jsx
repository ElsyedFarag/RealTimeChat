import { Moon, Sun, Globe } from 'lucide-react'
import { useTheme } from '../../contexts/ThemeContext'
import { useLanguage } from '../../contexts/LanguageContext'

export default function ThemeLanguageToggle({ compact = false }) {
  const { isDark, toggleTheme } = useTheme()
  const { language, changeLanguage } = useLanguage()

  return (
    <div className="flex items-center gap-1">
      <button
        onClick={toggleTheme}
        title={isDark ? 'Switch to light mode' : 'Switch to dark mode'}
        className="p-2 rounded-full text-white hover:bg-white/20 transition-colors"
      >
        {isDark ? <Sun size={16} /> : <Moon size={16} />}
      </button>
      <button
        onClick={() => changeLanguage(language === 'en' ? 'ar' : 'en')}
        title="Toggle language"
        className="p-2 rounded-full text-white hover:bg-white/20 transition-colors text-xs font-bold"
      >
        {language === 'en' ? 'ع' : 'EN'}
      </button>
    </div>
  )
}
