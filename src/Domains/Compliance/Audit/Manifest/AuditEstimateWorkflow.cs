namespace Acme.Workforce.Compliance.Audit.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves audit estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditEstimateManager(IAuditEstimateStore Store, AuditEstimateValidator Validator, IClock Clock, List<string> Journal, AuditEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditEstimateStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditEstimateStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit estimate {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of audit estimate workflow.</summary>
public static class AuditEstimateWorkflow
{
    public static AuditEstimateManager NewAuditEstimateManager(IAuditEstimateStore store, IClock clock, Registry flags)
    {
        return new AuditEstimateManager(
            store,
            AuditEstimateRules.DefaultAuditEstimateValidator(),
            clock,
            new List<string>(),
            AuditEstimateEngine.DefaultAuditEstimateEngine(flags));
    }
}
