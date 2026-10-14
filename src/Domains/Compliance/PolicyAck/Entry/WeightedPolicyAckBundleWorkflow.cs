namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted policy ack bundles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedPolicyAckBundleCoordinator(IWeightedPolicyAckBundleStore Store, WeightedPolicyAckBundleValidator Validator, IClock Clock, List<string> Journal, WeightedPolicyAckBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted policy ack bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedPolicyAckBundleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted policy ack bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedPolicyAckBundleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted policy ack bundle {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of weighted policy ack bundle workflow.</summary>
public static class WeightedPolicyAckBundleWorkflow
{
    public static WeightedPolicyAckBundleCoordinator NewWeightedPolicyAckBundleCoordinator(IWeightedPolicyAckBundleStore store, IClock clock)
    {
        return new WeightedPolicyAckBundleCoordinator(
            store,
            WeightedPolicyAckBundleRules.DefaultWeightedPolicyAckBundleValidator(),
            clock,
            new List<string>(),
            WeightedPolicyAckBundleEngine.DefaultWeightedPolicyAckBundleEngine());
    }
}
