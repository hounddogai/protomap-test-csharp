namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves engagement metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementMetricCoordinator(IEngagementMetricStore Store, EngagementMetricValidator Validator, IClock Clock, List<string> Journal, EngagementMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementMetricStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementMetricStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement metric {code} not found");
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

/// <summary>Functions of engagement metric workflow.</summary>
public static class EngagementMetricWorkflow
{
    public static EngagementMetricCoordinator NewEngagementMetricCoordinator(IEngagementMetricStore store, IClock clock, Registry flags)
    {
        return new EngagementMetricCoordinator(
            store,
            EngagementMetricRules.DefaultEngagementMetricValidator(),
            clock,
            new List<string>(),
            EngagementMetricEngine.DefaultEngagementMetricEngine(flags));
    }
}
