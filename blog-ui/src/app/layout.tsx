import type { Metadata } from 'next'
import './globals.css'
import '../styles/tiptap.css'
import { ClientProviders } from '@/components/providers/client-providers'

export const metadata: Metadata = {
  title: 'Blog UI',
  description: 'A modern blog built with Next.js',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body suppressHydrationWarning={true}>
        <ClientProviders>
          {children}
        </ClientProviders>
      </body>
    </html>
  )
}