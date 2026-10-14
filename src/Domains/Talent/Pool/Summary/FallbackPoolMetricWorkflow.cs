namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback pool metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackPoolMetricCoordinator(IFallbackPoolMetricStore Store, FallbackPoolMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pool metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackPoolMetricStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pool metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackPoolMetricStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of fallback pool metric workflow.</summary>
public static class FallbackPoolMetricWorkflow
{
    public static FallbackPoolMetricCoordinator NewFallbackPoolMetricCoordinator(IFallbackPoolMetricStore store, IClock clock)
    {
        return new FallbackPoolMetricCoordinator(
            store,
            FallbackPoolMetricRules.DefaultFallbackPoolMetricValidator(),
            clock,
            new List<string>());
    }
}
