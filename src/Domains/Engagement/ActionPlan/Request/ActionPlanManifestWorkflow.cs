namespace Acme.Workforce.Engagement.ActionPlan.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanManifestManager(IActionPlanManifestStore Store, ActionPlanManifestValidator Validator, IClock Clock, List<string> Journal, ActionPlanManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanManifestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanManifestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan manifest {code} not found");
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

/// <summary>Functions of action plan manifest workflow.</summary>
public static class ActionPlanManifestWorkflow
{
    public static ActionPlanManifestManager NewActionPlanManifestManager(IActionPlanManifestStore store, IClock clock)
    {
        return new ActionPlanManifestManager(
            store,
            ActionPlanManifestRules.DefaultActionPlanManifestValidator(),
            clock,
            new List<string>(),
            ActionPlanManifestEngine.DefaultActionPlanManifestEngine());
    }
}
