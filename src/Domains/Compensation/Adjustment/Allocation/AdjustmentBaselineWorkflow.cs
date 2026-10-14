namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentBaselineWorkflow(IAdjustmentBaselineStore Store, AdjustmentBaselineValidator Validator, IClock Clock, List<string> Journal, AdjustmentBaselineEngine Engine)
{
    public static AdjustmentBaselineWorkflow NewAdjustmentBaselineWorkflow(IAdjustmentBaselineStore store, IClock clock)
    {
        return new AdjustmentBaselineWorkflow(
            store,
            AdjustmentBaselineRules.DefaultAdjustmentBaselineValidator(),
            clock,
            new List<string>(),
            AdjustmentBaselineEngine.DefaultAdjustmentBaselineEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentBaselineStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentBaselineStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment baseline {code} not found");
        }
        var result = Engine.Compute(item);
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
