namespace Acme.Workforce.Workplace.Occupancy.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves occupancy plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyPlanDesk(IOccupancyPlanStore Store, OccupancyPlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyPlanStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyPlanStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of occupancy plan workflow.</summary>
public static class OccupancyPlanWorkflow
{
    public static OccupancyPlanDesk NewOccupancyPlanDesk(IOccupancyPlanStore store, IClock clock)
    {
        return new OccupancyPlanDesk(
            store,
            OccupancyPlanRules.DefaultOccupancyPlanValidator(),
            clock,
            new List<string>());
    }
}
