namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy offboarding rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyOffboardingRollupDesk(ILegacyOffboardingRollupStore Store, LegacyOffboardingRollupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy offboarding rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyOffboardingRollupStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy offboarding rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyOffboardingRollupStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of legacy offboarding rollup workflow.</summary>
public static class LegacyOffboardingRollupWorkflow
{
    public static LegacyOffboardingRollupDesk NewLegacyOffboardingRollupDesk(ILegacyOffboardingRollupStore store, IClock clock)
    {
        return new LegacyOffboardingRollupDesk(
            store,
            LegacyOffboardingRollupRules.DefaultLegacyOffboardingRollupValidator(),
            clock,
            new List<string>());
    }
}
