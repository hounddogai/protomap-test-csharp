namespace Acme.Workforce.Assets.Refresh.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal refresh allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalRefreshAllocationDesk(ISeasonalRefreshAllocationStore Store, SeasonalRefreshAllocationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal refresh allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalRefreshAllocationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal refresh allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalRefreshAllocationStatus.Expired);
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

/// <summary>Functions of seasonal refresh allocation workflow.</summary>
public static class SeasonalRefreshAllocationWorkflow
{
    public static SeasonalRefreshAllocationDesk NewSeasonalRefreshAllocationDesk(ISeasonalRefreshAllocationStore store, IClock clock)
    {
        return new SeasonalRefreshAllocationDesk(
            store,
            SeasonalRefreshAllocationRules.DefaultSeasonalRefreshAllocationValidator(),
            clock,
            new List<string>());
    }
}
