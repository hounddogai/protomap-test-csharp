namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves vendor mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorMappingDesk(IVendorMappingStore Store, VendorMappingValidator Validator, IClock Clock, List<string> Journal, VendorMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorMappingStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorMappingStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor mapping {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of vendor mapping workflow.</summary>
public static class VendorMappingWorkflow
{
    public static VendorMappingDesk NewVendorMappingDesk(IVendorMappingStore store, IClock clock, Registry flags)
    {
        return new VendorMappingDesk(
            store,
            VendorMappingRules.DefaultVendorMappingValidator(),
            clock,
            new List<string>(),
            VendorMappingEngine.DefaultVendorMappingEngine(flags));
    }
}
