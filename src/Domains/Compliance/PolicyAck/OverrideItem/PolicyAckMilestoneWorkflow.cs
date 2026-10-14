namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy ack milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAckMilestoneManager(IPolicyAckMilestoneStore Store, PolicyAckMilestoneValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAckMilestoneStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAckMilestoneStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of policy ack milestone workflow.</summary>
public static class PolicyAckMilestoneWorkflow
{
    public static PolicyAckMilestoneManager NewPolicyAckMilestoneManager(IPolicyAckMilestoneStore store, IClock clock)
    {
        return new PolicyAckMilestoneManager(
            store,
            PolicyAckMilestoneRules.DefaultPolicyAckMilestoneValidator(),
            clock,
            new List<string>());
    }
}
