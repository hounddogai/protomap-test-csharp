namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves laptop forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record LaptopForecastService(ILaptopForecastStore Store, LaptopForecastValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LaptopForecastStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LaptopForecastStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of laptop forecast workflow.</summary>
public static class LaptopForecastWorkflow
{
    public static LaptopForecastService NewLaptopForecastService(ILaptopForecastStore store, IClock clock)
    {
        return new LaptopForecastService(
            store,
            LaptopForecastRules.DefaultLaptopForecastValidator(),
            clock,
            new List<string>());
    }
}
