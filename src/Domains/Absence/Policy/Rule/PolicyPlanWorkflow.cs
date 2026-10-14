namespace Acme.Workforce.Absence.Policy.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyPlanManager(IPolicyPlanStore Store, PolicyPlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyPlanStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyPlanStatus.Failed);
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

/// <summary>Functions of policy plan workflow.</summary>
public static class PolicyPlanWorkflow
{
    public static PolicyPlanManager NewPolicyPlanManager(IPolicyPlanStore store, IClock clock)
    {
        return new PolicyPlanManager(store, PolicyPlanRules.DefaultPolicyPlanValidator(), clock, new List<string>());
    }
}
