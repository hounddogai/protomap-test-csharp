namespace Acme.Workforce.Contractors.Rate.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback rate bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackRateBundleCoordinator(IFallbackRateBundleStore Store, FallbackRateBundleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback rate bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackRateBundleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback rate bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackRateBundleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of fallback rate bundle workflow.</summary>
public static class FallbackRateBundleWorkflow
{
    public static FallbackRateBundleCoordinator NewFallbackRateBundleCoordinator(IFallbackRateBundleStore store, IClock clock)
    {
        return new FallbackRateBundleCoordinator(
            store,
            FallbackRateBundleRules.DefaultFallbackRateBundleValidator(),
            clock,
            new List<string>());
    }
}
