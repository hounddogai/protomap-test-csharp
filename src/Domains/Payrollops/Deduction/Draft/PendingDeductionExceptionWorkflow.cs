namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending deduction exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingDeductionExceptionCoordinator(IPendingDeductionExceptionStore Store, PendingDeductionExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending deduction exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingDeductionExceptionStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending deduction exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingDeductionExceptionStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of pending deduction exception workflow.</summary>
public static class PendingDeductionExceptionWorkflow
{
    public static PendingDeductionExceptionCoordinator NewPendingDeductionExceptionCoordinator(IPendingDeductionExceptionStore store, IClock clock)
    {
        return new PendingDeductionExceptionCoordinator(
            store,
            PendingDeductionExceptionRules.DefaultPendingDeductionExceptionValidator(),
            clock,
            new List<string>());
    }
}
