namespace Acme.Workforce.Contractors.Vendor.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict vendor requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictVendorRequestCoordinator(IStrictVendorRequestStore Store, StrictVendorRequestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict vendor request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictVendorRequestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict vendor request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictVendorRequestStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of strict vendor request workflow.</summary>
public static class StrictVendorRequestWorkflow
{
    public static StrictVendorRequestCoordinator NewStrictVendorRequestCoordinator(IStrictVendorRequestStore store, IClock clock)
    {
        return new StrictVendorRequestCoordinator(
            store,
            StrictVendorRequestRules.DefaultStrictVendorRequestValidator(),
            clock,
            new List<string>());
    }
}
