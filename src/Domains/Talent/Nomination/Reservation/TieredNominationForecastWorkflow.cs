namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered nomination forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredNominationForecastManager(ITieredNominationForecastStore Store, TieredNominationForecastValidator Validator, IClock Clock, List<string> Journal, TieredNominationForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered nomination forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredNominationForecastStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered nomination forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredNominationForecastStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered nomination forecast {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of tiered nomination forecast workflow.</summary>
public static class TieredNominationForecastWorkflow
{
    public static TieredNominationForecastManager NewTieredNominationForecastManager(ITieredNominationForecastStore store, IClock clock, Registry flags)
    {
        return new TieredNominationForecastManager(
            store,
            TieredNominationForecastRules.DefaultTieredNominationForecastValidator(),
            clock,
            new List<string>(),
            TieredNominationForecastEngine.DefaultTieredNominationForecastEngine(flags));
    }
}
