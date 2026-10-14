namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves career path forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record CareerPathForecastDesk(ICareerPathForecastStore Store, CareerPathForecastValidator Validator, IClock Clock, List<string> Journal, CareerPathForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CareerPathForecastStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CareerPathForecastStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"career path forecast {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of career path forecast workflow.</summary>
public static class CareerPathForecastWorkflow
{
    public static CareerPathForecastDesk NewCareerPathForecastDesk(ICareerPathForecastStore store, IClock clock)
    {
        return new CareerPathForecastDesk(
            store,
            CareerPathForecastRules.DefaultCareerPathForecastValidator(),
            clock,
            new List<string>(),
            CareerPathForecastEngine.DefaultCareerPathForecastEngine());
    }
}
