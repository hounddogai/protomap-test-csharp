namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal floor plan batches through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalFloorPlanBatchWorkflow(ISeasonalFloorPlanBatchStore Store, SeasonalFloorPlanBatchValidator Validator, IClock Clock, List<string> Journal, SeasonalFloorPlanBatchEngine Engine)
{
    public static SeasonalFloorPlanBatchWorkflow NewSeasonalFloorPlanBatchWorkflow(ISeasonalFloorPlanBatchStore store, IClock clock)
    {
        return new SeasonalFloorPlanBatchWorkflow(
            store,
            SeasonalFloorPlanBatchRules.DefaultSeasonalFloorPlanBatchValidator(),
            clock,
            new List<string>(),
            SeasonalFloorPlanBatchEngine.DefaultSeasonalFloorPlanBatchEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal floor plan batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalFloorPlanBatchStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal floor plan batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalFloorPlanBatchStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal floor plan batch {code} not found");
        }
        var result = Engine.Compute(item);
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
