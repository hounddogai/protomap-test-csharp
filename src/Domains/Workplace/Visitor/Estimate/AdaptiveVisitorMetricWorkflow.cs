namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive visitor metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveVisitorMetricManager(IAdaptiveVisitorMetricStore Store, AdaptiveVisitorMetricValidator Validator, IClock Clock, List<string> Journal, AdaptiveVisitorMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive visitor metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveVisitorMetricStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive visitor metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveVisitorMetricStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive visitor metric {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of adaptive visitor metric workflow.</summary>
public static class AdaptiveVisitorMetricWorkflow
{
    public static AdaptiveVisitorMetricManager NewAdaptiveVisitorMetricManager(IAdaptiveVisitorMetricStore store, IClock clock)
    {
        return new AdaptiveVisitorMetricManager(
            store,
            AdaptiveVisitorMetricRules.DefaultAdaptiveVisitorMetricValidator(),
            clock,
            new List<string>(),
            AdaptiveVisitorMetricEngine.DefaultAdaptiveVisitorMetricEngine());
    }
}
