namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled pay band forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledPayBandForecastWorkflow(IScheduledPayBandForecastStore Store, ScheduledPayBandForecastValidator Validator, IClock Clock, List<string> Journal, ScheduledPayBandForecastEngine Engine)
{
    public static ScheduledPayBandForecastWorkflow NewScheduledPayBandForecastWorkflow(IScheduledPayBandForecastStore store, IClock clock)
    {
        return new ScheduledPayBandForecastWorkflow(
            store,
            ScheduledPayBandForecastRules.DefaultScheduledPayBandForecastValidator(),
            clock,
            new List<string>(),
            ScheduledPayBandForecastEngine.DefaultScheduledPayBandForecastEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay band forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPayBandForecastStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay band forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPayBandForecastStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay band forecast {code} not found");
        }
        var result = Engine.Compute(item);
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
