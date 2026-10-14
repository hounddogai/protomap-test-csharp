namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy ack adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAckAdjustmentService(IPolicyAckAdjustmentStore Store, PolicyAckAdjustmentValidator Validator, IClock Clock, List<string> Journal, PolicyAckAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAckAdjustmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAckAdjustmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack adjustment {code} not found");
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

/// <summary>Functions of policy ack adjustment workflow.</summary>
public static class PolicyAckAdjustmentWorkflow
{
    public static PolicyAckAdjustmentService NewPolicyAckAdjustmentService(IPolicyAckAdjustmentStore store, IClock clock)
    {
        return new PolicyAckAdjustmentService(
            store,
            PolicyAckAdjustmentRules.DefaultPolicyAckAdjustmentValidator(),
            clock,
            new List<string>(),
            PolicyAckAdjustmentEngine.DefaultPolicyAckAdjustmentEngine());
    }
}
