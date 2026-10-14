namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending attestation thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingAttestationThresholdCoordinator(IPendingAttestationThresholdStore Store, PendingAttestationThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending attestation threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingAttestationThresholdStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending attestation threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingAttestationThresholdStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of pending attestation threshold workflow.</summary>
public static class PendingAttestationThresholdWorkflow
{
    public static PendingAttestationThresholdCoordinator NewPendingAttestationThresholdCoordinator(IPendingAttestationThresholdStore store, IClock clock)
    {
        return new PendingAttestationThresholdCoordinator(
            store,
            PendingAttestationThresholdRules.DefaultPendingAttestationThresholdValidator(),
            clock,
            new List<string>());
    }
}
