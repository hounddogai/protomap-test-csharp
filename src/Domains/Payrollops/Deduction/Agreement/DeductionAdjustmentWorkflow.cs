namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves deduction adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionAdjustmentCoordinator(IDeductionAdjustmentStore Store, DeductionAdjustmentValidator Validator, IClock Clock, List<string> Journal, DeductionAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionAdjustmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionAdjustmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction adjustment {code} not found");
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

/// <summary>Functions of deduction adjustment workflow.</summary>
public static class DeductionAdjustmentWorkflow
{
    public static DeductionAdjustmentCoordinator NewDeductionAdjustmentCoordinator(IDeductionAdjustmentStore store, IClock clock, Registry flags)
    {
        return new DeductionAdjustmentCoordinator(
            store,
            DeductionAdjustmentRules.DefaultDeductionAdjustmentValidator(),
            clock,
            new List<string>(),
            DeductionAdjustmentEngine.DefaultDeductionAdjustmentEngine(flags));
    }
}
