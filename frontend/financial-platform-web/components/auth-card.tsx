import type { ReactNode } from "react";
import { Wordmark } from "@/components/wordmark";

/**
 * Centered dark card used by /login and /register:
 * wordmark on top, card below, vertically centered in the viewport.
 */
export function AuthCard({ children }: { children: ReactNode }) {
  return (
    <main className="flex flex-1 flex-col items-center justify-center gap-6 px-4 py-12">
      <Wordmark />
      <div className="w-full max-w-md rounded-2xl border border-zinc-800 bg-zinc-900 p-6 shadow-xl sm:p-8">
        {children}
      </div>
    </main>
  );
}
