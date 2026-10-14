namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy ack scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAckScoreCoordinator(IPolicyAckScoreStore Store, PolicyAckScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAckScoreStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAckScoreStatus.Failed);
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

/// <summary>Functions of policy ack score workflow.</summary>
public static class PolicyAckScoreWorkflow
{
    public static PolicyAckScoreCoordinator NewPolicyAckScoreCoordinator(IPolicyAckScoreStore store, IClock clock)
    {
        return new PolicyAckScoreCoordinator(
            store,
            PolicyAckScoreRules.DefaultPolicyAckScoreValidator(),
            clock,
            new List<string>());
    }
}
