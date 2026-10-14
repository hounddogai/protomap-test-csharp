namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves deduction exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionExceptionService(IDeductionExceptionStore Store, DeductionExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionExceptionStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionExceptionStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of deduction exception workflow.</summary>
public static class DeductionExceptionWorkflow
{
    public static DeductionExceptionService NewDeductionExceptionService(IDeductionExceptionStore store, IClock clock)
    {
        return new DeductionExceptionService(
            store,
            DeductionExceptionRules.DefaultDeductionExceptionValidator(),
            clock,
            new List<string>());
    }
}
