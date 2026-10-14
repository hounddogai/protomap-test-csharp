namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves floor plan quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanQuotaService(IFloorPlanQuotaStore Store, FloorPlanQuotaValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanQuotaStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanQuotaStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of floor plan quota workflow.</summary>
public static class FloorPlanQuotaWorkflow
{
    public static FloorPlanQuotaService NewFloorPlanQuotaService(IFloorPlanQuotaStore store, IClock clock)
    {
        return new FloorPlanQuotaService(
            store,
            FloorPlanQuotaRules.DefaultFloorPlanQuotaValidator(),
            clock,
            new List<string>());
    }
}
