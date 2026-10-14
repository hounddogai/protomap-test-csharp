namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves reorg ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgLedgerCoordinator(IReorgLedgerStore Store, ReorgLedgerValidator Validator, IClock Clock, List<string> Journal, ReorgLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgLedgerStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgLedgerStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg ledger {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of reorg ledger workflow.</summary>
public static class ReorgLedgerWorkflow
{
    public static ReorgLedgerCoordinator NewReorgLedgerCoordinator(IReorgLedgerStore store, IClock clock, Registry flags)
    {
        return new ReorgLedgerCoordinator(
            store,
            ReorgLedgerRules.DefaultReorgLedgerValidator(),
            clock,
            new List<string>(),
            ReorgLedgerEngine.DefaultReorgLedgerEngine(flags));
    }
}
