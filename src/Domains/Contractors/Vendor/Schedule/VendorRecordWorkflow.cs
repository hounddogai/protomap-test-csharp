namespace Acme.Workforce.Contractors.Vendor.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves vendor records through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorRecordDesk(IVendorRecordStore Store, VendorRecordValidator Validator, IClock Clock, List<string> Journal, VendorRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorRecordStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorRecordStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor record {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of vendor record workflow.</summary>
public static class VendorRecordWorkflow
{
    public static VendorRecordDesk NewVendorRecordDesk(IVendorRecordStore store, IClock clock, Registry flags)
    {
        return new VendorRecordDesk(
            store,
            VendorRecordRules.DefaultVendorRecordValidator(),
            clock,
            new List<string>(),
            VendorRecordEngine.DefaultVendorRecordEngine(flags));
    }
}
