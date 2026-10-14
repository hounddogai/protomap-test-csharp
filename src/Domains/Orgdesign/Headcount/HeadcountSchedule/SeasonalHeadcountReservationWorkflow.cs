namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal headcount reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalHeadcountReservationDesk(ISeasonalHeadcountReservationStore Store, SeasonalHeadcountReservationValidator Validator, IClock Clock, List<string> Journal, SeasonalHeadcountReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal headcount reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalHeadcountReservationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal headcount reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalHeadcountReservationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal headcount reservation {code} not found");
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

/// <summary>Functions of seasonal headcount reservation workflow.</summary>
public static class SeasonalHeadcountReservationWorkflow
{
    public static SeasonalHeadcountReservationDesk NewSeasonalHeadcountReservationDesk(ISeasonalHeadcountReservationStore store, IClock clock)
    {
        return new SeasonalHeadcountReservationDesk(
            store,
            SeasonalHeadcountReservationRules.DefaultSeasonalHeadcountReservationValidator(),
            clock,
            new List<string>(),
            SeasonalHeadcountReservationEngine.DefaultSeasonalHeadcountReservationEngine());
    }
}
