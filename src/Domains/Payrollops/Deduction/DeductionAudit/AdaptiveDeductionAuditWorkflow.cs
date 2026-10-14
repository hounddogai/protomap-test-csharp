namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive deduction audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveDeductionAuditService(IAdaptiveDeductionAuditStore Store, AdaptiveDeductionAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive deduction audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveDeductionAuditStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive deduction audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveDeductionAuditStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of adaptive deduction audit workflow.</summary>
public static class AdaptiveDeductionAuditWorkflow
{
    public static AdaptiveDeductionAuditService NewAdaptiveDeductionAuditService(IAdaptiveDeductionAuditStore store, IClock clock)
    {
        return new AdaptiveDeductionAuditService(
            store,
            AdaptiveDeductionAuditRules.DefaultAdaptiveDeductionAuditValidator(),
            clock,
            new List<string>());
    }
}
