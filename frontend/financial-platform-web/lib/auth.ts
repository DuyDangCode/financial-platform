/**
 * Dependency-free client-side auth: session persistence + auth API calls.
 *
 * The session (tokens + user info) is stored in a single `localStorage` key.
 * Pure module, no React — the React binding lives in components/auth-provider.tsx.
 */

import { ApiRequestError, postJson } from "@/lib/api";

const STORAGE_KEY = "fp.auth.session";

/** Subset of LoginResponse that identifies the signed-in user. */
export interface AuthUser {
  userId: string;
  userName: string;
  email: string;
  displayName: string;
}

export interface AuthSession {
  token: string;
  /** ISO-8601 UTC timestamp of access token expiration. */
  expiresAt: string;
  refreshToken: string | null;
  user: AuthUser;
}

interface LoginResponseDto {
  token: string;
  expiresAt: string;
  userId: string;
  userName: string;
  email: string;
  displayName: string;
  refreshToken: string | null;
}

export interface RegisterInput {
  userName: string;
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
  displayName?: string;
  phoneNumber?: string;
}

export function toSession(response: LoginResponseDto): AuthSession {
  return {
    token: response.token,
    expiresAt: response.expiresAt,
    refreshToken: response.refreshToken,
    user: {
      userId: response.userId,
      userName: response.userName,
      email: response.email,
      displayName: response.displayName,
    },
  };
}

export async function loginRequest(
  email: string,
  password: string,
): Promise<AuthSession> {
  const data = await postJson<LoginResponseDto>("/api/auth/login", {
    email,
    password,
  });
  const session = toSession(data);
  saveSession(session);
  return session;
}

export async function registerRequest(
  input: RegisterInput,
): Promise<AuthSession> {
  // Omit optional keys entirely when empty, matching the backend contract.
  const payload = Object.fromEntries(
    Object.entries(input).filter(([, value]) => value !== undefined && value !== ""),
  );
  const data = await postJson<LoginResponseDto>(
    "/api/auth/register",
    payload,
  );
  const session = toSession(data);
  saveSession(session);
  return session;
}

/**
 * Revoke the refresh token server-side. Best-effort: failures are swallowed
 * so local sign-out always completes.
 */
export async function logoutRequest(
  refreshToken: string | null,
): Promise<void> {
  if (!refreshToken) {
    return;
  }
  try {
    await postJson<unknown>("/api/auth/logout", { refreshToken });
  } catch {
    // Ignore API/network failures on logout; local state is cleared regardless.
  }
}

export function saveSession(session: AuthSession): void {
  try {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  } catch {
    // Storage may be unavailable (private mode); auth still works in memory.
  }
}

export function loadSession(): AuthSession | null {
  if (typeof window === "undefined") {
    return null;
  }
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }
    const parsed = JSON.parse(raw) as Partial<AuthSession>;
    if (
      typeof parsed?.token === "string" &&
      typeof parsed?.expiresAt === "string" &&
      parsed.user &&
      typeof parsed.user.userId === "string"
    ) {
      return parsed as AuthSession;
    }
    return null;
  } catch {
    return null;
  }
}

export function clearSession(): void {
  try {
    window.localStorage.removeItem(STORAGE_KEY);
  } catch {
    // Ignore storage failures.
  }
}

export { ApiRequestError };
