namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceBudgetWorkflow(IBalanceBudgetStore Store, BalanceBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    public static BalanceBudgetWorkflow NewBalanceBudgetWorkflow(IBalanceBudgetStore store, IClock clock)
    {
        return new BalanceBudgetWorkflow(
            store,
            BalanceBudgetRules.DefaultBalanceBudgetValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceBudgetStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceBudgetStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
