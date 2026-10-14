namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanMetricService(IActionPlanMetricStore Store, ActionPlanMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanMetricStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanMetricStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of action plan metric workflow.</summary>
public static class ActionPlanMetricWorkflow
{
    public static ActionPlanMetricService NewActionPlanMetricService(IActionPlanMetricStore store, IClock clock)
    {
        return new ActionPlanMetricService(
            store,
            ActionPlanMetricRules.DefaultActionPlanMetricValidator(),
            clock,
            new List<string>());
    }
}
