namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy ack mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAckMappingCoordinator(IPolicyAckMappingStore Store, PolicyAckMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAckMappingStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAckMappingStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of policy ack mapping workflow.</summary>
public static class PolicyAckMappingWorkflow
{
    public static PolicyAckMappingCoordinator NewPolicyAckMappingCoordinator(IPolicyAckMappingStore store, IClock clock)
    {
        return new PolicyAckMappingCoordinator(
            store,
            PolicyAckMappingRules.DefaultPolicyAckMappingValidator(),
            clock,
            new List<string>());
    }
}
