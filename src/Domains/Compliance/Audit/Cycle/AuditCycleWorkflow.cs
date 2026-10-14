namespace Acme.Workforce.Compliance.Audit.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves audit cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditCycleManager(IAuditCycleStore Store, AuditCycleValidator Validator, IClock Clock, List<string> Journal, AuditCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditCycleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditCycleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit cycle {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of audit cycle workflow.</summary>
public static class AuditCycleWorkflow
{
    public static AuditCycleManager NewAuditCycleManager(IAuditCycleStore store, IClock clock, Registry flags)
    {
        return new AuditCycleManager(
            store,
            AuditCycleRules.DefaultAuditCycleValidator(),
            clock,
            new List<string>(),
            AuditCycleEngine.DefaultAuditCycleEngine(flags));
    }
}
