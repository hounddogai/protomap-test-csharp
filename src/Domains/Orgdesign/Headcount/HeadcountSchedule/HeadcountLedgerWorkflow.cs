namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves headcount ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountLedgerService(IHeadcountLedgerStore Store, HeadcountLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountLedgerStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountLedgerStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of headcount ledger workflow.</summary>
public static class HeadcountLedgerWorkflow
{
    public static HeadcountLedgerService NewHeadcountLedgerService(IHeadcountLedgerStore store, IClock clock)
    {
        return new HeadcountLedgerService(
            store,
            HeadcountLedgerRules.DefaultHeadcountLedgerValidator(),
            clock,
            new List<string>());
    }
}
