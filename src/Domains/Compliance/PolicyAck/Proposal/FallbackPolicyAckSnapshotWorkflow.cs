namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback policy ack snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackPolicyAckSnapshotService(IFallbackPolicyAckSnapshotStore Store, FallbackPolicyAckSnapshotValidator Validator, IClock Clock, List<string> Journal, FallbackPolicyAckSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback policy ack snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackPolicyAckSnapshotStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback policy ack snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackPolicyAckSnapshotStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback policy ack snapshot {code} not found");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of fallback policy ack snapshot workflow.</summary>
public static class FallbackPolicyAckSnapshotWorkflow
{
    public static FallbackPolicyAckSnapshotService NewFallbackPolicyAckSnapshotService(IFallbackPolicyAckSnapshotStore store, IClock clock, Registry flags)
    {
        return new FallbackPolicyAckSnapshotService(
            store,
            FallbackPolicyAckSnapshotRules.DefaultFallbackPolicyAckSnapshotValidator(),
            clock,
            new List<string>(),
            FallbackPolicyAckSnapshotEngine.DefaultFallbackPolicyAckSnapshotEngine(flags));
    }
}
