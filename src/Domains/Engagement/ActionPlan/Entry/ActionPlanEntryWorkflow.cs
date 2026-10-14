namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanEntryDesk(IActionPlanEntryStore Store, ActionPlanEntryValidator Validator, IClock Clock, List<string> Journal, ActionPlanEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanEntryStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanEntryStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan entry {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of action plan entry workflow.</summary>
public static class ActionPlanEntryWorkflow
{
    public static ActionPlanEntryDesk NewActionPlanEntryDesk(IActionPlanEntryStore store, IClock clock)
    {
        return new ActionPlanEntryDesk(
            store,
            ActionPlanEntryRules.DefaultActionPlanEntryValidator(),
            clock,
            new List<string>(),
            ActionPlanEntryEngine.DefaultActionPlanEntryEngine());
    }
}
