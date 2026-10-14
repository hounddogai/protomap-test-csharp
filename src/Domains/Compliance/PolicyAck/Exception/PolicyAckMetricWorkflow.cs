namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves policy ack metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record PolicyAckMetricCoordinator(IPolicyAckMetricStore Store, PolicyAckMetricValidator Validator, IClock Clock, List<string> Journal, PolicyAckMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PolicyAckMetricStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PolicyAckMetricStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"policy ack metric {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of policy ack metric workflow.</summary>
public static class PolicyAckMetricWorkflow
{
    public static PolicyAckMetricCoordinator NewPolicyAckMetricCoordinator(IPolicyAckMetricStore store, IClock clock, Registry flags)
    {
        return new PolicyAckMetricCoordinator(
            store,
            PolicyAckMetricRules.DefaultPolicyAckMetricValidator(),
            clock,
            new List<string>(),
            PolicyAckMetricEngine.DefaultPolicyAckMetricEngine(flags));
    }
}
