namespace Acme.Workforce.Payrollops.RetroPay.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped retro pay forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedRetroPayForecastService(ICappedRetroPayForecastStore Store, CappedRetroPayForecastValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped retro pay forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedRetroPayForecastStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped retro pay forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedRetroPayForecastStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of capped retro pay forecast workflow.</summary>
public static class CappedRetroPayForecastWorkflow
{
    public static CappedRetroPayForecastService NewCappedRetroPayForecastService(ICappedRetroPayForecastStore store, IClock clock)
    {
        return new CappedRetroPayForecastService(
            store,
            CappedRetroPayForecastRules.DefaultCappedRetroPayForecastValidator(),
            clock,
            new List<string>());
    }
}
