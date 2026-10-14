namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualCycleDesk(IAccrualCycleStore Store, AccrualCycleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualCycleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualCycleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of accrual cycle workflow.</summary>
public static class AccrualCycleWorkflow
{
    public static AccrualCycleDesk NewAccrualCycleDesk(IAccrualCycleStore store, IClock clock)
    {
        return new AccrualCycleDesk(
            store,
            AccrualCycleRules.DefaultAccrualCycleValidator(),
            clock,
            new List<string>());
    }
}
