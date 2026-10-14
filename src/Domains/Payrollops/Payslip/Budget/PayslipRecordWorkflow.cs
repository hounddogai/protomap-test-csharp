namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves payslip records through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipRecordManager(IPayslipRecordStore Store, PayslipRecordValidator Validator, IClock Clock, List<string> Journal, PayslipRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipRecordStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipRecordStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip record {code} not found");
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

/// <summary>Functions of payslip record workflow.</summary>
public static class PayslipRecordWorkflow
{
    public static PayslipRecordManager NewPayslipRecordManager(IPayslipRecordStore store, IClock clock, Registry flags)
    {
        return new PayslipRecordManager(
            store,
            PayslipRecordRules.DefaultPayslipRecordValidator(),
            clock,
            new List<string>(),
            PayslipRecordEngine.DefaultPayslipRecordEngine(flags));
    }
}
