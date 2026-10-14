namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves warranty audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyAuditCoordinator(IWarrantyAuditStore Store, WarrantyAuditValidator Validator, IClock Clock, List<string> Journal, WarrantyAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyAuditStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyAuditStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty audit {code} not found");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of warranty audit workflow.</summary>
public static class WarrantyAuditWorkflow
{
    public static WarrantyAuditCoordinator NewWarrantyAuditCoordinator(IWarrantyAuditStore store, IClock clock, Registry flags)
    {
        return new WarrantyAuditCoordinator(
            store,
            WarrantyAuditRules.DefaultWarrantyAuditValidator(),
            clock,
            new List<string>(),
            WarrantyAuditEngine.DefaultWarrantyAuditEngine(flags));
    }
}
