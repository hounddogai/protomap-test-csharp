namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered offboarding segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredOffboardingSegmentManager(ITieredOffboardingSegmentStore Store, TieredOffboardingSegmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered offboarding segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredOffboardingSegmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered offboarding segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredOffboardingSegmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of tiered offboarding segment workflow.</summary>
public static class TieredOffboardingSegmentWorkflow
{
    public static TieredOffboardingSegmentManager NewTieredOffboardingSegmentManager(ITieredOffboardingSegmentStore store, IClock clock)
    {
        return new TieredOffboardingSegmentManager(
            store,
            TieredOffboardingSegmentRules.DefaultTieredOffboardingSegmentValidator(),
            clock,
            new List<string>());
    }
}
