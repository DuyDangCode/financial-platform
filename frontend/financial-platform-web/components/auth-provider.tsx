"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import type { ReactNode } from "react";
import {
  clearSession,
  loadSession,
  loginRequest,
  logoutRequest,
  registerRequest,
} from "@/lib/auth";
import type { AuthSession, AuthUser, RegisterInput } from "@/lib/auth";

interface AuthContextValue {
  /** Signed-in user, or null for guests / before hydration. */
  user: AuthUser | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<AuthSession>;
  register: (input: RegisterInput) => Promise<AuthSession>;
  /** Revokes the refresh token best-effort, then always clears local state. */
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(null);

  // localStorage is browser-only: hydrate after mount so the first client
  // render matches the server output (guest state), then swap.
  useEffect(() => {
    const stored = loadSession();
    if (stored) {
      setSession(stored);
    }
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const next = await loginRequest(email, password);
    setSession(next);
    return next;
  }, []);

  const register = useCallback(async (input: RegisterInput) => {
    const next = await registerRequest(input);
    setSession(next);
    return next;
  }, []);

  const logout = useCallback(async () => {
    const refreshToken = session?.refreshToken ?? null;
    setSession(null);
    clearSession();
    await logoutRequest(refreshToken);
  }, [session?.refreshToken]);

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session?.user ?? null,
      isAuthenticated: session !== null,
      login,
      register,
      logout,
    }),
    [session, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an <AuthProvider>.");
  }
  return ctx;
}
