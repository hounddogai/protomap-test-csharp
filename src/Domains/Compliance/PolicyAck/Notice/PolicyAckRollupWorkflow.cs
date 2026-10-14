namespace Acme.Workforce.Compliance.PolicyAck.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy ack rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAckRollupWorkflow(IPolicyAckRollupStore Store, PolicyAckRollupValidator Validator, IClock Clock, List<string> Journal)
{
    public static PolicyAckRollupWorkflow NewPolicyAckRollupWorkflow(IPolicyAckRollupStore store, IClock clock)
    {
        return new PolicyAckRollupWorkflow(
            store,
            PolicyAckRollupRules.DefaultPolicyAckRollupValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAckRollupStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAckRollupStatus.Cancelled);
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
