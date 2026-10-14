namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive warranty audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveWarrantyAuditManager(IAdaptiveWarrantyAuditStore Store, AdaptiveWarrantyAuditValidator Validator, IClock Clock, List<string> Journal, AdaptiveWarrantyAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive warranty audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveWarrantyAuditStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive warranty audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveWarrantyAuditStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive warranty audit {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of adaptive warranty audit workflow.</summary>
public static class AdaptiveWarrantyAuditWorkflow
{
    public static AdaptiveWarrantyAuditManager NewAdaptiveWarrantyAuditManager(IAdaptiveWarrantyAuditStore store, IClock clock, Registry flags)
    {
        return new AdaptiveWarrantyAuditManager(
            store,
            AdaptiveWarrantyAuditRules.DefaultAdaptiveWarrantyAuditValidator(),
            clock,
            new List<string>(),
            AdaptiveWarrantyAuditEngine.DefaultAdaptiveWarrantyAuditEngine(flags));
    }
}
