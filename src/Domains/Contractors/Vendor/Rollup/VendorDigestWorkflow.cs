namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves vendor digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorDigestDesk(IVendorDigestStore Store, VendorDigestValidator Validator, IClock Clock, List<string> Journal, VendorDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorDigestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorDigestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor digest {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of vendor digest workflow.</summary>
public static class VendorDigestWorkflow
{
    public static VendorDigestDesk NewVendorDigestDesk(IVendorDigestStore store, IClock clock)
    {
        return new VendorDigestDesk(
            store,
            VendorDigestRules.DefaultVendorDigestValidator(),
            clock,
            new List<string>(),
            VendorDigestEngine.DefaultVendorDigestEngine());
    }
}
