namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict accrual forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictAccrualForecastService(IStrictAccrualForecastStore Store, StrictAccrualForecastValidator Validator, IClock Clock, List<string> Journal, StrictAccrualForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict accrual forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAccrualForecastStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict accrual forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAccrualForecastStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict accrual forecast {code} not found");
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

/// <summary>Functions of strict accrual forecast workflow.</summary>
public static class StrictAccrualForecastWorkflow
{
    public static StrictAccrualForecastService NewStrictAccrualForecastService(IStrictAccrualForecastStore store, IClock clock)
    {
        return new StrictAccrualForecastService(
            store,
            StrictAccrualForecastRules.DefaultStrictAccrualForecastValidator(),
            clock,
            new List<string>(),
            StrictAccrualForecastEngine.DefaultStrictAccrualForecastEngine());
    }
}
