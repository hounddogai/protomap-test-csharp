namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback balance claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackBalanceClaimManager(IFallbackBalanceClaimStore Store, FallbackBalanceClaimValidator Validator, IClock Clock, List<string> Journal, FallbackBalanceClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback balance claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackBalanceClaimStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback balance claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackBalanceClaimStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback balance claim {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of fallback balance claim workflow.</summary>
public static class FallbackBalanceClaimWorkflow
{
    public static FallbackBalanceClaimManager NewFallbackBalanceClaimManager(IFallbackBalanceClaimStore store, IClock clock, Registry flags)
    {
        return new FallbackBalanceClaimManager(
            store,
            FallbackBalanceClaimRules.DefaultFallbackBalanceClaimValidator(),
            clock,
            new List<string>(),
            FallbackBalanceClaimEngine.DefaultFallbackBalanceClaimEngine(flags));
    }
}
