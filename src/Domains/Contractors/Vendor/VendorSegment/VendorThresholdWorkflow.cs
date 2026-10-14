namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves vendor thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorThresholdManager(IVendorThresholdStore Store, VendorThresholdValidator Validator, IClock Clock, List<string> Journal, VendorThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorThresholdStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorThresholdStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor threshold {code} not found");
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

/// <summary>Functions of vendor threshold workflow.</summary>
public static class VendorThresholdWorkflow
{
    public static VendorThresholdManager NewVendorThresholdManager(IVendorThresholdStore store, IClock clock, Registry flags)
    {
        return new VendorThresholdManager(
            store,
            VendorThresholdRules.DefaultVendorThresholdValidator(),
            clock,
            new List<string>(),
            VendorThresholdEngine.DefaultVendorThresholdEngine(flags));
    }
}
