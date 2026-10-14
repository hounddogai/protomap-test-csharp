namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves attestation batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationBatchDesk(IAttestationBatchStore Store, AttestationBatchValidator Validator, IClock Clock, List<string> Journal, AttestationBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationBatchStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationBatchStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation batch {code} not found");
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

/// <summary>Functions of attestation batch workflow.</summary>
public static class AttestationBatchWorkflow
{
    public static AttestationBatchDesk NewAttestationBatchDesk(IAttestationBatchStore store, IClock clock, Registry flags)
    {
        return new AttestationBatchDesk(
            store,
            AttestationBatchRules.DefaultAttestationBatchValidator(),
            clock,
            new List<string>(),
            AttestationBatchEngine.DefaultAttestationBatchEngine(flags));
    }
}
