namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy adjustment batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyAdjustmentBatchService(ILegacyAdjustmentBatchStore Store, LegacyAdjustmentBatchValidator Validator, IClock Clock, List<string> Journal, LegacyAdjustmentBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy adjustment batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyAdjustmentBatchStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy adjustment batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyAdjustmentBatchStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy adjustment batch {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of legacy adjustment batch workflow.</summary>
public static class LegacyAdjustmentBatchWorkflow
{
    public static LegacyAdjustmentBatchService NewLegacyAdjustmentBatchService(ILegacyAdjustmentBatchStore store, IClock clock)
    {
        return new LegacyAdjustmentBatchService(
            store,
            LegacyAdjustmentBatchRules.DefaultLegacyAdjustmentBatchValidator(),
            clock,
            new List<string>(),
            LegacyAdjustmentBatchEngine.DefaultLegacyAdjustmentBatchEngine());
    }
}
