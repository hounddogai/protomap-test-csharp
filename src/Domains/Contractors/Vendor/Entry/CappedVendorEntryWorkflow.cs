namespace Acme.Workforce.Contractors.Vendor.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped vendor entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedVendorEntryService(ICappedVendorEntryStore Store, CappedVendorEntryValidator Validator, IClock Clock, List<string> Journal, CappedVendorEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped vendor entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedVendorEntryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped vendor entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedVendorEntryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped vendor entry {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of capped vendor entry workflow.</summary>
public static class CappedVendorEntryWorkflow
{
    public static CappedVendorEntryService NewCappedVendorEntryService(ICappedVendorEntryStore store, IClock clock)
    {
        return new CappedVendorEntryService(
            store,
            CappedVendorEntryRules.DefaultCappedVendorEntryValidator(),
            clock,
            new List<string>(),
            CappedVendorEntryEngine.DefaultCappedVendorEntryEngine());
    }
}
