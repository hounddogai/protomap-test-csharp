namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted policy ack profiles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedPolicyAckProfileManager(IWeightedPolicyAckProfileStore Store, WeightedPolicyAckProfileValidator Validator, IClock Clock, List<string> Journal, WeightedPolicyAckProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted policy ack profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedPolicyAckProfileStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted policy ack profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedPolicyAckProfileStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted policy ack profile {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of weighted policy ack profile workflow.</summary>
public static class WeightedPolicyAckProfileWorkflow
{
    public static WeightedPolicyAckProfileManager NewWeightedPolicyAckProfileManager(IWeightedPolicyAckProfileStore store, IClock clock, Registry flags)
    {
        return new WeightedPolicyAckProfileManager(
            store,
            WeightedPolicyAckProfileRules.DefaultWeightedPolicyAckProfileValidator(),
            clock,
            new List<string>(),
            WeightedPolicyAckProfileEngine.DefaultWeightedPolicyAckProfileEngine(flags));
    }
}
