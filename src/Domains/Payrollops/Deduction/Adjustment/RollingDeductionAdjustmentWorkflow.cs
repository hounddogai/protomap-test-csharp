namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling deduction adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingDeductionAdjustmentCoordinator(IRollingDeductionAdjustmentStore Store, RollingDeductionAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling deduction adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingDeductionAdjustmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling deduction adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingDeductionAdjustmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of rolling deduction adjustment workflow.</summary>
public static class RollingDeductionAdjustmentWorkflow
{
    public static RollingDeductionAdjustmentCoordinator NewRollingDeductionAdjustmentCoordinator(IRollingDeductionAdjustmentStore store, IClock clock)
    {
        return new RollingDeductionAdjustmentCoordinator(
            store,
            RollingDeductionAdjustmentRules.DefaultRollingDeductionAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
