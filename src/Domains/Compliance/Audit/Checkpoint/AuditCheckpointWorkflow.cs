namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves audit checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditCheckpointManager(IAuditCheckpointStore Store, AuditCheckpointValidator Validator, IClock Clock, List<string> Journal, AuditCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditCheckpointStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditCheckpointStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit checkpoint {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of audit checkpoint workflow.</summary>
public static class AuditCheckpointWorkflow
{
    public static AuditCheckpointManager NewAuditCheckpointManager(IAuditCheckpointStore store, IClock clock, Registry flags)
    {
        return new AuditCheckpointManager(
            store,
            AuditCheckpointRules.DefaultAuditCheckpointValidator(),
            clock,
            new List<string>(),
            AuditCheckpointEngine.DefaultAuditCheckpointEngine(flags));
    }
}
