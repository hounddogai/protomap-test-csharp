namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global audit mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalAuditMappingDesk(IGlobalAuditMappingStore Store, GlobalAuditMappingValidator Validator, IClock Clock, List<string> Journal, GlobalAuditMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global audit mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalAuditMappingStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global audit mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalAuditMappingStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global audit mapping {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of global audit mapping workflow.</summary>
public static class GlobalAuditMappingWorkflow
{
    public static GlobalAuditMappingDesk NewGlobalAuditMappingDesk(IGlobalAuditMappingStore store, IClock clock)
    {
        return new GlobalAuditMappingDesk(
            store,
            GlobalAuditMappingRules.DefaultGlobalAuditMappingValidator(),
            clock,
            new List<string>(),
            GlobalAuditMappingEngine.DefaultGlobalAuditMappingEngine());
    }
}
