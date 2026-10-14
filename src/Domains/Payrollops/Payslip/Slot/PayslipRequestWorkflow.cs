namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves payslip requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipRequestCoordinator(IPayslipRequestStore Store, PayslipRequestValidator Validator, IClock Clock, List<string> Journal, PayslipRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipRequestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipRequestStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip request {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of payslip request workflow.</summary>
public static class PayslipRequestWorkflow
{
    public static PayslipRequestCoordinator NewPayslipRequestCoordinator(IPayslipRequestStore store, IClock clock, Registry flags)
    {
        return new PayslipRequestCoordinator(
            store,
            PayslipRequestRules.DefaultPayslipRequestValidator(),
            clock,
            new List<string>(),
            PayslipRequestEngine.DefaultPayslipRequestEngine(flags));
    }
}
