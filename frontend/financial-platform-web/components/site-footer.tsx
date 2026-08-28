export function SiteFooter() {
  return (
    <footer className="border-t border-zinc-800/70">
      <div className="mx-auto flex w-full max-w-6xl flex-col gap-2 px-4 py-8 sm:px-6">
        <p className="text-sm font-medium text-zinc-300">
          Financial Platform
        </p>
        <p className="max-w-2xl text-xs leading-5 text-zinc-500">
          Simulation environment for portfolio and trading workflows. No real
          money is involved and nothing on this platform constitutes
          investment advice.
        </p>
        <p className="text-xs text-zinc-600">
          © {new Date().getFullYear()} Financial Platform. All rights
          reserved.
        </p>
      </div>
    </footer>
  );
}
