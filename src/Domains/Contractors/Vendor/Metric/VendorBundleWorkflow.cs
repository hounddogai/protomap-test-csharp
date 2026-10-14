namespace Acme.Workforce.Contractors.Vendor.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves vendor bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorBundleManager(IVendorBundleStore Store, VendorBundleValidator Validator, IClock Clock, List<string> Journal, VendorBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorBundleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorBundleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor bundle {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of vendor bundle workflow.</summary>
public static class VendorBundleWorkflow
{
    public static VendorBundleManager NewVendorBundleManager(IVendorBundleStore store, IClock clock)
    {
        return new VendorBundleManager(
            store,
            VendorBundleRules.DefaultVendorBundleValidator(),
            clock,
            new List<string>(),
            VendorBundleEngine.DefaultVendorBundleEngine());
    }
}
