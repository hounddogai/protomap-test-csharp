namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves payslip notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipNoticeCoordinator(IPayslipNoticeStore Store, PayslipNoticeValidator Validator, IClock Clock, List<string> Journal, PayslipNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipNoticeStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipNoticeStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip notice {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of payslip notice workflow.</summary>
public static class PayslipNoticeWorkflow
{
    public static PayslipNoticeCoordinator NewPayslipNoticeCoordinator(IPayslipNoticeStore store, IClock clock, Registry flags)
    {
        return new PayslipNoticeCoordinator(
            store,
            PayslipNoticeRules.DefaultPayslipNoticeValidator(),
            clock,
            new List<string>(),
            PayslipNoticeEngine.DefaultPayslipNoticeEngine(flags));
    }
}
