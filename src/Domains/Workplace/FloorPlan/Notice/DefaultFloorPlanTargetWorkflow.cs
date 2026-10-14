namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default floor plan targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultFloorPlanTargetCoordinator(IDefaultFloorPlanTargetStore Store, DefaultFloorPlanTargetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default floor plan target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultFloorPlanTargetStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default floor plan target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultFloorPlanTargetStatus.Cancelled);
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

/// <summary>Functions of default floor plan target workflow.</summary>
public static class DefaultFloorPlanTargetWorkflow
{
    public static DefaultFloorPlanTargetCoordinator NewDefaultFloorPlanTargetCoordinator(IDefaultFloorPlanTargetStore store, IClock clock)
    {
        return new DefaultFloorPlanTargetCoordinator(
            store,
            DefaultFloorPlanTargetRules.DefaultDefaultFloorPlanTargetValidator(),
            clock,
            new List<string>());
    }
}
