namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves bonus pool audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolAuditService(IBonusPoolAuditStore Store, BonusPoolAuditValidator Validator, IClock Clock, List<string> Journal, BonusPoolAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolAuditStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolAuditStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool audit {code} not found");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of bonus pool audit workflow.</summary>
public static class BonusPoolAuditWorkflow
{
    public static BonusPoolAuditService NewBonusPoolAuditService(IBonusPoolAuditStore store, IClock clock, Registry flags)
    {
        return new BonusPoolAuditService(
            store,
            BonusPoolAuditRules.DefaultBonusPoolAuditValidator(),
            clock,
            new List<string>(),
            BonusPoolAuditEngine.DefaultBonusPoolAuditEngine(flags));
    }
}
