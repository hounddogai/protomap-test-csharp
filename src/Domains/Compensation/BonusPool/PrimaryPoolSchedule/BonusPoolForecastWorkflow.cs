namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves bonus pool forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolForecastDesk(IBonusPoolForecastStore Store, BonusPoolForecastValidator Validator, IClock Clock, List<string> Journal, BonusPoolForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolForecastStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolForecastStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool forecast {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of bonus pool forecast workflow.</summary>
public static class BonusPoolForecastWorkflow
{
    public static BonusPoolForecastDesk NewBonusPoolForecastDesk(IBonusPoolForecastStore store, IClock clock)
    {
        return new BonusPoolForecastDesk(
            store,
            BonusPoolForecastRules.DefaultBonusPoolForecastValidator(),
            clock,
            new List<string>(),
            BonusPoolForecastEngine.DefaultBonusPoolForecastEngine());
    }
}
