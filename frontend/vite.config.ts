import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    proxy: {
      '/api/auth': 'http://localhost:5001',
      '/api/users/online': 'http://localhost:5002',
      '/api/messages': 'http://localhost:5002',
      '/api/users': 'http://localhost:5001',
      '/hubs': {
        target: 'http://localhost:5002',
        ws: true
      }
    }
  }
})
