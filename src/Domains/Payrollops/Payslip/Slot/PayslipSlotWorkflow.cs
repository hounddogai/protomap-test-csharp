namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves payslip slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipSlotWorkflow(IPayslipSlotStore Store, PayslipSlotValidator Validator, IClock Clock, List<string> Journal, PayslipSlotEngine Engine)
{
    public static PayslipSlotWorkflow NewPayslipSlotWorkflow(IPayslipSlotStore store, IClock clock, Registry flags)
    {
        return new PayslipSlotWorkflow(
            store,
            PayslipSlotRules.DefaultPayslipSlotValidator(),
            clock,
            new List<string>(),
            PayslipSlotEngine.DefaultPayslipSlotEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipSlotStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipSlotStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip slot {code} not found");
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
