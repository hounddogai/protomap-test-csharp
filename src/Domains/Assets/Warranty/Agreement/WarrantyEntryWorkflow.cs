namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves warranty entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyEntryCoordinator(IWarrantyEntryStore Store, WarrantyEntryValidator Validator, IClock Clock, List<string> Journal, WarrantyEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyEntryStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyEntryStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty entry {code} not found");
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

/// <summary>Functions of warranty entry workflow.</summary>
public static class WarrantyEntryWorkflow
{
    public static WarrantyEntryCoordinator NewWarrantyEntryCoordinator(IWarrantyEntryStore store, IClock clock, Registry flags)
    {
        return new WarrantyEntryCoordinator(
            store,
            WarrantyEntryRules.DefaultWarrantyEntryValidator(),
            clock,
            new List<string>(),
            WarrantyEntryEngine.DefaultWarrantyEntryEngine(flags));
    }
}
