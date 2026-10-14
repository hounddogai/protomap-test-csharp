namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adjustment assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentAssignmentService(IAdjustmentAssignmentStore Store, AdjustmentAssignmentValidator Validator, IClock Clock, List<string> Journal, AdjustmentAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentAssignmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentAssignmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment assignment {code} not found");
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

/// <summary>Functions of adjustment assignment workflow.</summary>
public static class AdjustmentAssignmentWorkflow
{
    public static AdjustmentAssignmentService NewAdjustmentAssignmentService(IAdjustmentAssignmentStore store, IClock clock, Registry flags)
    {
        return new AdjustmentAssignmentService(
            store,
            AdjustmentAssignmentRules.DefaultAdjustmentAssignmentValidator(),
            clock,
            new List<string>(),
            AdjustmentAssignmentEngine.DefaultAdjustmentAssignmentEngine(flags));
    }
}
