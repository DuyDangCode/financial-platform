"use client";

import Link from "next/link";
import { useAuth } from "@/components/auth-provider";
import { SignOutButton } from "@/components/sign-out-button";

function GuestCta() {
  return (
    <div className="flex flex-col gap-3 sm:flex-row">
      <Link
        href="/register"
        className="inline-flex h-12 w-full items-center justify-center rounded-lg bg-emerald-500 px-6 text-sm font-medium text-white transition-colors hover:bg-emerald-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400 sm:w-auto"
      >
        Create free account
      </Link>
      <Link
        href="/login"
        className="inline-flex h-12 w-full items-center justify-center rounded-lg border border-zinc-700 px-6 text-sm font-medium text-zinc-200 transition-colors hover:bg-zinc-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400 sm:w-auto"
      >
        Sign in
      </Link>
    </div>
  );
}

function SignedInPanel({
  displayName,
  email,
}: {
  displayName: string;
  email: string;
}) {
  return (
    <div
      role="status"
      className="inline-flex flex-col items-start gap-4 rounded-xl border border-emerald-500/25 bg-emerald-500/5 p-5"
    >
      <p className="flex items-center gap-2 text-sm font-medium text-emerald-300">
        {/* Check icon paired with text so state is not conveyed by color alone */}
        <svg
          aria-hidden="true"
          viewBox="0 0 20 20"
          fill="currentColor"
          className="h-5 w-5"
        >
          <path
            fillRule="evenodd"
            d="M10 18a8 8 0 1 0 0-16 8 8 0 0 0 0 16Zm3.857-9.809a.75.75 0 0 0-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 1 0-1.06 1.061l2.5 2.5a.75.75 0 0 0 1.137-.089l4-5.5Z"
            clipRule="evenodd"
          />
        </svg>
        You’re signed in as {displayName || email}
      </p>
      <p className="text-sm text-zinc-400">{email}</p>
      <SignOutButton />
    </div>
  );
}

/**
 * Hero CTA zone. Renders guest CTAs until hydration completes so the
 * server HTML matches the first client render; swaps to the signed-in
 * panel afterwards when a stored session exists.
 */
export function HomeCta() {
  const { user, isAuthenticated } = useAuth();

  if (isAuthenticated && user) {
    return (
      <SignedInPanel
        displayName={user.displayName || user.userName}
        email={user.email}
      />
    );
  }
  return <GuestCta />;
}
