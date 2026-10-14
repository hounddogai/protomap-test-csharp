namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves vendor tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorTierDesk(IVendorTierStore Store, VendorTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorTierStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorTierStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of vendor tier workflow.</summary>
public static class VendorTierWorkflow
{
    public static VendorTierDesk NewVendorTierDesk(IVendorTierStore store, IClock clock)
    {
        return new VendorTierDesk(store, VendorTierRules.DefaultVendorTierValidator(), clock, new List<string>());
    }
}
