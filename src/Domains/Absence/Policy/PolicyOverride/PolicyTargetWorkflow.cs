namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyTargetCoordinator(IPolicyTargetStore Store, PolicyTargetValidator Validator, IClock Clock, List<string> Journal, PolicyTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyTargetStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyTargetStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy target {code} not found");
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

/// <summary>Functions of policy target workflow.</summary>
public static class PolicyTargetWorkflow
{
    public static PolicyTargetCoordinator NewPolicyTargetCoordinator(IPolicyTargetStore store, IClock clock)
    {
        return new PolicyTargetCoordinator(
            store,
            PolicyTargetRules.DefaultPolicyTargetValidator(),
            clock,
            new List<string>(),
            PolicyTargetEngine.DefaultPolicyTargetEngine());
    }
}
