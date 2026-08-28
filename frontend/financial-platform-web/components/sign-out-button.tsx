"use client";

import { useState } from "react";
import { useAuth } from "@/components/auth-provider";
import { Spinner } from "@/components/spinner";

/**
 * Sign-out button shared by the site header and the signed-in hero panel.
 * Disables itself while the best-effort API revoke is in flight.
 */
export function SignOutButton({ className = "" }: { className?: string }) {
  const { logout } = useAuth();
  const [pending, setPending] = useState(false);

  async function handleClick() {
    setPending(true);
    try {
      await logout();
    } finally {
      setPending(false);
    }
  }

  return (
    <button
      type="button"
      onClick={handleClick}
      disabled={pending}
      aria-busy={pending}
      className={`inline-flex h-10 items-center justify-center gap-2 rounded-lg border border-zinc-700 px-4 text-sm font-medium text-zinc-200 transition-colors hover:bg-zinc-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400 disabled:cursor-not-allowed disabled:opacity-60 ${className}`}
    >
      {pending ? <Spinner /> : null}
      {pending ? "Signing out…" : "Sign out"}
    </button>
  );
}
