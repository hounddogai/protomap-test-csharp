namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves deduction batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeductionBatchCoordinator(IDeductionBatchStore Store, DeductionBatchValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeductionBatchStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"deduction batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeductionBatchStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of deduction batch workflow.</summary>
public static class DeductionBatchWorkflow
{
    public static DeductionBatchCoordinator NewDeductionBatchCoordinator(IDeductionBatchStore store, IClock clock)
    {
        return new DeductionBatchCoordinator(
            store,
            DeductionBatchRules.DefaultDeductionBatchValidator(),
            clock,
            new List<string>());
    }
}
