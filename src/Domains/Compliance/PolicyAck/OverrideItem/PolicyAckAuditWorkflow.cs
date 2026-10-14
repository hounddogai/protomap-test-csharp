namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy ack audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAckAuditManager(IPolicyAckAuditStore Store, PolicyAckAuditValidator Validator, IClock Clock, List<string> Journal, PolicyAckAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAckAuditStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAckAuditStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack audit {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of policy ack audit workflow.</summary>
public static class PolicyAckAuditWorkflow
{
    public static PolicyAckAuditManager NewPolicyAckAuditManager(IPolicyAckAuditStore store, IClock clock)
    {
        return new PolicyAckAuditManager(
            store,
            PolicyAckAuditRules.DefaultPolicyAckAuditValidator(),
            clock,
            new List<string>(),
            PolicyAckAuditEngine.DefaultPolicyAckAuditEngine());
    }
}
