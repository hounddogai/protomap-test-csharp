namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentCheckpointManager(IAdjustmentCheckpointStore Store, AdjustmentCheckpointValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentCheckpointStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentCheckpointStatus.Withdrawn);
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

/// <summary>Functions of adjustment checkpoint workflow.</summary>
public static class AdjustmentCheckpointWorkflow
{
    public static AdjustmentCheckpointManager NewAdjustmentCheckpointManager(IAdjustmentCheckpointStore store, IClock clock)
    {
        return new AdjustmentCheckpointManager(
            store,
            AdjustmentCheckpointRules.DefaultAdjustmentCheckpointValidator(),
            clock,
            new List<string>());
    }
}
