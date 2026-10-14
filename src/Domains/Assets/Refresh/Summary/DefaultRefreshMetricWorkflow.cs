namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default refresh metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultRefreshMetricManager(IDefaultRefreshMetricStore Store, DefaultRefreshMetricValidator Validator, IClock Clock, List<string> Journal, DefaultRefreshMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default refresh metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultRefreshMetricStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default refresh metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultRefreshMetricStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default refresh metric {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of default refresh metric workflow.</summary>
public static class DefaultRefreshMetricWorkflow
{
    public static DefaultRefreshMetricManager NewDefaultRefreshMetricManager(IDefaultRefreshMetricStore store, IClock clock, Registry flags)
    {
        return new DefaultRefreshMetricManager(
            store,
            DefaultRefreshMetricRules.DefaultDefaultRefreshMetricValidator(),
            clock,
            new List<string>(),
            DefaultRefreshMetricEngine.DefaultDefaultRefreshMetricEngine(flags));
    }
}
