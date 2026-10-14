namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted laptop audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedLaptopAuditCoordinator(IWeightedLaptopAuditStore Store, WeightedLaptopAuditValidator Validator, IClock Clock, List<string> Journal, WeightedLaptopAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted laptop audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedLaptopAuditStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted laptop audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedLaptopAuditStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted laptop audit {code} not found");
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

/// <summary>Functions of weighted laptop audit workflow.</summary>
public static class WeightedLaptopAuditWorkflow
{
    public static WeightedLaptopAuditCoordinator NewWeightedLaptopAuditCoordinator(IWeightedLaptopAuditStore store, IClock clock, Registry flags)
    {
        return new WeightedLaptopAuditCoordinator(
            store,
            WeightedLaptopAuditRules.DefaultWeightedLaptopAuditValidator(),
            clock,
            new List<string>(),
            WeightedLaptopAuditEngine.DefaultWeightedLaptopAuditEngine(flags));
    }
}
