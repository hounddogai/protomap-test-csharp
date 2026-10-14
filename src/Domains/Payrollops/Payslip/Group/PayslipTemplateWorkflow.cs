namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves payslip templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipTemplateCoordinator(IPayslipTemplateStore Store, PayslipTemplateValidator Validator, IClock Clock, List<string> Journal, PayslipTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipTemplateStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipTemplateStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip template {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of payslip template workflow.</summary>
public static class PayslipTemplateWorkflow
{
    public static PayslipTemplateCoordinator NewPayslipTemplateCoordinator(IPayslipTemplateStore store, IClock clock, Registry flags)
    {
        return new PayslipTemplateCoordinator(
            store,
            PayslipTemplateRules.DefaultPayslipTemplateValidator(),
            clock,
            new List<string>(),
            PayslipTemplateEngine.DefaultPayslipTemplateEngine(flags));
    }
}
