namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict holiday allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictHolidayAllocationManager(IStrictHolidayAllocationStore Store, StrictHolidayAllocationValidator Validator, IClock Clock, List<string> Journal, StrictHolidayAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict holiday allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictHolidayAllocationStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict holiday allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictHolidayAllocationStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict holiday allocation {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of strict holiday allocation workflow.</summary>
public static class StrictHolidayAllocationWorkflow
{
    public static StrictHolidayAllocationManager NewStrictHolidayAllocationManager(IStrictHolidayAllocationStore store, IClock clock)
    {
        return new StrictHolidayAllocationManager(
            store,
            StrictHolidayAllocationRules.DefaultStrictHolidayAllocationValidator(),
            clock,
            new List<string>(),
            StrictHolidayAllocationEngine.DefaultStrictHolidayAllocationEngine());
    }
}
