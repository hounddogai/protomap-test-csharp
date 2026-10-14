namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves policy ack requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAckRequestDesk(IPolicyAckRequestStore Store, PolicyAckRequestValidator Validator, IClock Clock, List<string> Journal, PolicyAckRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAckRequestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAckRequestStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack request {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of policy ack request workflow.</summary>
public static class PolicyAckRequestWorkflow
{
    public static PolicyAckRequestDesk NewPolicyAckRequestDesk(IPolicyAckRequestStore store, IClock clock, Registry flags)
    {
        return new PolicyAckRequestDesk(
            store,
            PolicyAckRequestRules.DefaultPolicyAckRequestValidator(),
            clock,
            new List<string>(),
            PolicyAckRequestEngine.DefaultPolicyAckRequestEngine(flags));
    }
}
