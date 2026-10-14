namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default action plan preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultActionPlanPreferenceService(IDefaultActionPlanPreferenceStore Store, DefaultActionPlanPreferenceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default action plan preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultActionPlanPreferenceStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default action plan preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultActionPlanPreferenceStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of default action plan preference workflow.</summary>
public static class DefaultActionPlanPreferenceWorkflow
{
    public static DefaultActionPlanPreferenceService NewDefaultActionPlanPreferenceService(IDefaultActionPlanPreferenceStore store, IClock clock)
    {
        return new DefaultActionPlanPreferenceService(
            store,
            DefaultActionPlanPreferenceRules.DefaultDefaultActionPlanPreferenceValidator(),
            clock,
            new List<string>());
    }
}
