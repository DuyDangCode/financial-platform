import type { ReactNode } from "react";
import { HomeCta } from "@/components/home-cta";
import { SiteFooter } from "@/components/site-footer";
import { SiteHeader } from "@/components/site-header";

interface Feature {
  title: string;
  description: string;
  icon: ReactNode;
}

function ChartIcon() {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      className="h-5 w-5"
    >
      <path d="M3 3v16a2 2 0 0 0 2 2h16" />
      <path d="m7 14 4-4 4 3 5-6" />
    </svg>
  );
}

function ExchangeIcon() {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      className="h-5 w-5"
    >
      <path d="M8 3 4 7l4 4" />
      <path d="M4 7h16" />
      <path d="m16 21 4-4-4-4" />
      <path d="M20 17H4" />
    </svg>
  );
}

function GlobeIcon() {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      className="h-5 w-5"
    >
      <circle cx="12" cy="12" r="9" />
      <path d="M3 12h18" />
      <path d="M12 3a15 15 0 0 1 0 18 15 15 0 0 1 0-18Z" />
    </svg>
  );
}

const FEATURES: Feature[] = [
  {
    title: "Portfolio tracking",
    description:
      "Create portfolios, follow valuations and review performance across all of your positions.",
    icon: <ChartIcon />,
  },
  {
    title: "Trading & orders",
    description:
      "Place and manage simulated trading orders end-to-end, from ticket to transaction history.",
    icon: <ExchangeIcon />,
  },
  {
    title: "Market data",
    description:
      "Look up supported assets and monitor the market context behind every decision you make.",
    icon: <GlobeIcon />,
  },
];

export default function Home() {
  return (
    <>
      <SiteHeader />
      <main className="flex-1">
        {/* Hero */}
        <section className="mx-auto w-full max-w-6xl px-4 py-20 sm:px-6 sm:py-28 border-b border-zinc-800/70">
          <p className="text-xs font-semibold uppercase tracking-widest text-emerald-400">
            Portfolio &amp; trading operations
          </p>
          <h1 className="mt-4 max-w-2xl text-4xl font-semibold tracking-tight text-zinc-50 sm:text-5xl">
            Your capital, clearly under control.
          </h1>
          <p className="mt-5 max-w-xl text-lg leading-8 text-zinc-400">
            Track portfolios, place simulated trades and follow market data —
            one platform built for clarity, from first deposit to full
            performance review.
          </p>
          <div className="mt-8">
            <HomeCta />
          </div>
        </section>

        {/* Feature grid */}
        <section aria-labelledby="features-heading" className="mx-auto w-full max-w-6xl px-4 py-20 sm:px-6">
          <h2
            id="features-heading"
            className="text-2xl font-semibold tracking-tight text-zinc-50"
          >
            What you can do here
          </h2>
          <p className="mt-2 max-w-xl text-base text-zinc-400">
            A focused roadmap of capabilities, delivered module by module.
          </p>
          <ul className="mt-10 grid grid-cols-1 gap-5 md:grid-cols-3">
            {FEATURES.map((feature) => (
              <li
                key={feature.title}
                className="flex flex-col gap-3 rounded-xl border border-zinc-800 bg-zinc-900 p-6"
              >
                <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-emerald-500/10 text-emerald-400">
                  {feature.icon}
                </span>
                <div>
                  <h3 className="text-base font-semibold text-zinc-50">
                    {feature.title}
                  </h3>
                  <span className="mt-1 inline-block rounded-full border border-zinc-700 bg-zinc-950 px-2 py-0.5 text-[11px] font-medium uppercase tracking-wide text-zinc-400">
                    Roadmap
                  </span>
                </div>
                <p className="text-sm leading-6 text-zinc-400">
                  {feature.description}
                </p>
              </li>
            ))}
          </ul>
        </section>
      </main>
      <SiteFooter />
    </>
  );
}
