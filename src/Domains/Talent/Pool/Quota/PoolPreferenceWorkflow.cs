namespace Acme.Workforce.Talent.Pool.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pool preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolPreferenceCoordinator(IPoolPreferenceStore Store, PoolPreferenceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolPreferenceStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolPreferenceStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of pool preference workflow.</summary>
public static class PoolPreferenceWorkflow
{
    public static PoolPreferenceCoordinator NewPoolPreferenceCoordinator(IPoolPreferenceStore store, IClock clock)
    {
        return new PoolPreferenceCoordinator(
            store,
            PoolPreferenceRules.DefaultPoolPreferenceValidator(),
            clock,
            new List<string>());
    }
}
