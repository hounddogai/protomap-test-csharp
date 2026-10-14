namespace Acme.Workforce.Payrollops.Payslip.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted payslip budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedPayslipBudgetManager(IWeightedPayslipBudgetStore Store, WeightedPayslipBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted payslip budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedPayslipBudgetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted payslip budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedPayslipBudgetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of weighted payslip budget workflow.</summary>
public static class WeightedPayslipBudgetWorkflow
{
    public static WeightedPayslipBudgetManager NewWeightedPayslipBudgetManager(IWeightedPayslipBudgetStore store, IClock clock)
    {
        return new WeightedPayslipBudgetManager(
            store,
            WeightedPayslipBudgetRules.DefaultWeightedPayslipBudgetValidator(),
            clock,
            new List<string>());
    }
}
