namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered career path forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredCareerPathForecastManager(ITieredCareerPathForecastStore Store, TieredCareerPathForecastValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered career path forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredCareerPathForecastStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered career path forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredCareerPathForecastStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of tiered career path forecast workflow.</summary>
public static class TieredCareerPathForecastWorkflow
{
    public static TieredCareerPathForecastManager NewTieredCareerPathForecastManager(ITieredCareerPathForecastStore store, IClock clock)
    {
        return new TieredCareerPathForecastManager(
            store,
            TieredCareerPathForecastRules.DefaultTieredCareerPathForecastValidator(),
            clock,
            new List<string>());
    }
}
