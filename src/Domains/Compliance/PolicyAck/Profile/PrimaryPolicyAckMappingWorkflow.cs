namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary policy ack mappings through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryPolicyAckMappingCoordinator(IPrimaryPolicyAckMappingStore Store, PrimaryPolicyAckMappingValidator Validator, IClock Clock, List<string> Journal, PrimaryPolicyAckMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary policy ack mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryPolicyAckMappingStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary policy ack mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryPolicyAckMappingStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary policy ack mapping {code} not found");
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

/// <summary>Functions of primary policy ack mapping workflow.</summary>
public static class PrimaryPolicyAckMappingWorkflow
{
    public static PrimaryPolicyAckMappingCoordinator NewPrimaryPolicyAckMappingCoordinator(IPrimaryPolicyAckMappingStore store, IClock clock, Registry flags)
    {
        return new PrimaryPolicyAckMappingCoordinator(
            store,
            PrimaryPolicyAckMappingRules.DefaultPrimaryPolicyAckMappingValidator(),
            clock,
            new List<string>(),
            PrimaryPolicyAckMappingEngine.DefaultPrimaryPolicyAckMappingEngine(flags));
    }
}
