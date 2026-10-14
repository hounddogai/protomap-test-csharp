namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal engagement groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalEngagementGroupCoordinator(ISeasonalEngagementGroupStore Store, SeasonalEngagementGroupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal engagement group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalEngagementGroupStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal engagement group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalEngagementGroupStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of seasonal engagement group workflow.</summary>
public static class SeasonalEngagementGroupWorkflow
{
    public static SeasonalEngagementGroupCoordinator NewSeasonalEngagementGroupCoordinator(ISeasonalEngagementGroupStore store, IClock clock)
    {
        return new SeasonalEngagementGroupCoordinator(
            store,
            SeasonalEngagementGroupRules.DefaultSeasonalEngagementGroupValidator(),
            clock,
            new List<string>());
    }
}
