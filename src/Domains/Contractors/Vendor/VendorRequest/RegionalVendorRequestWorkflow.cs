namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional vendor requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalVendorRequestCoordinator(IRegionalVendorRequestStore Store, RegionalVendorRequestValidator Validator, IClock Clock, List<string> Journal, RegionalVendorRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional vendor request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalVendorRequestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional vendor request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalVendorRequestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional vendor request {code} not found");
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

/// <summary>Functions of regional vendor request workflow.</summary>
public static class RegionalVendorRequestWorkflow
{
    public static RegionalVendorRequestCoordinator NewRegionalVendorRequestCoordinator(IRegionalVendorRequestStore store, IClock clock, Registry flags)
    {
        return new RegionalVendorRequestCoordinator(
            store,
            RegionalVendorRequestRules.DefaultRegionalVendorRequestValidator(),
            clock,
            new List<string>(),
            RegionalVendorRequestEngine.DefaultRegionalVendorRequestEngine(flags));
    }
}
