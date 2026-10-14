namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adjustment segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentSegmentCoordinator(IAdjustmentSegmentStore Store, AdjustmentSegmentValidator Validator, IClock Clock, List<string> Journal, AdjustmentSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentSegmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentSegmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment segment {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of adjustment segment workflow.</summary>
public static class AdjustmentSegmentWorkflow
{
    public static AdjustmentSegmentCoordinator NewAdjustmentSegmentCoordinator(IAdjustmentSegmentStore store, IClock clock, Registry flags)
    {
        return new AdjustmentSegmentCoordinator(
            store,
            AdjustmentSegmentRules.DefaultAdjustmentSegmentValidator(),
            clock,
            new List<string>(),
            AdjustmentSegmentEngine.DefaultAdjustmentSegmentEngine(flags));
    }
}
