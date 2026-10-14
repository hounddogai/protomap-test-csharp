namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves payslip exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayslipExceptionDesk(IPayslipExceptionStore Store, PayslipExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayslipExceptionStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"payslip exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayslipExceptionStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of payslip exception workflow.</summary>
public static class PayslipExceptionWorkflow
{
    public static PayslipExceptionDesk NewPayslipExceptionDesk(IPayslipExceptionStore store, IClock clock)
    {
        return new PayslipExceptionDesk(
            store,
            PayslipExceptionRules.DefaultPayslipExceptionValidator(),
            clock,
            new List<string>());
    }
}
