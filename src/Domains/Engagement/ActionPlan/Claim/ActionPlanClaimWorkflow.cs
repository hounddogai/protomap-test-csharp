namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanClaimWorkflow(IActionPlanClaimStore Store, ActionPlanClaimValidator Validator, IClock Clock, List<string> Journal)
{
    public static ActionPlanClaimWorkflow NewActionPlanClaimWorkflow(IActionPlanClaimStore store, IClock clock)
    {
        return new ActionPlanClaimWorkflow(
            store,
            ActionPlanClaimRules.DefaultActionPlanClaimValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanClaimStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanClaimStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
