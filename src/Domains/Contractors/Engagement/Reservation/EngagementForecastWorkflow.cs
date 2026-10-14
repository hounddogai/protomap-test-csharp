namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves engagement forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementForecastCoordinator(IEngagementForecastStore Store, EngagementForecastValidator Validator, IClock Clock, List<string> Journal, EngagementForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementForecastStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementForecastStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement forecast {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of engagement forecast workflow.</summary>
public static class EngagementForecastWorkflow
{
    public static EngagementForecastCoordinator NewEngagementForecastCoordinator(IEngagementForecastStore store, IClock clock)
    {
        return new EngagementForecastCoordinator(
            store,
            EngagementForecastRules.DefaultEngagementForecastValidator(),
            clock,
            new List<string>(),
            EngagementForecastEngine.DefaultEngagementForecastEngine());
    }
}
