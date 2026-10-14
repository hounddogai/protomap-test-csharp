namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive deduction baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveDeductionBaselineDesk(IAdaptiveDeductionBaselineStore Store, AdaptiveDeductionBaselineValidator Validator, IClock Clock, List<string> Journal, AdaptiveDeductionBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive deduction baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveDeductionBaselineStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive deduction baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveDeductionBaselineStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive deduction baseline {code} not found");
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

/// <summary>Functions of adaptive deduction baseline workflow.</summary>
public static class AdaptiveDeductionBaselineWorkflow
{
    public static AdaptiveDeductionBaselineDesk NewAdaptiveDeductionBaselineDesk(IAdaptiveDeductionBaselineStore store, IClock clock)
    {
        return new AdaptiveDeductionBaselineDesk(
            store,
            AdaptiveDeductionBaselineRules.DefaultAdaptiveDeductionBaselineValidator(),
            clock,
            new List<string>(),
            AdaptiveDeductionBaselineEngine.DefaultAdaptiveDeductionBaselineEngine());
    }
}
