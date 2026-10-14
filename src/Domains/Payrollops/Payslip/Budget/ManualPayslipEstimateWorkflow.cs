namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual payslip estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualPayslipEstimateDesk(IManualPayslipEstimateStore Store, ManualPayslipEstimateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual payslip estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualPayslipEstimateStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual payslip estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualPayslipEstimateStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of manual payslip estimate workflow.</summary>
public static class ManualPayslipEstimateWorkflow
{
    public static ManualPayslipEstimateDesk NewManualPayslipEstimateDesk(IManualPayslipEstimateStore store, IClock clock)
    {
        return new ManualPayslipEstimateDesk(
            store,
            ManualPayslipEstimateRules.DefaultManualPayslipEstimateValidator(),
            clock,
            new List<string>());
    }
}
