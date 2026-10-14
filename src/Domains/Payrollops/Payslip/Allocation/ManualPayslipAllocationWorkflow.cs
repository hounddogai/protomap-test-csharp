namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual payslip allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualPayslipAllocationDesk(IManualPayslipAllocationStore Store, ManualPayslipAllocationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual payslip allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualPayslipAllocationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual payslip allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualPayslipAllocationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of manual payslip allocation workflow.</summary>
public static class ManualPayslipAllocationWorkflow
{
    public static ManualPayslipAllocationDesk NewManualPayslipAllocationDesk(IManualPayslipAllocationStore store, IClock clock)
    {
        return new ManualPayslipAllocationDesk(
            store,
            ManualPayslipAllocationRules.DefaultManualPayslipAllocationValidator(),
            clock,
            new List<string>());
    }
}
