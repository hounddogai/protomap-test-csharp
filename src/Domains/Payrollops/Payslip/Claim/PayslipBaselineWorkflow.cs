namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves payslip baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipBaselineService(IPayslipBaselineStore Store, PayslipBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipBaselineStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipBaselineStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of payslip baseline workflow.</summary>
public static class PayslipBaselineWorkflow
{
    public static PayslipBaselineService NewPayslipBaselineService(IPayslipBaselineStore store, IClock clock)
    {
        return new PayslipBaselineService(
            store,
            PayslipBaselineRules.DefaultPayslipBaselineValidator(),
            clock,
            new List<string>());
    }
}
