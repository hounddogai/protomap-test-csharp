namespace Acme.Workforce.Compensation.Adjustment.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional adjustment reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalAdjustmentReservationDesk(IRegionalAdjustmentReservationStore Store, RegionalAdjustmentReservationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional adjustment reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalAdjustmentReservationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional adjustment reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalAdjustmentReservationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of regional adjustment reservation workflow.</summary>
public static class RegionalAdjustmentReservationWorkflow
{
    public static RegionalAdjustmentReservationDesk NewRegionalAdjustmentReservationDesk(IRegionalAdjustmentReservationStore store, IClock clock)
    {
        return new RegionalAdjustmentReservationDesk(
            store,
            RegionalAdjustmentReservationRules.DefaultRegionalAdjustmentReservationValidator(),
            clock,
            new List<string>());
    }
}
