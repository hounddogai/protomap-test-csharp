namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary mobility audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryMobilityAuditCoordinator(IPrimaryMobilityAuditStore Store, PrimaryMobilityAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary mobility audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryMobilityAuditStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary mobility audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryMobilityAuditStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of primary mobility audit workflow.</summary>
public static class PrimaryMobilityAuditWorkflow
{
    public static PrimaryMobilityAuditCoordinator NewPrimaryMobilityAuditCoordinator(IPrimaryMobilityAuditStore store, IClock clock)
    {
        return new PrimaryMobilityAuditCoordinator(
            store,
            PrimaryMobilityAuditRules.DefaultPrimaryMobilityAuditValidator(),
            clock,
            new List<string>());
    }
}
