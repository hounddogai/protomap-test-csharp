namespace Acme.Workforce.Payrollops.Deduction.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves deduction budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionBudgetService(IDeductionBudgetStore Store, DeductionBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionBudgetStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionBudgetStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of deduction budget workflow.</summary>
public static class DeductionBudgetWorkflow
{
    public static DeductionBudgetService NewDeductionBudgetService(IDeductionBudgetStore store, IClock clock)
    {
        return new DeductionBudgetService(
            store,
            DeductionBudgetRules.DefaultDeductionBudgetValidator(),
            clock,
            new List<string>());
    }
}
