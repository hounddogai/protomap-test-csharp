namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending action plan baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingActionPlanBaselineCoordinator(IPendingActionPlanBaselineStore Store, PendingActionPlanBaselineValidator Validator, IClock Clock, List<string> Journal, PendingActionPlanBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending action plan baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingActionPlanBaselineStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending action plan baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingActionPlanBaselineStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending action plan baseline {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of pending action plan baseline workflow.</summary>
public static class PendingActionPlanBaselineWorkflow
{
    public static PendingActionPlanBaselineCoordinator NewPendingActionPlanBaselineCoordinator(IPendingActionPlanBaselineStore store, IClock clock, Registry flags)
    {
        return new PendingActionPlanBaselineCoordinator(
            store,
            PendingActionPlanBaselineRules.DefaultPendingActionPlanBaselineValidator(),
            clock,
            new List<string>(),
            PendingActionPlanBaselineEngine.DefaultPendingActionPlanBaselineEngine(flags));
    }
}
