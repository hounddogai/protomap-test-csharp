namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves vendor rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorRollupManager(IVendorRollupStore Store, VendorRollupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorRollupStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorRollupStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of vendor rollup workflow.</summary>
public static class VendorRollupWorkflow
{
    public static VendorRollupManager NewVendorRollupManager(IVendorRollupStore store, IClock clock)
    {
        return new VendorRollupManager(
            store,
            VendorRollupRules.DefaultVendorRollupValidator(),
            clock,
            new List<string>());
    }
}
