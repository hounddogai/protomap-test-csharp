namespace Acme.Workforce.Compensation.MeritCycle.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback merit cycle claims through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackMeritCycleClaimManager(IFallbackMeritCycleClaimStore Store, FallbackMeritCycleClaimValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback merit cycle claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackMeritCycleClaimStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback merit cycle claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackMeritCycleClaimStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of fallback merit cycle claim workflow.</summary>
public static class FallbackMeritCycleClaimWorkflow
{
    public static FallbackMeritCycleClaimManager NewFallbackMeritCycleClaimManager(IFallbackMeritCycleClaimStore store, IClock clock)
    {
        return new FallbackMeritCycleClaimManager(
            store,
            FallbackMeritCycleClaimRules.DefaultFallbackMeritCycleClaimValidator(),
            clock,
            new List<string>());
    }
}
