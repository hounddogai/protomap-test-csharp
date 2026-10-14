namespace Acme.Workforce.Contractors.Vendor.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves vendor overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorOverrideCoordinator(IVendorOverrideStore Store, VendorOverrideValidator Validator, IClock Clock, List<string> Journal, VendorOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorOverrideStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorOverrideStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor override {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of vendor override workflow.</summary>
public static class VendorOverrideWorkflow
{
    public static VendorOverrideCoordinator NewVendorOverrideCoordinator(IVendorOverrideStore store, IClock clock)
    {
        return new VendorOverrideCoordinator(
            store,
            VendorOverrideRules.DefaultVendorOverrideValidator(),
            clock,
            new List<string>(),
            VendorOverrideEngine.DefaultVendorOverrideEngine());
    }
}
