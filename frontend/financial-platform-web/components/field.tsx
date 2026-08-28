"use client";

import { useId } from "react";
import type { ChangeEvent, ReactNode } from "react";

interface FieldProps {
  name: string;
  label: string;
  value: string;
  onValueChange: (value: string) => void;
  type?: "text" | "email" | "password" | "tel";
  required?: boolean;
  hint?: ReactNode;
  error?: string;
  autoComplete?: string;
  placeholder?: string;
  maxLength?: number;
  disabled?: boolean;
}

/**
 * Labeled input with inline error/hint wiring (aria-invalid + aria-describedby).
 * Shared by the login and register forms.
 */
export function Field({
  name,
  label,
  value,
  onValueChange,
  type = "text",
  required = false,
  hint,
  error,
  autoComplete,
  placeholder,
  maxLength,
  disabled = false,
}: FieldProps) {
  const id = useId();
  const hintId = `${id}-hint`;
  const errorId = `${id}-error`;
  const describedBy =
    [error ? errorId : null, hint ? hintId : null]
      .filter(Boolean)
      .join(" ") || undefined;

  return (
    <div className="flex flex-col gap-1.5">
      <label htmlFor={id} className="text-sm font-medium text-zinc-300">
        {label}
        {required ? (
          <span aria-hidden="true" className="text-red-400">
            {" "}
            *
          </span>
        ) : null}
      </label>
      <input
        id={id}
        name={name}
        type={type}
        value={value}
        onChange={(event: ChangeEvent<HTMLInputElement>) =>
          onValueChange(event.target.value)
        }
        required={required}
        autoComplete={autoComplete}
        placeholder={placeholder}
        maxLength={maxLength}
        disabled={disabled}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy}
        className={`h-11 w-full rounded-lg border bg-zinc-950 px-3 text-sm text-zinc-100 transition-colors placeholder:text-zinc-600 focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-400 disabled:cursor-not-allowed disabled:opacity-60 ${
          error
            ? "border-red-500/60 focus-visible:ring-red-400"
            : "border-zinc-700 hover:border-zinc-600"
        }`}
      />
      {hint && !error ? (
        <p id={hintId} className="text-xs text-zinc-500">
          {hint}
        </p>
      ) : null}
      {error ? (
        <p id={errorId} className="text-xs text-red-400">
          {error}
        </p>
      ) : null}
    </div>
  );
}
