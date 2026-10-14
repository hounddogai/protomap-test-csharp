namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual action plan preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualActionPlanPreferenceDesk(IManualActionPlanPreferenceStore Store, ManualActionPlanPreferenceValidator Validator, IClock Clock, List<string> Journal, ManualActionPlanPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual action plan preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualActionPlanPreferenceStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual action plan preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualActionPlanPreferenceStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual action plan preference {code} not found");
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

/// <summary>Functions of manual action plan preference workflow.</summary>
public static class ManualActionPlanPreferenceWorkflow
{
    public static ManualActionPlanPreferenceDesk NewManualActionPlanPreferenceDesk(IManualActionPlanPreferenceStore store, IClock clock, Registry flags)
    {
        return new ManualActionPlanPreferenceDesk(
            store,
            ManualActionPlanPreferenceRules.DefaultManualActionPlanPreferenceValidator(),
            clock,
            new List<string>(),
            ManualActionPlanPreferenceEngine.DefaultManualActionPlanPreferenceEngine(flags));
    }
}
