namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled payslip notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledPayslipNoticeWorkflow(IScheduledPayslipNoticeStore Store, ScheduledPayslipNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    public static ScheduledPayslipNoticeWorkflow NewScheduledPayslipNoticeWorkflow(IScheduledPayslipNoticeStore store, IClock clock)
    {
        return new ScheduledPayslipNoticeWorkflow(
            store,
            ScheduledPayslipNoticeRules.DefaultScheduledPayslipNoticeValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled payslip notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPayslipNoticeStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled payslip notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPayslipNoticeStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
