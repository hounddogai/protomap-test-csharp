namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal floor plan records through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalFloorPlanRecordDesk(ISeasonalFloorPlanRecordStore Store, SeasonalFloorPlanRecordValidator Validator, IClock Clock, List<string> Journal, SeasonalFloorPlanRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal floor plan record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalFloorPlanRecordStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal floor plan record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalFloorPlanRecordStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal floor plan record {code} not found");
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

/// <summary>Functions of seasonal floor plan record workflow.</summary>
public static class SeasonalFloorPlanRecordWorkflow
{
    public static SeasonalFloorPlanRecordDesk NewSeasonalFloorPlanRecordDesk(ISeasonalFloorPlanRecordStore store, IClock clock, Registry flags)
    {
        return new SeasonalFloorPlanRecordDesk(
            store,
            SeasonalFloorPlanRecordRules.DefaultSeasonalFloorPlanRecordValidator(),
            clock,
            new List<string>(),
            SeasonalFloorPlanRecordEngine.DefaultSeasonalFloorPlanRecordEngine(flags));
    }
}
