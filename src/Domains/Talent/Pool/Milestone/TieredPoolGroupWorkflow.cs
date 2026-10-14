namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered pool groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredPoolGroupDesk(ITieredPoolGroupStore Store, TieredPoolGroupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered pool group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredPoolGroupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered pool group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredPoolGroupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of tiered pool group workflow.</summary>
public static class TieredPoolGroupWorkflow
{
    public static TieredPoolGroupDesk NewTieredPoolGroupDesk(ITieredPoolGroupStore store, IClock clock)
    {
        return new TieredPoolGroupDesk(
            store,
            TieredPoolGroupRules.DefaultTieredPoolGroupValidator(),
            clock,
            new List<string>());
    }
}
