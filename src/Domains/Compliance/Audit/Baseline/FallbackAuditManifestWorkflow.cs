namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback audit manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackAuditManifestCoordinator(IFallbackAuditManifestStore Store, FallbackAuditManifestValidator Validator, IClock Clock, List<string> Journal, FallbackAuditManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback audit manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackAuditManifestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback audit manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackAuditManifestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback audit manifest {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of fallback audit manifest workflow.</summary>
public static class FallbackAuditManifestWorkflow
{
    public static FallbackAuditManifestCoordinator NewFallbackAuditManifestCoordinator(IFallbackAuditManifestStore store, IClock clock, Registry flags)
    {
        return new FallbackAuditManifestCoordinator(
            store,
            FallbackAuditManifestRules.DefaultFallbackAuditManifestValidator(),
            clock,
            new List<string>(),
            FallbackAuditManifestEngine.DefaultFallbackAuditManifestEngine(flags));
    }
}
