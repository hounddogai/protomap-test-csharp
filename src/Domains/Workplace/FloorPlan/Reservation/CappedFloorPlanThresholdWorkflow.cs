namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped floor plan thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedFloorPlanThresholdDesk(ICappedFloorPlanThresholdStore Store, CappedFloorPlanThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped floor plan threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedFloorPlanThresholdStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped floor plan threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedFloorPlanThresholdStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of capped floor plan threshold workflow.</summary>
public static class CappedFloorPlanThresholdWorkflow
{
    public static CappedFloorPlanThresholdDesk NewCappedFloorPlanThresholdDesk(ICappedFloorPlanThresholdStore store, IClock clock)
    {
        return new CappedFloorPlanThresholdDesk(
            store,
            CappedFloorPlanThresholdRules.DefaultCappedFloorPlanThresholdValidator(),
            clock,
            new List<string>());
    }
}
