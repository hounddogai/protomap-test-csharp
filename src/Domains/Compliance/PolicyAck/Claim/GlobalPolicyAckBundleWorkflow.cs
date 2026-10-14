namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global policy ack bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalPolicyAckBundleCoordinator(IGlobalPolicyAckBundleStore Store, GlobalPolicyAckBundleValidator Validator, IClock Clock, List<string> Journal, GlobalPolicyAckBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global policy ack bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalPolicyAckBundleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global policy ack bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalPolicyAckBundleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global policy ack bundle {code} not found");
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

/// <summary>Functions of global policy ack bundle workflow.</summary>
public static class GlobalPolicyAckBundleWorkflow
{
    public static GlobalPolicyAckBundleCoordinator NewGlobalPolicyAckBundleCoordinator(IGlobalPolicyAckBundleStore store, IClock clock)
    {
        return new GlobalPolicyAckBundleCoordinator(
            store,
            GlobalPolicyAckBundleRules.DefaultGlobalPolicyAckBundleValidator(),
            clock,
            new List<string>(),
            GlobalPolicyAckBundleEngine.DefaultGlobalPolicyAckBundleEngine());
    }
}
