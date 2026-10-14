namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves policy ack preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAckPreferenceDesk(IPolicyAckPreferenceStore Store, PolicyAckPreferenceValidator Validator, IClock Clock, List<string> Journal, PolicyAckPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAckPreferenceStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAckPreferenceStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack preference {code} not found");
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

/// <summary>Functions of policy ack preference workflow.</summary>
public static class PolicyAckPreferenceWorkflow
{
    public static PolicyAckPreferenceDesk NewPolicyAckPreferenceDesk(IPolicyAckPreferenceStore store, IClock clock)
    {
        return new PolicyAckPreferenceDesk(
            store,
            PolicyAckPreferenceRules.DefaultPolicyAckPreferenceValidator(),
            clock,
            new List<string>(),
            PolicyAckPreferenceEngine.DefaultPolicyAckPreferenceEngine());
    }
}
