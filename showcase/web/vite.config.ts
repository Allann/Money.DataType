import { fileURLToPath } from 'node:url'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// The tours import the solution's real C# files, so Vite may read from the repository root.
const repositoryRoot = fileURLToPath(new URL('../..', import.meta.url))

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: { '/api': 'http://localhost:5080' },
    fs: { allow: [repositoryRoot] },
  },
  // The API serves the built app, so one process hosts both in production.
  build: {
    outDir: '../Hs.Showcase.Api/wwwroot',
    emptyOutDir: true,
  },
  test: {
    environment: 'jsdom',
    // Lets Testing Library register its own cleanup after each test.
    globals: true,
    setupFiles: ['./src/test/setup.ts'],
  },
})
