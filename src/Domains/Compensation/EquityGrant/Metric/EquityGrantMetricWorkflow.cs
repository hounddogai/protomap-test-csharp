namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves equity grant metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantMetricDesk(IEquityGrantMetricStore Store, EquityGrantMetricValidator Validator, IClock Clock, List<string> Journal, EquityGrantMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantMetricStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantMetricStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant metric {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of equity grant metric workflow.</summary>
public static class EquityGrantMetricWorkflow
{
    public static EquityGrantMetricDesk NewEquityGrantMetricDesk(IEquityGrantMetricStore store, IClock clock)
    {
        return new EquityGrantMetricDesk(
            store,
            EquityGrantMetricRules.DefaultEquityGrantMetricValidator(),
            clock,
            new List<string>(),
            EquityGrantMetricEngine.DefaultEquityGrantMetricEngine());
    }
}
