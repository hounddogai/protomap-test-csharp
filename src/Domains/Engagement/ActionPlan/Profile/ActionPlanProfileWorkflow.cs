namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanProfileManager(IActionPlanProfileStore Store, ActionPlanProfileValidator Validator, IClock Clock, List<string> Journal, ActionPlanProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanProfileStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanProfileStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan profile {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of action plan profile workflow.</summary>
public static class ActionPlanProfileWorkflow
{
    public static ActionPlanProfileManager NewActionPlanProfileManager(IActionPlanProfileStore store, IClock clock)
    {
        return new ActionPlanProfileManager(
            store,
            ActionPlanProfileRules.DefaultActionPlanProfileValidator(),
            clock,
            new List<string>(),
            ActionPlanProfileEngine.DefaultActionPlanProfileEngine());
    }
}
