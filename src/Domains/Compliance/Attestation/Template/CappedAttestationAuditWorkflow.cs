namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped attestation audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedAttestationAuditManager(ICappedAttestationAuditStore Store, CappedAttestationAuditValidator Validator, IClock Clock, List<string> Journal, CappedAttestationAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped attestation audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedAttestationAuditStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped attestation audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedAttestationAuditStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped attestation audit {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of capped attestation audit workflow.</summary>
public static class CappedAttestationAuditWorkflow
{
    public static CappedAttestationAuditManager NewCappedAttestationAuditManager(ICappedAttestationAuditStore store, IClock clock)
    {
        return new CappedAttestationAuditManager(
            store,
            CappedAttestationAuditRules.DefaultCappedAttestationAuditValidator(),
            clock,
            new List<string>(),
            CappedAttestationAuditEngine.DefaultCappedAttestationAuditEngine());
    }
}
