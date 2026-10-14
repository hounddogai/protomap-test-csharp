namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered mobility reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredMobilityReservationManager(ITieredMobilityReservationStore Store, TieredMobilityReservationValidator Validator, IClock Clock, List<string> Journal, TieredMobilityReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered mobility reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredMobilityReservationStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered mobility reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredMobilityReservationStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered mobility reservation {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of tiered mobility reservation workflow.</summary>
public static class TieredMobilityReservationWorkflow
{
    public static TieredMobilityReservationManager NewTieredMobilityReservationManager(ITieredMobilityReservationStore store, IClock clock)
    {
        return new TieredMobilityReservationManager(
            store,
            TieredMobilityReservationRules.DefaultTieredMobilityReservationValidator(),
            clock,
            new List<string>(),
            TieredMobilityReservationEngine.DefaultTieredMobilityReservationEngine());
    }
}
