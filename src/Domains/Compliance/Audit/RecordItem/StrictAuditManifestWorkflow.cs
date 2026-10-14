namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict audit manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictAuditManifestService(IStrictAuditManifestStore Store, StrictAuditManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict audit manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAuditManifestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict audit manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAuditManifestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of strict audit manifest workflow.</summary>
public static class StrictAuditManifestWorkflow
{
    public static StrictAuditManifestService NewStrictAuditManifestService(IStrictAuditManifestStore store, IClock clock)
    {
        return new StrictAuditManifestService(
            store,
            StrictAuditManifestRules.DefaultStrictAuditManifestValidator(),
            clock,
            new List<string>());
    }
}
