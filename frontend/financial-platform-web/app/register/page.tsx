import type { Metadata } from "next";
import { AuthCard } from "@/components/auth-card";
import { SiteFooter } from "@/components/site-footer";
import { RegisterForm } from "./register-form";

export const metadata: Metadata = {
  title: "Create account",
};

export default function RegisterPage() {
  return (
    <>
      <AuthCard>
        <h1 className="text-2xl font-semibold tracking-tight text-zinc-50">
          Create your account
        </h1>
        <p className="mt-1.5 text-sm text-zinc-400">
          Start tracking portfolios in minutes. You’ll be signed in right
          after registering.
        </p>
        <div className="mt-6">
          <RegisterForm />
        </div>
      </AuthCard>
      <SiteFooter />
    </>
  );
}
