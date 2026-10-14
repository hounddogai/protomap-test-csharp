namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves payslip rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipRuleService(IPayslipRuleStore Store, PayslipRuleValidator Validator, IClock Clock, List<string> Journal, PayslipRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipRuleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipRuleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip rule {code} not found");
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

/// <summary>Functions of payslip rule workflow.</summary>
public static class PayslipRuleWorkflow
{
    public static PayslipRuleService NewPayslipRuleService(IPayslipRuleStore store, IClock clock, Registry flags)
    {
        return new PayslipRuleService(
            store,
            PayslipRuleRules.DefaultPayslipRuleValidator(),
            clock,
            new List<string>(),
            PayslipRuleEngine.DefaultPayslipRuleEngine(flags));
    }
}
