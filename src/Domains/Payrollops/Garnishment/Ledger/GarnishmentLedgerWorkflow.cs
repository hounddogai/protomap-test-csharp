namespace Acme.Workforce.Payrollops.Garnishment.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves garnishment ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentLedgerService(IGarnishmentLedgerStore Store, GarnishmentLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentLedgerStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentLedgerStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of garnishment ledger workflow.</summary>
public static class GarnishmentLedgerWorkflow
{
    public static GarnishmentLedgerService NewGarnishmentLedgerService(IGarnishmentLedgerStore store, IClock clock)
    {
        return new GarnishmentLedgerService(
            store,
            GarnishmentLedgerRules.DefaultGarnishmentLedgerValidator(),
            clock,
            new List<string>());
    }
}
