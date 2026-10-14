namespace Acme.Workforce.Absence.Accrual.AccrualSlot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient accrual slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientAccrualSlotService(ILenientAccrualSlotStore Store, LenientAccrualSlotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient accrual slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientAccrualSlotStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient accrual slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientAccrualSlotStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of lenient accrual slot workflow.</summary>
public static class LenientAccrualSlotWorkflow
{
    public static LenientAccrualSlotService NewLenientAccrualSlotService(ILenientAccrualSlotStore store, IClock clock)
    {
        return new LenientAccrualSlotService(
            store,
            LenientAccrualSlotRules.DefaultLenientAccrualSlotValidator(),
            clock,
            new List<string>());
    }
}
