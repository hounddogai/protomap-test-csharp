namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending license seat forecasts through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingLicenseSeatForecastCoordinator(IPendingLicenseSeatForecastStore Store, PendingLicenseSeatForecastValidator Validator, IClock Clock, List<string> Journal, PendingLicenseSeatForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending license seat forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingLicenseSeatForecastStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending license seat forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingLicenseSeatForecastStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending license seat forecast {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of pending license seat forecast workflow.</summary>
public static class PendingLicenseSeatForecastWorkflow
{
    public static PendingLicenseSeatForecastCoordinator NewPendingLicenseSeatForecastCoordinator(IPendingLicenseSeatForecastStore store, IClock clock)
    {
        return new PendingLicenseSeatForecastCoordinator(
            store,
            PendingLicenseSeatForecastRules.DefaultPendingLicenseSeatForecastValidator(),
            clock,
            new List<string>(),
            PendingLicenseSeatForecastEngine.DefaultPendingLicenseSeatForecastEngine());
    }
}
