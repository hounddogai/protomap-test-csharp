namespace Acme.Workforce.Payrollops.Payslip.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped payslip rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedPayslipRuleManager(ICappedPayslipRuleStore Store, CappedPayslipRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped payslip rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedPayslipRuleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped payslip rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedPayslipRuleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of capped payslip rule workflow.</summary>
public static class CappedPayslipRuleWorkflow
{
    public static CappedPayslipRuleManager NewCappedPayslipRuleManager(ICappedPayslipRuleStore store, IClock clock)
    {
        return new CappedPayslipRuleManager(
            store,
            CappedPayslipRuleRules.DefaultCappedPayslipRuleValidator(),
            clock,
            new List<string>());
    }
}
