namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive position ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptivePositionLedgerManager(IAdaptivePositionLedgerStore Store, AdaptivePositionLedgerValidator Validator, IClock Clock, List<string> Journal, AdaptivePositionLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive position ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptivePositionLedgerStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive position ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptivePositionLedgerStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive position ledger {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of adaptive position ledger workflow.</summary>
public static class AdaptivePositionLedgerWorkflow
{
    public static AdaptivePositionLedgerManager NewAdaptivePositionLedgerManager(IAdaptivePositionLedgerStore store, IClock clock, Registry flags)
    {
        return new AdaptivePositionLedgerManager(
            store,
            AdaptivePositionLedgerRules.DefaultAdaptivePositionLedgerValidator(),
            clock,
            new List<string>(),
            AdaptivePositionLedgerEngine.DefaultAdaptivePositionLedgerEngine(flags));
    }
}
