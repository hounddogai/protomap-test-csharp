namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves accrual slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualSlotDesk(IAccrualSlotStore Store, AccrualSlotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualSlotStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualSlotStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of accrual slot workflow.</summary>
public static class AccrualSlotWorkflow
{
    public static AccrualSlotDesk NewAccrualSlotDesk(IAccrualSlotStore store, IClock clock)
    {
        return new AccrualSlotDesk(store, AccrualSlotRules.DefaultAccrualSlotValidator(), clock, new List<string>());
    }
}
