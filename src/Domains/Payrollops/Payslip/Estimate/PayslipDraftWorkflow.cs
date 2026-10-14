namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves payslip drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipDraftManager(IPayslipDraftStore Store, PayslipDraftValidator Validator, IClock Clock, List<string> Journal, PayslipDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipDraftStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipDraftStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip draft {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of payslip draft workflow.</summary>
public static class PayslipDraftWorkflow
{
    public static PayslipDraftManager NewPayslipDraftManager(IPayslipDraftStore store, IClock clock)
    {
        return new PayslipDraftManager(
            store,
            PayslipDraftRules.DefaultPayslipDraftValidator(),
            clock,
            new List<string>(),
            PayslipDraftEngine.DefaultPayslipDraftEngine());
    }
}
