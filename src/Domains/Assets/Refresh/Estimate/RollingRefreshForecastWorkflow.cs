namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling refresh forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingRefreshForecastWorkflow(IRollingRefreshForecastStore Store, RollingRefreshForecastValidator Validator, IClock Clock, List<string> Journal, RollingRefreshForecastEngine Engine)
{
    public static RollingRefreshForecastWorkflow NewRollingRefreshForecastWorkflow(IRollingRefreshForecastStore store, IClock clock, Registry flags)
    {
        return new RollingRefreshForecastWorkflow(
            store,
            RollingRefreshForecastRules.DefaultRollingRefreshForecastValidator(),
            clock,
            new List<string>(),
            RollingRefreshForecastEngine.DefaultRollingRefreshForecastEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling refresh forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingRefreshForecastStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling refresh forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingRefreshForecastStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling refresh forecast {code} not found");
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
