namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves offboarding forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record OffboardingForecastDesk(IOffboardingForecastStore Store, OffboardingForecastValidator Validator, IClock Clock, List<string> Journal, OffboardingForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OffboardingForecastStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OffboardingForecastStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"offboarding forecast {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of offboarding forecast workflow.</summary>
public static class OffboardingForecastWorkflow
{
    public static OffboardingForecastDesk NewOffboardingForecastDesk(IOffboardingForecastStore store, IClock clock, Registry flags)
    {
        return new OffboardingForecastDesk(
            store,
            OffboardingForecastRules.DefaultOffboardingForecastValidator(),
            clock,
            new List<string>(),
            OffboardingForecastEngine.DefaultOffboardingForecastEngine(flags));
    }
}
