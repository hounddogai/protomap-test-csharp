namespace Acme.Workforce.Absence.Accrual.AccrualSlot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal accrual batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalAccrualBatchWorkflow(ISeasonalAccrualBatchStore Store, SeasonalAccrualBatchValidator Validator, IClock Clock, List<string> Journal)
{
    public static SeasonalAccrualBatchWorkflow NewSeasonalAccrualBatchWorkflow(ISeasonalAccrualBatchStore store, IClock clock)
    {
        return new SeasonalAccrualBatchWorkflow(
            store,
            SeasonalAccrualBatchRules.DefaultSeasonalAccrualBatchValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal accrual batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAccrualBatchStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal accrual batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAccrualBatchStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
            if (item.StartsAt > cutoff)
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
