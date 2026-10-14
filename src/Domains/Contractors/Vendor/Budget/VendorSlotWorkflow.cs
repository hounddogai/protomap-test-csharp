namespace Acme.Workforce.Contractors.Vendor.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves vendor slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorSlotService(IVendorSlotStore Store, VendorSlotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorSlotStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorSlotStatus.Retired);
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

/// <summary>Functions of vendor slot workflow.</summary>
public static class VendorSlotWorkflow
{
    public static VendorSlotService NewVendorSlotService(IVendorSlotStore store, IClock clock)
    {
        return new VendorSlotService(store, VendorSlotRules.DefaultVendorSlotValidator(), clock, new List<string>());
    }
}
