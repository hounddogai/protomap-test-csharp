namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves policy mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyMappingManager(IPolicyMappingStore Store, PolicyMappingValidator Validator, IClock Clock, List<string> Journal, PolicyMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyMappingStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyMappingStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy mapping {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of policy mapping workflow.</summary>
public static class PolicyMappingWorkflow
{
    public static PolicyMappingManager NewPolicyMappingManager(IPolicyMappingStore store, IClock clock, Registry flags)
    {
        return new PolicyMappingManager(
            store,
            PolicyMappingRules.DefaultPolicyMappingValidator(),
            clock,
            new List<string>(),
            PolicyMappingEngine.DefaultPolicyMappingEngine(flags));
    }
}
