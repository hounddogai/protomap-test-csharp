namespace Acme.Workforce.Compliance.Audit.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves audit mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditMappingCoordinator(IAuditMappingStore Store, AuditMappingValidator Validator, IClock Clock, List<string> Journal, AuditMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditMappingStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditMappingStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit mapping {code} not found");
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

/// <summary>Functions of audit mapping workflow.</summary>
public static class AuditMappingWorkflow
{
    public static AuditMappingCoordinator NewAuditMappingCoordinator(IAuditMappingStore store, IClock clock, Registry flags)
    {
        return new AuditMappingCoordinator(
            store,
            AuditMappingRules.DefaultAuditMappingValidator(),
            clock,
            new List<string>(),
            AuditMappingEngine.DefaultAuditMappingEngine(flags));
    }
}
