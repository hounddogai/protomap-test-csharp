namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict equity grant metrics through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictEquityGrantMetricManager(IStrictEquityGrantMetricStore Store, StrictEquityGrantMetricValidator Validator, IClock Clock, List<string> Journal, StrictEquityGrantMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict equity grant metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictEquityGrantMetricStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict equity grant metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictEquityGrantMetricStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict equity grant metric {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of strict equity grant metric workflow.</summary>
public static class StrictEquityGrantMetricWorkflow
{
    public static StrictEquityGrantMetricManager NewStrictEquityGrantMetricManager(IStrictEquityGrantMetricStore store, IClock clock)
    {
        return new StrictEquityGrantMetricManager(
            store,
            StrictEquityGrantMetricRules.DefaultStrictEquityGrantMetricValidator(),
            clock,
            new List<string>(),
            StrictEquityGrantMetricEngine.DefaultStrictEquityGrantMetricEngine());
    }
}
