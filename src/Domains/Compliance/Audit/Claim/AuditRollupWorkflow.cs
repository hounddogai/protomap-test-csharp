namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves audit rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditRollupCoordinator(IAuditRollupStore Store, AuditRollupValidator Validator, IClock Clock, List<string> Journal, AuditRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditRollupStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditRollupStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit rollup {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of audit rollup workflow.</summary>
public static class AuditRollupWorkflow
{
    public static AuditRollupCoordinator NewAuditRollupCoordinator(IAuditRollupStore store, IClock clock, Registry flags)
    {
        return new AuditRollupCoordinator(
            store,
            AuditRollupRules.DefaultAuditRollupValidator(),
            clock,
            new List<string>(),
            AuditRollupEngine.DefaultAuditRollupEngine(flags));
    }
}
