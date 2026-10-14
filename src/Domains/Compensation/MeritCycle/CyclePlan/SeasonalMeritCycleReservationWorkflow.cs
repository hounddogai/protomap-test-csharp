namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal merit cycle reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalMeritCycleReservationWorkflow(ISeasonalMeritCycleReservationStore Store, SeasonalMeritCycleReservationValidator Validator, IClock Clock, List<string> Journal, SeasonalMeritCycleReservationEngine Engine)
{
    public static SeasonalMeritCycleReservationWorkflow NewSeasonalMeritCycleReservationWorkflow(ISeasonalMeritCycleReservationStore store, IClock clock)
    {
        return new SeasonalMeritCycleReservationWorkflow(
            store,
            SeasonalMeritCycleReservationRules.DefaultSeasonalMeritCycleReservationValidator(),
            clock,
            new List<string>(),
            SeasonalMeritCycleReservationEngine.DefaultSeasonalMeritCycleReservationEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal merit cycle reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalMeritCycleReservationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal merit cycle reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalMeritCycleReservationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal merit cycle reservation {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
