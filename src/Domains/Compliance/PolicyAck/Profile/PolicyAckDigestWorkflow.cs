namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy ack digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAckDigestCoordinator(IPolicyAckDigestStore Store, PolicyAckDigestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAckDigestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAckDigestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of policy ack digest workflow.</summary>
public static class PolicyAckDigestWorkflow
{
    public static PolicyAckDigestCoordinator NewPolicyAckDigestCoordinator(IPolicyAckDigestStore store, IClock clock)
    {
        return new PolicyAckDigestCoordinator(
            store,
            PolicyAckDigestRules.DefaultPolicyAckDigestValidator(),
            clock,
            new List<string>());
    }
}
