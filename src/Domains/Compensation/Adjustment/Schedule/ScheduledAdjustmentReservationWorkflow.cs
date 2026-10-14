namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled adjustment reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledAdjustmentReservationCoordinator(IScheduledAdjustmentReservationStore Store, ScheduledAdjustmentReservationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled adjustment reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledAdjustmentReservationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled adjustment reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledAdjustmentReservationStatus.Expired);
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

/// <summary>Functions of scheduled adjustment reservation workflow.</summary>
public static class ScheduledAdjustmentReservationWorkflow
{
    public static ScheduledAdjustmentReservationCoordinator NewScheduledAdjustmentReservationCoordinator(IScheduledAdjustmentReservationStore store, IClock clock)
    {
        return new ScheduledAdjustmentReservationCoordinator(
            store,
            ScheduledAdjustmentReservationRules.DefaultScheduledAdjustmentReservationValidator(),
            clock,
            new List<string>());
    }
}
