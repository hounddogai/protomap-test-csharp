namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary headcount ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryHeadcountLedgerService(IPrimaryHeadcountLedgerStore Store, PrimaryHeadcountLedgerValidator Validator, IClock Clock, List<string> Journal, PrimaryHeadcountLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary headcount ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryHeadcountLedgerStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary headcount ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryHeadcountLedgerStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary headcount ledger {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of primary headcount ledger workflow.</summary>
public static class PrimaryHeadcountLedgerWorkflow
{
    public static PrimaryHeadcountLedgerService NewPrimaryHeadcountLedgerService(IPrimaryHeadcountLedgerStore store, IClock clock)
    {
        return new PrimaryHeadcountLedgerService(
            store,
            PrimaryHeadcountLedgerRules.DefaultPrimaryHeadcountLedgerValidator(),
            clock,
            new List<string>(),
            PrimaryHeadcountLedgerEngine.DefaultPrimaryHeadcountLedgerEngine());
    }
}
