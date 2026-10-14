namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional audit records through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalAuditRecordWorkflow(IRegionalAuditRecordStore Store, RegionalAuditRecordValidator Validator, IClock Clock, List<string> Journal)
{
    public static RegionalAuditRecordWorkflow NewRegionalAuditRecordWorkflow(IRegionalAuditRecordStore store, IClock clock)
    {
        return new RegionalAuditRecordWorkflow(
            store,
            RegionalAuditRecordRules.DefaultRegionalAuditRecordValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional audit record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalAuditRecordStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional audit record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalAuditRecordStatus.Closed);
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
