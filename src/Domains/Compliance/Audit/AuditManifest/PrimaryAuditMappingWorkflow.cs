namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary audit mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryAuditMappingDesk(IPrimaryAuditMappingStore Store, PrimaryAuditMappingValidator Validator, IClock Clock, List<string> Journal, PrimaryAuditMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary audit mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryAuditMappingStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary audit mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryAuditMappingStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary audit mapping {code} not found");
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

/// <summary>Functions of primary audit mapping workflow.</summary>
public static class PrimaryAuditMappingWorkflow
{
    public static PrimaryAuditMappingDesk NewPrimaryAuditMappingDesk(IPrimaryAuditMappingStore store, IClock clock, Registry flags)
    {
        return new PrimaryAuditMappingDesk(
            store,
            PrimaryAuditMappingRules.DefaultPrimaryAuditMappingValidator(),
            clock,
            new List<string>(),
            PrimaryAuditMappingEngine.DefaultPrimaryAuditMappingEngine(flags));
    }
}
