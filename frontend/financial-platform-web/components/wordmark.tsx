import Link from "next/link";

/**
 * Brand wordmark. Pure navigation — no client features needed,
 * so it renders in both Server and Client Components.
 */
export function Wordmark({ className = "" }: { className?: string }) {
  return (
    <Link
      href="/"
      className={`flex items-center gap-2 rounded-lg text-base font-semibold tracking-tight text-zinc-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400 ${className}`}
    >
      <span
        aria-hidden="true"
        className="flex h-7 w-7 items-center justify-center rounded-md bg-emerald-500/15 text-sm font-bold text-emerald-400"
      >
        F
      </span>
      Financial Platform
    </Link>
  );
}
