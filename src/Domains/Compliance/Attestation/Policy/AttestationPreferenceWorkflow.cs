namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves attestation preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationPreferenceDesk(IAttestationPreferenceStore Store, AttestationPreferenceValidator Validator, IClock Clock, List<string> Journal, AttestationPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationPreferenceStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationPreferenceStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation preference {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of attestation preference workflow.</summary>
public static class AttestationPreferenceWorkflow
{
    public static AttestationPreferenceDesk NewAttestationPreferenceDesk(IAttestationPreferenceStore store, IClock clock, Registry flags)
    {
        return new AttestationPreferenceDesk(
            store,
            AttestationPreferenceRules.DefaultAttestationPreferenceValidator(),
            clock,
            new List<string>(),
            AttestationPreferenceEngine.DefaultAttestationPreferenceEngine(flags));
    }
}
