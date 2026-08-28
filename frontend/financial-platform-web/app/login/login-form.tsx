"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import type { FormEvent } from "react";
import { useAuth } from "@/components/auth-provider";
import { Field } from "@/components/field";
import { Spinner } from "@/components/spinner";
import { ApiRequestError } from "@/lib/api";

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

type FieldErrors = Partial<Record<"email" | "password", string>>;

function validate(email: string, password: string): FieldErrors {
  const errors: FieldErrors = {};
  if (!email.trim()) {
    errors.email = "Email is required.";
  } else if (!EMAIL_PATTERN.test(email.trim())) {
    errors.email = "Enter a valid email address.";
  }
  if (!password) {
    errors.password = "Password is required.";
  }
  return errors;
}

/** Map API validationErrors onto form fields; returns leftover messages. */
function applyServerValidation(
  entries: { field: string; message: string }[],
): { fieldErrors: FieldErrors; unmatched: string[] } {
  const fieldErrors: FieldErrors = {};
  const unmatched: string[] = [];
  for (const entry of entries) {
    if (entry.field === "email" || entry.field === "password") {
      fieldErrors[entry.field] = entry.message;
    } else {
      unmatched.push(entry.message);
    }
  }
  return { fieldErrors, unmatched };
}

export function LoginForm() {
  const router = useRouter();
  const { login } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [banner, setBanner] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBanner(null);

    const trimmedEmail = email.trim();
    const errors = validate(trimmedEmail, password);
    setFieldErrors(errors);
    if (Object.keys(errors).length > 0) {
      return;
    }

    setSubmitting(true);
    try {
      await login(trimmedEmail, password);
      router.replace("/");
    } catch (error) {
      if (error instanceof ApiRequestError) {
        const { fieldErrors: serverFieldErrors, unmatched } =
          applyServerValidation(error.validationErrors);
        if (Object.keys(serverFieldErrors).length > 0) {
          setFieldErrors(serverFieldErrors);
        }
        if (unmatched.length > 0 || Object.keys(serverFieldErrors).length === 0) {
          setBanner(unmatched.length > 0 ? unmatched.join(" ") : error.message);
        }
      } else {
        setBanner("Something went wrong. Please try again.");
      }
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} noValidate className="flex flex-col gap-4">
      {banner ? (
        <div
          role="alert"
          className="rounded-lg border border-red-500/40 bg-red-500/10 px-4 py-3 text-sm text-red-300"
        >
          {banner}
        </div>
      ) : null}

      <Field
        name="email"
        label="Email"
        type="email"
        value={email}
        onValueChange={(value) => setEmail(value)}
        error={fieldErrors.email}
        autoComplete="email"
        placeholder="you@example.com"
        disabled={submitting}
        required
      />

      <Field
        name="password"
        label="Password"
        type="password"
        value={password}
        onValueChange={(value) => setPassword(value)}
        error={fieldErrors.password}
        autoComplete="current-password"
        placeholder="••••••••"
        disabled={submitting}
        required
      />

      <button
        type="submit"
        disabled={submitting}
        aria-busy={submitting}
        className="mt-2 inline-flex h-11 w-full items-center justify-center gap-2 rounded-lg bg-emerald-500 text-sm font-medium text-white transition-colors hover:bg-emerald-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400 focus-visible:ring-offset-2 focus-visible:ring-offset-zinc-900 disabled:cursor-not-allowed disabled:opacity-60"
      >
        {submitting ? <Spinner /> : null}
        {submitting ? "Signing in…" : "Sign in"}
      </button>

      <p className="text-center text-sm text-zinc-400">
        Don’t have an account?{" "}
        <Link
          href="/register"
          className="font-medium text-emerald-400 hover:text-emerald-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400 rounded-sm"
        >
          Create one
        </Link>
      </p>
    </form>
  );
}
