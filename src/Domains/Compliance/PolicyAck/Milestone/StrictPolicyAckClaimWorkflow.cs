namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict policy ack claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictPolicyAckClaimManager(IStrictPolicyAckClaimStore Store, StrictPolicyAckClaimValidator Validator, IClock Clock, List<string> Journal, StrictPolicyAckClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict policy ack claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictPolicyAckClaimStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict policy ack claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictPolicyAckClaimStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict policy ack claim {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of strict policy ack claim workflow.</summary>
public static class StrictPolicyAckClaimWorkflow
{
    public static StrictPolicyAckClaimManager NewStrictPolicyAckClaimManager(IStrictPolicyAckClaimStore store, IClock clock)
    {
        return new StrictPolicyAckClaimManager(
            store,
            StrictPolicyAckClaimRules.DefaultStrictPolicyAckClaimValidator(),
            clock,
            new List<string>(),
            StrictPolicyAckClaimEngine.DefaultStrictPolicyAckClaimEngine());
    }
}
