namespace Acme.Workforce.Compliance.Attestation.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves attestation audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationAuditManager(IAttestationAuditStore Store, AttestationAuditValidator Validator, IClock Clock, List<string> Journal, AttestationAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationAuditStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationAuditStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation audit {code} not found");
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

/// <summary>Functions of attestation audit workflow.</summary>
public static class AttestationAuditWorkflow
{
    public static AttestationAuditManager NewAttestationAuditManager(IAttestationAuditStore store, IClock clock)
    {
        return new AttestationAuditManager(
            store,
            AttestationAuditRules.DefaultAttestationAuditValidator(),
            clock,
            new List<string>(),
            AttestationAuditEngine.DefaultAttestationAuditEngine());
    }
}
