namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive merit cycle reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveMeritCycleReservationManager(IAdaptiveMeritCycleReservationStore Store, AdaptiveMeritCycleReservationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive merit cycle reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveMeritCycleReservationStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive merit cycle reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveMeritCycleReservationStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of adaptive merit cycle reservation workflow.</summary>
public static class AdaptiveMeritCycleReservationWorkflow
{
    public static AdaptiveMeritCycleReservationManager NewAdaptiveMeritCycleReservationManager(IAdaptiveMeritCycleReservationStore store, IClock clock)
    {
        return new AdaptiveMeritCycleReservationManager(
            store,
            AdaptiveMeritCycleReservationRules.DefaultAdaptiveMeritCycleReservationValidator(),
            clock,
            new List<string>());
    }
}
