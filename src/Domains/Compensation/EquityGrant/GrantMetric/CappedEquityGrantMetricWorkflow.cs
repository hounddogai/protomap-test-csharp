namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped equity grant metrics through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedEquityGrantMetricDesk(ICappedEquityGrantMetricStore Store, CappedEquityGrantMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped equity grant metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedEquityGrantMetricStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped equity grant metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedEquityGrantMetricStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of capped equity grant metric workflow.</summary>
public static class CappedEquityGrantMetricWorkflow
{
    public static CappedEquityGrantMetricDesk NewCappedEquityGrantMetricDesk(ICappedEquityGrantMetricStore store, IClock clock)
    {
        return new CappedEquityGrantMetricDesk(
            store,
            CappedEquityGrantMetricRules.DefaultCappedEquityGrantMetricValidator(),
            clock,
            new List<string>());
    }
}
