namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves payslip allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipAllocationService(IPayslipAllocationStore Store, PayslipAllocationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipAllocationStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipAllocationStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of payslip allocation workflow.</summary>
public static class PayslipAllocationWorkflow
{
    public static PayslipAllocationService NewPayslipAllocationService(IPayslipAllocationStore store, IClock clock)
    {
        return new PayslipAllocationService(
            store,
            PayslipAllocationRules.DefaultPayslipAllocationValidator(),
            clock,
            new List<string>());
    }
}
