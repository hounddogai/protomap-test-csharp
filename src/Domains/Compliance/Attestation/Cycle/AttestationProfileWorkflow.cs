namespace Acme.Workforce.Compliance.Attestation.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves attestation profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationProfileCoordinator(IAttestationProfileStore Store, AttestationProfileValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationProfileStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationProfileStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of attestation profile workflow.</summary>
public static class AttestationProfileWorkflow
{
    public static AttestationProfileCoordinator NewAttestationProfileCoordinator(IAttestationProfileStore store, IClock clock)
    {
        return new AttestationProfileCoordinator(
            store,
            AttestationProfileRules.DefaultAttestationProfileValidator(),
            clock,
            new List<string>());
    }
}
