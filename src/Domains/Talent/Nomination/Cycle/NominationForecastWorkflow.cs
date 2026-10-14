namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves nomination forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record NominationForecastService(INominationForecastStore Store, NominationForecastValidator Validator, IClock Clock, List<string> Journal, NominationForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(NominationForecastStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(NominationForecastStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"nomination forecast {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of nomination forecast workflow.</summary>
public static class NominationForecastWorkflow
{
    public static NominationForecastService NewNominationForecastService(INominationForecastStore store, IClock clock)
    {
        return new NominationForecastService(
            store,
            NominationForecastRules.DefaultNominationForecastValidator(),
            clock,
            new List<string>(),
            NominationForecastEngine.DefaultNominationForecastEngine());
    }
}
