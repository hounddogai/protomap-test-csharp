namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback license seat ledgers through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackLicenseSeatLedgerDesk(IFallbackLicenseSeatLedgerStore Store, FallbackLicenseSeatLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback license seat ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackLicenseSeatLedgerStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback license seat ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackLicenseSeatLedgerStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.ExpiresAt > cutoff)
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of fallback license seat ledger workflow.</summary>
public static class FallbackLicenseSeatLedgerWorkflow
{
    public static FallbackLicenseSeatLedgerDesk NewFallbackLicenseSeatLedgerDesk(IFallbackLicenseSeatLedgerStore store, IClock clock)
    {
        return new FallbackLicenseSeatLedgerDesk(
            store,
            FallbackLicenseSeatLedgerRules.DefaultFallbackLicenseSeatLedgerValidator(),
            clock,
            new List<string>());
    }
}
