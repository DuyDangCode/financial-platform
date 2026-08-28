import type { Metadata } from "next";
import { AuthCard } from "@/components/auth-card";
import { SiteFooter } from "@/components/site-footer";
import { LoginForm } from "./login-form";

export const metadata: Metadata = {
  title: "Sign in",
};

export default function LoginPage() {
  return (
    <>
      <AuthCard>
        <h1 className="text-2xl font-semibold tracking-tight text-zinc-50">
          Sign in
        </h1>
        <p className="mt-1.5 text-sm text-zinc-400">
          Welcome back. Enter your credentials to continue.
        </p>
        <div className="mt-6">
          <LoginForm />
        </div>
      </AuthCard>
      <SiteFooter />
    </>
  );
}
