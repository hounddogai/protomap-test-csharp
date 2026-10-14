namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy ack baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAckBaselineDesk(IPolicyAckBaselineStore Store, PolicyAckBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAckBaselineStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAckBaselineStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of policy ack baseline workflow.</summary>
public static class PolicyAckBaselineWorkflow
{
    public static PolicyAckBaselineDesk NewPolicyAckBaselineDesk(IPolicyAckBaselineStore store, IClock clock)
    {
        return new PolicyAckBaselineDesk(
            store,
            PolicyAckBaselineRules.DefaultPolicyAckBaselineValidator(),
            clock,
            new List<string>());
    }
}
