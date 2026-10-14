namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal engagement milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalEngagementMilestoneManager(ISeasonalEngagementMilestoneStore Store, SeasonalEngagementMilestoneValidator Validator, IClock Clock, List<string> Journal, SeasonalEngagementMilestoneEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal engagement milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalEngagementMilestoneStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal engagement milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalEngagementMilestoneStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal engagement milestone {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of seasonal engagement milestone workflow.</summary>
public static class SeasonalEngagementMilestoneWorkflow
{
    public static SeasonalEngagementMilestoneManager NewSeasonalEngagementMilestoneManager(ISeasonalEngagementMilestoneStore store, IClock clock)
    {
        return new SeasonalEngagementMilestoneManager(
            store,
            SeasonalEngagementMilestoneRules.DefaultSeasonalEngagementMilestoneValidator(),
            clock,
            new List<string>(),
            SeasonalEngagementMilestoneEngine.DefaultSeasonalEngagementMilestoneEngine());
    }
}
