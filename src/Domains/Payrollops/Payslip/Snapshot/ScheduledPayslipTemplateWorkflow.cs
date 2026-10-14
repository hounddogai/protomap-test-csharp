namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled payslip templates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledPayslipTemplateDesk(IScheduledPayslipTemplateStore Store, ScheduledPayslipTemplateValidator Validator, IClock Clock, List<string> Journal, ScheduledPayslipTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled payslip template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPayslipTemplateStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled payslip template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPayslipTemplateStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled payslip template {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of scheduled payslip template workflow.</summary>
public static class ScheduledPayslipTemplateWorkflow
{
    public static ScheduledPayslipTemplateDesk NewScheduledPayslipTemplateDesk(IScheduledPayslipTemplateStore store, IClock clock)
    {
        return new ScheduledPayslipTemplateDesk(
            store,
            ScheduledPayslipTemplateRules.DefaultScheduledPayslipTemplateValidator(),
            clock,
            new List<string>(),
            ScheduledPayslipTemplateEngine.DefaultScheduledPayslipTemplateEngine());
    }
}
