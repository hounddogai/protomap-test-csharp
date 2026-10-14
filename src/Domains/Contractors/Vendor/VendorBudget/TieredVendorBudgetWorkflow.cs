namespace Acme.Workforce.Contractors.Vendor.VendorBudget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered vendor budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredVendorBudgetService(ITieredVendorBudgetStore Store, TieredVendorBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered vendor budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredVendorBudgetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered vendor budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredVendorBudgetStatus.Closed);
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

/// <summary>Functions of tiered vendor budget workflow.</summary>
public static class TieredVendorBudgetWorkflow
{
    public static TieredVendorBudgetService NewTieredVendorBudgetService(ITieredVendorBudgetStore store, IClock clock)
    {
        return new TieredVendorBudgetService(
            store,
            TieredVendorBudgetRules.DefaultTieredVendorBudgetValidator(),
            clock,
            new List<string>());
    }
}
