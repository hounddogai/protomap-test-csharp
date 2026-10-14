namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback occupancy adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackOccupancyAdjustmentCoordinator(IFallbackOccupancyAdjustmentStore Store, FallbackOccupancyAdjustmentValidator Validator, IClock Clock, List<string> Journal, FallbackOccupancyAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback occupancy adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackOccupancyAdjustmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback occupancy adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackOccupancyAdjustmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback occupancy adjustment {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of fallback occupancy adjustment workflow.</summary>
public static class FallbackOccupancyAdjustmentWorkflow
{
    public static FallbackOccupancyAdjustmentCoordinator NewFallbackOccupancyAdjustmentCoordinator(IFallbackOccupancyAdjustmentStore store, IClock clock, Registry flags)
    {
        return new FallbackOccupancyAdjustmentCoordinator(
            store,
            FallbackOccupancyAdjustmentRules.DefaultFallbackOccupancyAdjustmentValidator(),
            clock,
            new List<string>(),
            FallbackOccupancyAdjustmentEngine.DefaultFallbackOccupancyAdjustmentEngine(flags));
    }
}
