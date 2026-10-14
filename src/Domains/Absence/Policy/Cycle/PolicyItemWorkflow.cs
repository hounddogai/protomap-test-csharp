namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy items through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyItemManager(IPolicyItemStore Store, PolicyItemValidator Validator, IClock Clock, List<string> Journal, PolicyItemEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy item {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyItemStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy item {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyItemStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy item {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of policy item workflow.</summary>
public static class PolicyItemWorkflow
{
    public static PolicyItemManager NewPolicyItemManager(IPolicyItemStore store, IClock clock)
    {
        return new PolicyItemManager(
            store,
            PolicyItemRules.DefaultPolicyItemValidator(),
            clock,
            new List<string>(),
            PolicyItemEngine.DefaultPolicyItemEngine());
    }
}
