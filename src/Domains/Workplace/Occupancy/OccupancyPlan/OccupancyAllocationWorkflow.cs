namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves occupancy allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyAllocationWorkflow(IOccupancyAllocationStore Store, OccupancyAllocationValidator Validator, IClock Clock, List<string> Journal, OccupancyAllocationEngine Engine)
{
    public static OccupancyAllocationWorkflow NewOccupancyAllocationWorkflow(IOccupancyAllocationStore store, IClock clock)
    {
        return new OccupancyAllocationWorkflow(
            store,
            OccupancyAllocationRules.DefaultOccupancyAllocationValidator(),
            clock,
            new List<string>(),
            OccupancyAllocationEngine.DefaultOccupancyAllocationEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyAllocationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyAllocationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy allocation {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.UpdatedAt > cutoff)
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
