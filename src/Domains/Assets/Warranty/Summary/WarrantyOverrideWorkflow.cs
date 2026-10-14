namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyOverrideManager(IWarrantyOverrideStore Store, WarrantyOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyOverrideStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyOverrideStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of warranty override workflow.</summary>
public static class WarrantyOverrideWorkflow
{
    public static WarrantyOverrideManager NewWarrantyOverrideManager(IWarrantyOverrideStore store, IClock clock)
    {
        return new WarrantyOverrideManager(
            store,
            WarrantyOverrideRules.DefaultWarrantyOverrideValidator(),
            clock,
            new List<string>());
    }
}
