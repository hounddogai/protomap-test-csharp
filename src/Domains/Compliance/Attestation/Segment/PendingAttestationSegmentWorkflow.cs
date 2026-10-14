namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending attestation segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingAttestationSegmentCoordinator(IPendingAttestationSegmentStore Store, PendingAttestationSegmentValidator Validator, IClock Clock, List<string> Journal, PendingAttestationSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending attestation segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingAttestationSegmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending attestation segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingAttestationSegmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending attestation segment {code} not found");
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

/// <summary>Functions of pending attestation segment workflow.</summary>
public static class PendingAttestationSegmentWorkflow
{
    public static PendingAttestationSegmentCoordinator NewPendingAttestationSegmentCoordinator(IPendingAttestationSegmentStore store, IClock clock)
    {
        return new PendingAttestationSegmentCoordinator(
            store,
            PendingAttestationSegmentRules.DefaultPendingAttestationSegmentValidator(),
            clock,
            new List<string>(),
            PendingAttestationSegmentEngine.DefaultPendingAttestationSegmentEngine());
    }
}
