namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive pool forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptivePoolForecastDesk(IAdaptivePoolForecastStore Store, AdaptivePoolForecastValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive pool forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptivePoolForecastStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive pool forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptivePoolForecastStatus.Stopped);
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

/// <summary>Functions of adaptive pool forecast workflow.</summary>
public static class AdaptivePoolForecastWorkflow
{
    public static AdaptivePoolForecastDesk NewAdaptivePoolForecastDesk(IAdaptivePoolForecastStore store, IClock clock)
    {
        return new AdaptivePoolForecastDesk(
            store,
            AdaptivePoolForecastRules.DefaultAdaptivePoolForecastValidator(),
            clock,
            new List<string>());
    }
}
