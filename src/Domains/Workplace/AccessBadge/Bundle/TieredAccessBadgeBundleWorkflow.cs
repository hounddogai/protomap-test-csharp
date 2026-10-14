namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered access badge bundles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredAccessBadgeBundleCoordinator(ITieredAccessBadgeBundleStore Store, TieredAccessBadgeBundleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered access badge bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredAccessBadgeBundleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered access badge bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredAccessBadgeBundleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of tiered access badge bundle workflow.</summary>
public static class TieredAccessBadgeBundleWorkflow
{
    public static TieredAccessBadgeBundleCoordinator NewTieredAccessBadgeBundleCoordinator(ITieredAccessBadgeBundleStore store, IClock clock)
    {
        return new TieredAccessBadgeBundleCoordinator(
            store,
            TieredAccessBadgeBundleRules.DefaultTieredAccessBadgeBundleValidator(),
            clock,
            new List<string>());
    }
}
