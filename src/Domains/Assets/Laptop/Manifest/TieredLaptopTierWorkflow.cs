namespace Acme.Workforce.Assets.Laptop.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered laptop tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredLaptopTierService(ITieredLaptopTierStore Store, TieredLaptopTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered laptop tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredLaptopTierStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered laptop tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredLaptopTierStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of tiered laptop tier workflow.</summary>
public static class TieredLaptopTierWorkflow
{
    public static TieredLaptopTierService NewTieredLaptopTierService(ITieredLaptopTierStore store, IClock clock)
    {
        return new TieredLaptopTierService(
            store,
            TieredLaptopTierRules.DefaultTieredLaptopTierValidator(),
            clock,
            new List<string>());
    }
}
