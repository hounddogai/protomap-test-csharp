namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling occupancy allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingOccupancyAllocationManager(IRollingOccupancyAllocationStore Store, RollingOccupancyAllocationValidator Validator, IClock Clock, List<string> Journal, RollingOccupancyAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling occupancy allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingOccupancyAllocationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling occupancy allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingOccupancyAllocationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling occupancy allocation {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of rolling occupancy allocation workflow.</summary>
public static class RollingOccupancyAllocationWorkflow
{
    public static RollingOccupancyAllocationManager NewRollingOccupancyAllocationManager(IRollingOccupancyAllocationStore store, IClock clock, Registry flags)
    {
        return new RollingOccupancyAllocationManager(
            store,
            RollingOccupancyAllocationRules.DefaultRollingOccupancyAllocationValidator(),
            clock,
            new List<string>(),
            RollingOccupancyAllocationEngine.DefaultRollingOccupancyAllocationEngine(flags));
    }
}
