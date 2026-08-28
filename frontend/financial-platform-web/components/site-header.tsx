"use client";

import Link from "next/link";
import { useAuth } from "@/components/auth-provider";
import { SignOutButton } from "@/components/sign-out-button";
import { Wordmark } from "@/components/wordmark";

export function SiteHeader() {
  const { user, isAuthenticated } = useAuth();

  return (
    <header className="sticky top-0 z-20 border-b border-zinc-800/70 bg-zinc-950/85 backdrop-blur">
      <div className="mx-auto flex h-16 w-full max-w-6xl items-center justify-between gap-4 px-4 sm:px-6">
        <Wordmark />
        {isAuthenticated && user ? (
          <nav aria-label="Main" className="flex items-center gap-3">
            <span className="hidden text-sm text-zinc-400 sm:inline">
              Signed in as{" "}
              <span className="font-medium text-zinc-100">
                {user.displayName || user.userName}
              </span>
            </span>
            <SignOutButton />
          </nav>
        ) : (
          <nav aria-label="Main" className="flex items-center gap-3">
            <Link
              href="/login"
              className="inline-flex h-10 items-center rounded-lg px-3 text-sm font-medium text-zinc-300 transition-colors hover:text-zinc-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400"
            >
              Log in
            </Link>
            <Link
              href="/register"
              className="hidden h-10 items-center rounded-lg bg-emerald-500 px-4 text-sm font-medium text-white transition-colors hover:bg-emerald-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400 sm:inline-flex"
            >
              Get started
            </Link>
          </nav>
        )}
      </div>
    </header>
  );
}
