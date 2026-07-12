import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import LandingView from '../views/LandingView.vue'
import LoginView from '../views/LoginView.vue'
import UsersView from '../views/UsersView.vue'
import ChatView from '../views/ChatView.vue'

declare module 'vue-router' {
  interface RouteMeta {
    auth?: boolean
    guest?: boolean
  }
}

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'home', component: LandingView, meta: { guest: true } },
    { path: '/login', name: 'login', component: LoginView, meta: { guest: true } },
    { path: '/users', name: 'users', component: UsersView, meta: { auth: true } },
    { path: '/chat/:userId', name: 'chat', component: ChatView, meta: { auth: true } }
  ]
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.meta.auth && !auth.token) return { name: 'login' }
  if (to.meta.guest && auth.token && to.name !== 'home') return { name: 'users' }
  if (to.name === 'home' && auth.token) return { name: 'users' }
  return true
})

export default router
