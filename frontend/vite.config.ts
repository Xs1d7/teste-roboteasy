import path from 'node:path'
import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite'
import vue from '@vitejs/plugin-vue'

const root = fileURLToPath(new URL('.', import.meta.url))

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(root, './src')
    }
  },
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
