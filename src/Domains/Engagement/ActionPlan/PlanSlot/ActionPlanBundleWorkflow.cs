namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves action plan bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanBundleDesk(IActionPlanBundleStore Store, ActionPlanBundleValidator Validator, IClock Clock, List<string> Journal, ActionPlanBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanBundleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanBundleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan bundle {code} not found");
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

/// <summary>Functions of action plan bundle workflow.</summary>
public static class ActionPlanBundleWorkflow
{
    public static ActionPlanBundleDesk NewActionPlanBundleDesk(IActionPlanBundleStore store, IClock clock, Registry flags)
    {
        return new ActionPlanBundleDesk(
            store,
            ActionPlanBundleRules.DefaultActionPlanBundleValidator(),
            clock,
            new List<string>(),
            ActionPlanBundleEngine.DefaultActionPlanBundleEngine(flags));
    }
}
