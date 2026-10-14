namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves attestation quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record AttestationQuotaManager(IAttestationQuotaStore Store, AttestationQuotaValidator Validator, IClock Clock, List<string> Journal, AttestationQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AttestationQuotaStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AttestationQuotaStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"attestation quota {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of attestation quota workflow.</summary>
public static class AttestationQuotaWorkflow
{
    public static AttestationQuotaManager NewAttestationQuotaManager(IAttestationQuotaStore store, IClock clock)
    {
        return new AttestationQuotaManager(
            store,
            AttestationQuotaRules.DefaultAttestationQuotaValidator(),
            clock,
            new List<string>(),
            AttestationQuotaEngine.DefaultAttestationQuotaEngine());
    }
}
