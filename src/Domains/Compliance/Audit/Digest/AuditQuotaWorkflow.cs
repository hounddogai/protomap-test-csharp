namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves audit quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditQuotaManager(IAuditQuotaStore Store, AuditQuotaValidator Validator, IClock Clock, List<string> Journal, AuditQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditQuotaStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditQuotaStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit quota {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of audit quota workflow.</summary>
public static class AuditQuotaWorkflow
{
    public static AuditQuotaManager NewAuditQuotaManager(IAuditQuotaStore store, IClock clock)
    {
        return new AuditQuotaManager(
            store,
            AuditQuotaRules.DefaultAuditQuotaValidator(),
            clock,
            new List<string>(),
            AuditQuotaEngine.DefaultAuditQuotaEngine());
    }
}
