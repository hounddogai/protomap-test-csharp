namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted deduction plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedDeductionPlanService(IWeightedDeductionPlanStore Store, WeightedDeductionPlanValidator Validator, IClock Clock, List<string> Journal, WeightedDeductionPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted deduction plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedDeductionPlanStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted deduction plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedDeductionPlanStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted deduction plan {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of weighted deduction plan workflow.</summary>
public static class WeightedDeductionPlanWorkflow
{
    public static WeightedDeductionPlanService NewWeightedDeductionPlanService(IWeightedDeductionPlanStore store, IClock clock, Registry flags)
    {
        return new WeightedDeductionPlanService(
            store,
            WeightedDeductionPlanRules.DefaultWeightedDeductionPlanValidator(),
            clock,
            new List<string>(),
            WeightedDeductionPlanEngine.DefaultWeightedDeductionPlanEngine(flags));
    }
}
