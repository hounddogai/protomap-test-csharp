namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy nomination reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyNominationReservationDesk(ILegacyNominationReservationStore Store, LegacyNominationReservationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy nomination reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyNominationReservationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy nomination reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyNominationReservationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of legacy nomination reservation workflow.</summary>
public static class LegacyNominationReservationWorkflow
{
    public static LegacyNominationReservationDesk NewLegacyNominationReservationDesk(ILegacyNominationReservationStore store, IClock clock)
    {
        return new LegacyNominationReservationDesk(
            store,
            LegacyNominationReservationRules.DefaultLegacyNominationReservationValidator(),
            clock,
            new List<string>());
    }
}
