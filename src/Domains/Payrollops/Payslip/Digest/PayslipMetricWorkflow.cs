namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves payslip metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipMetricDesk(IPayslipMetricStore Store, PayslipMetricValidator Validator, IClock Clock, List<string> Journal, PayslipMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipMetricStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipMetricStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip metric {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of payslip metric workflow.</summary>
public static class PayslipMetricWorkflow
{
    public static PayslipMetricDesk NewPayslipMetricDesk(IPayslipMetricStore store, IClock clock, Registry flags)
    {
        return new PayslipMetricDesk(
            store,
            PayslipMetricRules.DefaultPayslipMetricValidator(),
            clock,
            new List<string>(),
            PayslipMetricEngine.DefaultPayslipMetricEngine(flags));
    }
}
