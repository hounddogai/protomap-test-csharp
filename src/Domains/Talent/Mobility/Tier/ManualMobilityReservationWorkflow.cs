namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual mobility reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualMobilityReservationCoordinator(IManualMobilityReservationStore Store, ManualMobilityReservationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual mobility reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualMobilityReservationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual mobility reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualMobilityReservationStatus.Stopped);
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

/// <summary>Functions of manual mobility reservation workflow.</summary>
public static class ManualMobilityReservationWorkflow
{
    public static ManualMobilityReservationCoordinator NewManualMobilityReservationCoordinator(IManualMobilityReservationStore store, IClock clock)
    {
        return new ManualMobilityReservationCoordinator(
            store,
            ManualMobilityReservationRules.DefaultManualMobilityReservationValidator(),
            clock,
            new List<string>());
    }
}
