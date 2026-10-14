namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves attestation checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationCheckpointManager(IAttestationCheckpointStore Store, AttestationCheckpointValidator Validator, IClock Clock, List<string> Journal, AttestationCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationCheckpointStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationCheckpointStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation checkpoint {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of attestation checkpoint workflow.</summary>
public static class AttestationCheckpointWorkflow
{
    public static AttestationCheckpointManager NewAttestationCheckpointManager(IAttestationCheckpointStore store, IClock clock, Registry flags)
    {
        return new AttestationCheckpointManager(
            store,
            AttestationCheckpointRules.DefaultAttestationCheckpointValidator(),
            clock,
            new List<string>(),
            AttestationCheckpointEngine.DefaultAttestationCheckpointEngine(flags));
    }
}
