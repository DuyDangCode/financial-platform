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

type FieldName =
  | "userName"
  | "email"
  | "password"
  | "confirmPassword"
  | "firstName"
  | "lastName"
  | "displayName"
  | "phoneNumber";

type FieldErrors = Partial<Record<FieldName, string>>;

interface FormValues {
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  displayName: string;
  phoneNumber: string;
}

const EMPTY_FORM: FormValues = {
  userName: "",
  email: "",
  password: "",
  confirmPassword: "",
  firstName: "",
  lastName: "",
  displayName: "",
  phoneNumber: "",
};

/** Mirrors the backend constraints documented in docs/05-api/auth.md. */
function validate(values: FormValues): FieldErrors {
  const errors: FieldErrors = {};

  if (!values.userName.trim()) {
    errors.userName = "Username is required.";
  } else if (values.userName.trim().length > 256) {
    errors.userName = "Username must be at most 256 characters.";
  }

  const email = values.email.trim();
  if (!email) {
    errors.email = "Email is required.";
  } else if (email.length > 256) {
    errors.email = "Email must be at most 256 characters.";
  } else if (!EMAIL_PATTERN.test(email)) {
    errors.email = "Enter a valid email address.";
  }

  if (!values.password) {
    errors.password = "Password is required.";
  } else if (values.password.length < 8) {
    errors.password = "Password must be at least 8 characters.";
  } else if (values.password.length > 128) {
    errors.password = "Password must be at most 128 characters.";
  }

  if (!values.confirmPassword) {
    errors.confirmPassword = "Please confirm your password.";
  } else if (values.confirmPassword !== values.password) {
    errors.confirmPassword = "Passwords do not match.";
  }

  // Optional fields: length caps only.
  if (values.firstName.length > 128) {
    errors.firstName = "First name must be at most 128 characters.";
  }
  if (values.lastName.length > 128) {
    errors.lastName = "Last name must be at most 128 characters.";
  }
  if (values.displayName.length > 256) {
    errors.displayName = "Display name must be at most 256 characters.";
  }
  if (values.phoneNumber.length > 32) {
    errors.phoneNumber = "Phone number must be at most 32 characters.";
  }

  return errors;
}

function applyServerValidation(
  entries: { field: string; message: string }[],
): { fieldErrors: FieldErrors; unmatched: string[] } {
  const fieldErrors: FieldErrors = {};
  const unmatched: string[] = [];
  const knownFields = new Set<string>([
    "userName",
    "email",
    "password",
    "confirmPassword",
    "firstName",
    "lastName",
    "displayName",
    "phoneNumber",
  ]);
  for (const entry of entries) {
    if (knownFields.has(entry.field)) {
      fieldErrors[entry.field as FieldName] = entry.message;
    } else {
      unmatched.push(entry.message);
    }
  }
  return { fieldErrors, unmatched };
}

export function RegisterForm() {
  const router = useRouter();
  const { register } = useAuth();

  const [values, setValues] = useState<FormValues>(EMPTY_FORM);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [banner, setBanner] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  function setValue(name: FieldName, value: string) {
    setValues((current) => ({ ...current, [name]: value }));
    // Re-validate a field after the first failed attempt touched it.
    setFieldErrors((current) =>
      current[name] ? { ...current, [name]: undefined } : current,
    );
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBanner(null);

    const errors = validate(values);
    setFieldErrors(errors);
    if (Object.keys(errors).length > 0) {
      return;
    }

    setSubmitting(true);
    try {
      await register({
        userName: values.userName.trim(),
        email: values.email.trim(),
        password: values.password,
        firstName: values.firstName,
        lastName: values.lastName,
        displayName: values.displayName,
        phoneNumber: values.phoneNumber,
      });
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
        name="userName"
        label="Username"
        value={values.userName}
        onValueChange={(value) => setValue("userName", value)}
        error={fieldErrors.userName}
        autoComplete="username"
        placeholder="thanhduy"
        maxLength={256}
        disabled={submitting}
        required
      />

      <Field
        name="email"
        label="Email"
        type="email"
        value={values.email}
        onValueChange={(value) => setValue("email", value)}
        error={fieldErrors.email}
        autoComplete="email"
        placeholder="you@example.com"
        maxLength={256}
        disabled={submitting}
        required
      />

      <Field
        name="password"
        label="Password"
        type="password"
        value={values.password}
        onValueChange={(value) => setValue("password", value)}
        error={fieldErrors.password}
        hint="8–128 characters."
        autoComplete="new-password"
        placeholder="••••••••"
        maxLength={128}
        disabled={submitting}
        required
      />

      <Field
        name="confirmPassword"
        label="Confirm password"
        type="password"
        value={values.confirmPassword}
        onValueChange={(value) => setValue("confirmPassword", value)}
        error={fieldErrors.confirmPassword}
        autoComplete="new-password"
        placeholder="••••••••"
        maxLength={128}
        disabled={submitting}
        required
      />

      {/* Optional details — native details/summary keeps this keyboard-operable. */}
      <details className="group mt-1 rounded-lg">
        <summary className="flex cursor-pointer select-none items-center gap-2 text-sm font-medium text-zinc-300 transition-colors hover:text-zinc-100 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400">
          <svg
            aria-hidden="true"
            viewBox="0 0 16 16"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
            className="h-3.5 w-3.5 transition-transform group-open:rotate-90"
          >
            <path d="m6 3 5 5-5 5" />
          </svg>
          Add personal details{" "}
          <span className="font-normal text-zinc-500">(optional)</span>
        </summary>
        <div className="mt-4 flex flex-col gap-4 border-t border-zinc-800 pt-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field
              name="firstName"
              label="First name"
              value={values.firstName}
              onValueChange={(value) => setValue("firstName", value)}
              error={fieldErrors.firstName}
              autoComplete="given-name"
              maxLength={128}
              disabled={submitting}
            />
            <Field
              name="lastName"
              label="Last name"
              value={values.lastName}
              onValueChange={(value) => setValue("lastName", value)}
              error={fieldErrors.lastName}
              autoComplete="family-name"
              maxLength={128}
              disabled={submitting}
            />
          </div>
          <Field
            name="displayName"
            label="Display name"
            value={values.displayName}
            onValueChange={(value) => setValue("displayName", value)}
            error={fieldErrors.displayName}
            autoComplete="nickname"
            maxLength={256}
            disabled={submitting}
          />
          <Field
            name="phoneNumber"
            label="Phone number"
            type="tel"
            value={values.phoneNumber}
            onValueChange={(value) => setValue("phoneNumber", value)}
            error={fieldErrors.phoneNumber}
            autoComplete="tel"
            placeholder="+84901234567"
            maxLength={32}
            disabled={submitting}
          />
        </div>
      </details>

      <button
        type="submit"
        disabled={submitting}
        aria-busy={submitting}
        className="mt-2 inline-flex h-11 w-full items-center justify-center gap-2 rounded-lg bg-emerald-500 text-sm font-medium text-white transition-colors hover:bg-emerald-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400 focus-visible:ring-offset-2 focus-visible:ring-offset-zinc-900 disabled:cursor-not-allowed disabled:opacity-60"
      >
        {submitting ? <Spinner /> : null}
        {submitting ? "Creating account…" : "Create account"}
      </button>

      <p className="text-center text-sm text-zinc-400">
        Already have an account?{" "}
        <Link
          href="/login"
          className="rounded-sm font-medium text-emerald-400 hover:text-emerald-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400"
        >
          Sign in
        </Link>
      </p>
    </form>
  );
}
