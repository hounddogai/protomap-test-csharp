namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict accrual slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictAccrualSlotService(IStrictAccrualSlotStore Store, StrictAccrualSlotValidator Validator, IClock Clock, List<string> Journal, StrictAccrualSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict accrual slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAccrualSlotStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict accrual slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAccrualSlotStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict accrual slot {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of strict accrual slot workflow.</summary>
public static class StrictAccrualSlotWorkflow
{
    public static StrictAccrualSlotService NewStrictAccrualSlotService(IStrictAccrualSlotStore store, IClock clock)
    {
        return new StrictAccrualSlotService(
            store,
            StrictAccrualSlotRules.DefaultStrictAccrualSlotValidator(),
            clock,
            new List<string>(),
            StrictAccrualSlotEngine.DefaultStrictAccrualSlotEngine());
    }
}
