namespace Acme.Workforce.Contractors.Vendor.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global vendor slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalVendorSlotCoordinator(IGlobalVendorSlotStore Store, GlobalVendorSlotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global vendor slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalVendorSlotStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global vendor slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalVendorSlotStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of global vendor slot workflow.</summary>
public static class GlobalVendorSlotWorkflow
{
    public static GlobalVendorSlotCoordinator NewGlobalVendorSlotCoordinator(IGlobalVendorSlotStore store, IClock clock)
    {
        return new GlobalVendorSlotCoordinator(
            store,
            GlobalVendorSlotRules.DefaultGlobalVendorSlotValidator(),
            clock,
            new List<string>());
    }
}
