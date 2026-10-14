namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayForecastDesk(IRetroPayForecastStore Store, RetroPayForecastValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayForecastStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayForecastStatus.Retired);
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

/// <summary>Functions of retro pay forecast workflow.</summary>
public static class RetroPayForecastWorkflow
{
    public static RetroPayForecastDesk NewRetroPayForecastDesk(IRetroPayForecastStore store, IClock clock)
    {
        return new RetroPayForecastDesk(
            store,
            RetroPayForecastRules.DefaultRetroPayForecastValidator(),
            clock,
            new List<string>());
    }
}
