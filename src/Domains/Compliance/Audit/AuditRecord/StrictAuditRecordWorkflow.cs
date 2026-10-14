namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict audit records through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictAuditRecordCoordinator(IStrictAuditRecordStore Store, StrictAuditRecordValidator Validator, IClock Clock, List<string> Journal, StrictAuditRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict audit record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAuditRecordStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict audit record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAuditRecordStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict audit record {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of strict audit record workflow.</summary>
public static class StrictAuditRecordWorkflow
{
    public static StrictAuditRecordCoordinator NewStrictAuditRecordCoordinator(IStrictAuditRecordStore store, IClock clock)
    {
        return new StrictAuditRecordCoordinator(
            store,
            StrictAuditRecordRules.DefaultStrictAuditRecordValidator(),
            clock,
            new List<string>(),
            StrictAuditRecordEngine.DefaultStrictAuditRecordEngine());
    }
}
