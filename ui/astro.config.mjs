import { defineConfig } from 'astro/config';
import { fileURLToPath } from 'url';
import tailwind from '@tailwindcss/vite';
import react from '@astrojs/react';
import sitemap from '@astrojs/sitemap';

export default defineConfig({
  site: 'https://www.phaenobiotech.com',
  trailingSlash: 'never',
  output: 'static',

  redirects: {
    // About consolidation
    '/about': { destination: '/about/about-us', status: 308 },
    '/mission': { destination: '/about/about-us', status: 308 },
    '/our-team': { destination: '/about/about-us', status: 308 },
    '/our-team-2': { destination: '/about/about-us', status: 308 },
    // Careers
    '/join-us': { destination: '/about/job-openings', status: 308 },
    // Login (external)
    '/log-in': { destination: 'https://portal.phaenobiotech.com/auth/sign-in', status: 308, },
    // Contact alias
    '/contactus': { destination: '/contact', status: 308 },
  },

  integrations: [
    react(),
    sitemap(),
    (await import('astro-compress')).default(),
  ],

  vite: {
    build: { minify: 'esbuild' },
    plugins: [tailwind()],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
      },
    },
  },
});