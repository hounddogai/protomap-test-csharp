namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback policy ack preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackPolicyAckPreferenceDesk(IFallbackPolicyAckPreferenceStore Store, FallbackPolicyAckPreferenceValidator Validator, IClock Clock, List<string> Journal, FallbackPolicyAckPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback policy ack preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackPolicyAckPreferenceStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback policy ack preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackPolicyAckPreferenceStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback policy ack preference {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of fallback policy ack preference workflow.</summary>
public static class FallbackPolicyAckPreferenceWorkflow
{
    public static FallbackPolicyAckPreferenceDesk NewFallbackPolicyAckPreferenceDesk(IFallbackPolicyAckPreferenceStore store, IClock clock)
    {
        return new FallbackPolicyAckPreferenceDesk(
            store,
            FallbackPolicyAckPreferenceRules.DefaultFallbackPolicyAckPreferenceValidator(),
            clock,
            new List<string>(),
            FallbackPolicyAckPreferenceEngine.DefaultFallbackPolicyAckPreferenceEngine());
    }
}
