namespace Acme.Workforce.Compensation.MeritCycle.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves merit cycle forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleForecastService(IMeritCycleForecastStore Store, MeritCycleForecastValidator Validator, IClock Clock, List<string> Journal, MeritCycleForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleForecastStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleForecastStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle forecast {code} not found");
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

/// <summary>Functions of merit cycle forecast workflow.</summary>
public static class MeritCycleForecastWorkflow
{
    public static MeritCycleForecastService NewMeritCycleForecastService(IMeritCycleForecastStore store, IClock clock, Registry flags)
    {
        return new MeritCycleForecastService(
            store,
            MeritCycleForecastRules.DefaultMeritCycleForecastValidator(),
            clock,
            new List<string>(),
            MeritCycleForecastEngine.DefaultMeritCycleForecastEngine(flags));
    }
}
