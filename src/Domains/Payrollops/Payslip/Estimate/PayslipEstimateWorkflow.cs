namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves payslip estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipEstimateWorkflow(IPayslipEstimateStore Store, PayslipEstimateValidator Validator, IClock Clock, List<string> Journal, PayslipEstimateEngine Engine)
{
    public static PayslipEstimateWorkflow NewPayslipEstimateWorkflow(IPayslipEstimateStore store, IClock clock, Registry flags)
    {
        return new PayslipEstimateWorkflow(
            store,
            PayslipEstimateRules.DefaultPayslipEstimateValidator(),
            clock,
            new List<string>(),
            PayslipEstimateEngine.DefaultPayslipEstimateEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipEstimateStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipEstimateStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip estimate {code} not found");
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
