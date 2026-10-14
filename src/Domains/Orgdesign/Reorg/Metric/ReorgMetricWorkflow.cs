namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves reorg metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgMetricDesk(IReorgMetricStore Store, ReorgMetricValidator Validator, IClock Clock, List<string> Journal, ReorgMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgMetricStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgMetricStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg metric {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of reorg metric workflow.</summary>
public static class ReorgMetricWorkflow
{
    public static ReorgMetricDesk NewReorgMetricDesk(IReorgMetricStore store, IClock clock, Registry flags)
    {
        return new ReorgMetricDesk(
            store,
            ReorgMetricRules.DefaultReorgMetricValidator(),
            clock,
            new List<string>(),
            ReorgMetricEngine.DefaultReorgMetricEngine(flags));
    }
}
