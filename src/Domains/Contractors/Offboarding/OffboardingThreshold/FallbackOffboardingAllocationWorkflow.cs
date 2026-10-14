namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback offboarding allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackOffboardingAllocationManager(IFallbackOffboardingAllocationStore Store, FallbackOffboardingAllocationValidator Validator, IClock Clock, List<string> Journal, FallbackOffboardingAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback offboarding allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackOffboardingAllocationStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback offboarding allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackOffboardingAllocationStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback offboarding allocation {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of fallback offboarding allocation workflow.</summary>
public static class FallbackOffboardingAllocationWorkflow
{
    public static FallbackOffboardingAllocationManager NewFallbackOffboardingAllocationManager(IFallbackOffboardingAllocationStore store, IClock clock)
    {
        return new FallbackOffboardingAllocationManager(
            store,
            FallbackOffboardingAllocationRules.DefaultFallbackOffboardingAllocationValidator(),
            clock,
            new List<string>(),
            FallbackOffboardingAllocationEngine.DefaultFallbackOffboardingAllocationEngine());
    }
}
