namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanRequestManager(IActionPlanRequestStore Store, ActionPlanRequestValidator Validator, IClock Clock, List<string> Journal, ActionPlanRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanRequestStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanRequestStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan request {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of action plan request workflow.</summary>
public static class ActionPlanRequestWorkflow
{
    public static ActionPlanRequestManager NewActionPlanRequestManager(IActionPlanRequestStore store, IClock clock)
    {
        return new ActionPlanRequestManager(
            store,
            ActionPlanRequestRules.DefaultActionPlanRequestValidator(),
            clock,
            new List<string>(),
            ActionPlanRequestEngine.DefaultActionPlanRequestEngine());
    }
}
