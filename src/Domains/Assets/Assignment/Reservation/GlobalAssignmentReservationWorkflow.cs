namespace Acme.Workforce.Assets.Assignment.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global assignment reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalAssignmentReservationService(IGlobalAssignmentReservationStore Store, GlobalAssignmentReservationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global assignment reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalAssignmentReservationStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global assignment reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalAssignmentReservationStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of global assignment reservation workflow.</summary>
public static class GlobalAssignmentReservationWorkflow
{
    public static GlobalAssignmentReservationService NewGlobalAssignmentReservationService(IGlobalAssignmentReservationStore store, IClock clock)
    {
        return new GlobalAssignmentReservationService(
            store,
            GlobalAssignmentReservationRules.DefaultGlobalAssignmentReservationValidator(),
            clock,
            new List<string>());
    }
}
