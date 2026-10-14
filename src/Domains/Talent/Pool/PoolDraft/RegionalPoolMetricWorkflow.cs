namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional pool metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalPoolMetricWorkflow(IRegionalPoolMetricStore Store, RegionalPoolMetricValidator Validator, IClock Clock, List<string> Journal)
{
    public static RegionalPoolMetricWorkflow NewRegionalPoolMetricWorkflow(IRegionalPoolMetricStore store, IClock clock)
    {
        return new RegionalPoolMetricWorkflow(
            store,
            RegionalPoolMetricRules.DefaultRegionalPoolMetricValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pool metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalPoolMetricStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional pool metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalPoolMetricStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.StartsAt > cutoff)
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
