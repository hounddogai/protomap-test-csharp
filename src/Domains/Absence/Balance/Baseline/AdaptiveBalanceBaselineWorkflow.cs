namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive balance baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveBalanceBaselineDesk(IAdaptiveBalanceBaselineStore Store, AdaptiveBalanceBaselineValidator Validator, IClock Clock, List<string> Journal, AdaptiveBalanceBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive balance baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveBalanceBaselineStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive balance baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveBalanceBaselineStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive balance baseline {code} not found");
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

/// <summary>Functions of adaptive balance baseline workflow.</summary>
public static class AdaptiveBalanceBaselineWorkflow
{
    public static AdaptiveBalanceBaselineDesk NewAdaptiveBalanceBaselineDesk(IAdaptiveBalanceBaselineStore store, IClock clock)
    {
        return new AdaptiveBalanceBaselineDesk(
            store,
            AdaptiveBalanceBaselineRules.DefaultAdaptiveBalanceBaselineValidator(),
            clock,
            new List<string>(),
            AdaptiveBalanceBaselineEngine.DefaultAdaptiveBalanceBaselineEngine());
    }
}
