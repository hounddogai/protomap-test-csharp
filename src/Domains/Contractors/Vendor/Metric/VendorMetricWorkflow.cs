namespace Acme.Workforce.Contractors.Vendor.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves vendor metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorMetricDesk(IVendorMetricStore Store, VendorMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorMetricStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorMetricStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of vendor metric workflow.</summary>
public static class VendorMetricWorkflow
{
    public static VendorMetricDesk NewVendorMetricDesk(IVendorMetricStore store, IClock clock)
    {
        return new VendorMetricDesk(
            store,
            VendorMetricRules.DefaultVendorMetricValidator(),
            clock,
            new List<string>());
    }
}
