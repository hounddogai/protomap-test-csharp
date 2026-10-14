namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched audit cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedAuditCycleDesk(IBatchedAuditCycleStore Store, BatchedAuditCycleValidator Validator, IClock Clock, List<string> Journal, BatchedAuditCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched audit cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedAuditCycleStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched audit cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedAuditCycleStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched audit cycle {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of batched audit cycle workflow.</summary>
public static class BatchedAuditCycleWorkflow
{
    public static BatchedAuditCycleDesk NewBatchedAuditCycleDesk(IBatchedAuditCycleStore store, IClock clock, Registry flags)
    {
        return new BatchedAuditCycleDesk(
            store,
            BatchedAuditCycleRules.DefaultBatchedAuditCycleValidator(),
            clock,
            new List<string>(),
            BatchedAuditCycleEngine.DefaultBatchedAuditCycleEngine(flags));
    }
}
