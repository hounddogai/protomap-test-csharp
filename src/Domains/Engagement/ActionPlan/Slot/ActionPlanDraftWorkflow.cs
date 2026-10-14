namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanDraftDesk(IActionPlanDraftStore Store, ActionPlanDraftValidator Validator, IClock Clock, List<string> Journal, ActionPlanDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanDraftStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanDraftStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan draft {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of action plan draft workflow.</summary>
public static class ActionPlanDraftWorkflow
{
    public static ActionPlanDraftDesk NewActionPlanDraftDesk(IActionPlanDraftStore store, IClock clock)
    {
        return new ActionPlanDraftDesk(
            store,
            ActionPlanDraftRules.DefaultActionPlanDraftValidator(),
            clock,
            new List<string>(),
            ActionPlanDraftEngine.DefaultActionPlanDraftEngine());
    }
}
