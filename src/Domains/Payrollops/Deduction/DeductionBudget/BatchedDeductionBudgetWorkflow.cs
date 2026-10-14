namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched deduction budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedDeductionBudgetDesk(IBatchedDeductionBudgetStore Store, BatchedDeductionBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched deduction budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedDeductionBudgetStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched deduction budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedDeductionBudgetStatus.Expired);
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

/// <summary>Functions of batched deduction budget workflow.</summary>
public static class BatchedDeductionBudgetWorkflow
{
    public static BatchedDeductionBudgetDesk NewBatchedDeductionBudgetDesk(IBatchedDeductionBudgetStore store, IClock clock)
    {
        return new BatchedDeductionBudgetDesk(
            store,
            BatchedDeductionBudgetRules.DefaultBatchedDeductionBudgetValidator(),
            clock,
            new List<string>());
    }
}
