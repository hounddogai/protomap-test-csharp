namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves floor plan milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanMilestoneService(IFloorPlanMilestoneStore Store, FloorPlanMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanMilestoneStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanMilestoneStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of floor plan milestone workflow.</summary>
public static class FloorPlanMilestoneWorkflow
{
    public static FloorPlanMilestoneService NewFloorPlanMilestoneService(IFloorPlanMilestoneStore store, IClock clock)
    {
        return new FloorPlanMilestoneService(
            store,
            FloorPlanMilestoneRules.DefaultFloorPlanMilestoneValidator(),
            clock,
            new List<string>());
    }
}
