namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceForecastDesk(IBalanceForecastStore Store, BalanceForecastValidator Validator, IClock Clock, List<string> Journal, BalanceForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceForecastStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceForecastStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance forecast {code} not found");
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

/// <summary>Functions of balance forecast workflow.</summary>
public static class BalanceForecastWorkflow
{
    public static BalanceForecastDesk NewBalanceForecastDesk(IBalanceForecastStore store, IClock clock)
    {
        return new BalanceForecastDesk(
            store,
            BalanceForecastRules.DefaultBalanceForecastValidator(),
            clock,
            new List<string>(),
            BalanceForecastEngine.DefaultBalanceForecastEngine());
    }
}
