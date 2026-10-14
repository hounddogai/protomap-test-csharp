namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanAllocationWorkflow(IActionPlanAllocationStore Store, ActionPlanAllocationValidator Validator, IClock Clock, List<string> Journal, ActionPlanAllocationEngine Engine)
{
    public static ActionPlanAllocationWorkflow NewActionPlanAllocationWorkflow(IActionPlanAllocationStore store, IClock clock)
    {
        return new ActionPlanAllocationWorkflow(
            store,
            ActionPlanAllocationRules.DefaultActionPlanAllocationValidator(),
            clock,
            new List<string>(),
            ActionPlanAllocationEngine.DefaultActionPlanAllocationEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanAllocationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanAllocationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan allocation {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
