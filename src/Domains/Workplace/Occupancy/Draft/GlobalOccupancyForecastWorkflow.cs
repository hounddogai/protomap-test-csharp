namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global occupancy forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalOccupancyForecastWorkflow(IGlobalOccupancyForecastStore Store, GlobalOccupancyForecastValidator Validator, IClock Clock, List<string> Journal)
{
    public static GlobalOccupancyForecastWorkflow NewGlobalOccupancyForecastWorkflow(IGlobalOccupancyForecastStore store, IClock clock)
    {
        return new GlobalOccupancyForecastWorkflow(
            store,
            GlobalOccupancyForecastRules.DefaultGlobalOccupancyForecastValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global occupancy forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalOccupancyForecastStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global occupancy forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalOccupancyForecastStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.UpdatedAt > cutoff)
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
