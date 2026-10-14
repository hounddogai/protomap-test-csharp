namespace Acme.Workforce.Payrollops.Payslip.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves payslip schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipScheduleCoordinator(IPayslipScheduleStore Store, PayslipScheduleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipScheduleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipScheduleStatus.Cancelled);
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

/// <summary>Functions of payslip schedule workflow.</summary>
public static class PayslipScheduleWorkflow
{
    public static PayslipScheduleCoordinator NewPayslipScheduleCoordinator(IPayslipScheduleStore store, IClock clock)
    {
        return new PayslipScheduleCoordinator(
            store,
            PayslipScheduleRules.DefaultPayslipScheduleValidator(),
            clock,
            new List<string>());
    }
}
