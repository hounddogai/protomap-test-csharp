namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves payslip quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipQuotaService(IPayslipQuotaStore Store, PayslipQuotaValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipQuotaStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipQuotaStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of payslip quota workflow.</summary>
public static class PayslipQuotaWorkflow
{
    public static PayslipQuotaService NewPayslipQuotaService(IPayslipQuotaStore store, IClock clock)
    {
        return new PayslipQuotaService(
            store,
            PayslipQuotaRules.DefaultPayslipQuotaValidator(),
            clock,
            new List<string>());
    }
}
