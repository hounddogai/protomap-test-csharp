namespace Acme.Workforce.Compliance.Certification.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves certification quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record CertificationQuotaManager(ICertificationQuotaStore Store, CertificationQuotaValidator Validator, IClock Clock, List<string> Journal, CertificationQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CertificationQuotaStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CertificationQuotaStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"certification quota {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of certification quota workflow.</summary>
public static class CertificationQuotaWorkflow
{
    public static CertificationQuotaManager NewCertificationQuotaManager(ICertificationQuotaStore store, IClock clock)
    {
        return new CertificationQuotaManager(
            store,
            CertificationQuotaRules.DefaultCertificationQuotaValidator(),
            clock,
            new List<string>(),
            CertificationQuotaEngine.DefaultCertificationQuotaEngine());
    }
}
