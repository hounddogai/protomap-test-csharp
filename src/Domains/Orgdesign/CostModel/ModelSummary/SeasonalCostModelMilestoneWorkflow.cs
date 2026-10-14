namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal cost model milestones through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalCostModelMilestoneCoordinator(ISeasonalCostModelMilestoneStore Store, SeasonalCostModelMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal cost model milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalCostModelMilestoneStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal cost model milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalCostModelMilestoneStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of seasonal cost model milestone workflow.</summary>
public static class SeasonalCostModelMilestoneWorkflow
{
    public static SeasonalCostModelMilestoneCoordinator NewSeasonalCostModelMilestoneCoordinator(ISeasonalCostModelMilestoneStore store, IClock clock)
    {
        return new SeasonalCostModelMilestoneCoordinator(
            store,
            SeasonalCostModelMilestoneRules.DefaultSeasonalCostModelMilestoneValidator(),
            clock,
            new List<string>());
    }
}
