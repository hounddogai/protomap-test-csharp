namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered policy ack variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredPolicyAckVarianceManager(ITieredPolicyAckVarianceStore Store, TieredPolicyAckVarianceValidator Validator, IClock Clock, List<string> Journal, TieredPolicyAckVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered policy ack variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredPolicyAckVarianceStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered policy ack variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredPolicyAckVarianceStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered policy ack variance {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of tiered policy ack variance workflow.</summary>
public static class TieredPolicyAckVarianceWorkflow
{
    public static TieredPolicyAckVarianceManager NewTieredPolicyAckVarianceManager(ITieredPolicyAckVarianceStore store, IClock clock)
    {
        return new TieredPolicyAckVarianceManager(
            store,
            TieredPolicyAckVarianceRules.DefaultTieredPolicyAckVarianceValidator(),
            clock,
            new List<string>(),
            TieredPolicyAckVarianceEngine.DefaultTieredPolicyAckVarianceEngine());
    }
}
