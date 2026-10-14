namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling mobility forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingMobilityForecastManager(IRollingMobilityForecastStore Store, RollingMobilityForecastValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling mobility forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingMobilityForecastStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling mobility forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingMobilityForecastStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of rolling mobility forecast workflow.</summary>
public static class RollingMobilityForecastWorkflow
{
    public static RollingMobilityForecastManager NewRollingMobilityForecastManager(IRollingMobilityForecastStore store, IClock clock)
    {
        return new RollingMobilityForecastManager(
            store,
            RollingMobilityForecastRules.DefaultRollingMobilityForecastValidator(),
            clock,
            new List<string>());
    }
}
