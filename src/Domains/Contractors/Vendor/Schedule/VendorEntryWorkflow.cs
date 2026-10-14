namespace Acme.Workforce.Contractors.Vendor.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves vendor entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorEntryWorkflow(IVendorEntryStore Store, VendorEntryValidator Validator, IClock Clock, List<string> Journal)
{
    public static VendorEntryWorkflow NewVendorEntryWorkflow(IVendorEntryStore store, IClock clock)
    {
        return new VendorEntryWorkflow(
            store,
            VendorEntryRules.DefaultVendorEntryValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorEntryStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorEntryStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
