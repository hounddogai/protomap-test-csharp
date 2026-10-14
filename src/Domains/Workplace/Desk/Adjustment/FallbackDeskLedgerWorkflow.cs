namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback desk ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackDeskLedgerWorkflow(IFallbackDeskLedgerStore Store, FallbackDeskLedgerValidator Validator, IClock Clock, List<string> Journal, FallbackDeskLedgerEngine Engine)
{
    public static FallbackDeskLedgerWorkflow NewFallbackDeskLedgerWorkflow(IFallbackDeskLedgerStore store, IClock clock)
    {
        return new FallbackDeskLedgerWorkflow(
            store,
            FallbackDeskLedgerRules.DefaultFallbackDeskLedgerValidator(),
            clock,
            new List<string>(),
            FallbackDeskLedgerEngine.DefaultFallbackDeskLedgerEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback desk ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackDeskLedgerStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback desk ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackDeskLedgerStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback desk ledger {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
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
