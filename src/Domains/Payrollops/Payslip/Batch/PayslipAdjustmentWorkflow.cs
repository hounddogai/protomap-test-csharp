namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves payslip adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipAdjustmentDesk(IPayslipAdjustmentStore Store, PayslipAdjustmentValidator Validator, IClock Clock, List<string> Journal, PayslipAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipAdjustmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipAdjustmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip adjustment {code} not found");
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

/// <summary>Functions of payslip adjustment workflow.</summary>
public static class PayslipAdjustmentWorkflow
{
    public static PayslipAdjustmentDesk NewPayslipAdjustmentDesk(IPayslipAdjustmentStore store, IClock clock, Registry flags)
    {
        return new PayslipAdjustmentDesk(
            store,
            PayslipAdjustmentRules.DefaultPayslipAdjustmentValidator(),
            clock,
            new List<string>(),
            PayslipAdjustmentEngine.DefaultPayslipAdjustmentEngine(flags));
    }
}
