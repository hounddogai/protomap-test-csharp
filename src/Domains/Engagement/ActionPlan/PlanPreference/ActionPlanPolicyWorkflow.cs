namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanPolicyManager(IActionPlanPolicyStore Store, ActionPlanPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanPolicyStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanPolicyStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of action plan policy workflow.</summary>
public static class ActionPlanPolicyWorkflow
{
    public static ActionPlanPolicyManager NewActionPlanPolicyManager(IActionPlanPolicyStore store, IClock clock)
    {
        return new ActionPlanPolicyManager(
            store,
            ActionPlanPolicyRules.DefaultActionPlanPolicyValidator(),
            clock,
            new List<string>());
    }
}
