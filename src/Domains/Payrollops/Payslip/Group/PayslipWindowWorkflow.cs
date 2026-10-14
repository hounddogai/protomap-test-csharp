namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves payslip windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipWindowManager(IPayslipWindowStore Store, PayslipWindowValidator Validator, IClock Clock, List<string> Journal, PayslipWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipWindowStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipWindowStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip window {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of payslip window workflow.</summary>
public static class PayslipWindowWorkflow
{
    public static PayslipWindowManager NewPayslipWindowManager(IPayslipWindowStore store, IClock clock)
    {
        return new PayslipWindowManager(
            store,
            PayslipWindowRules.DefaultPayslipWindowValidator(),
            clock,
            new List<string>(),
            PayslipWindowEngine.DefaultPayslipWindowEngine());
    }
}
