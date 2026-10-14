namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback license seat budgets by code.</summary>
public interface IFallbackLicenseSeatBudgetStore
{
    FallbackLicenseSeatBudget? Find(string code);
    void Save(FallbackLicenseSeatBudget item);
    bool Remove(string code);
    List<FallbackLicenseSeatBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackLicenseSeatBudgetStore(Dictionary<string, FallbackLicenseSeatBudget> items, List<string> order) : IFallbackLicenseSeatBudgetStore
{
    public Dictionary<string, FallbackLicenseSeatBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackLicenseSeatBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackLicenseSeatBudget item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<FallbackLicenseSeatBudget> List()
    {
        var outValue = new List<FallbackLicenseSeatBudget>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedFallbackLicenseSeatBudgetStore(MemoryFallbackLicenseSeatBudgetStore inner, FallbackLicenseSeatBudgetCache cache, int hits, int misses) : IFallbackLicenseSeatBudgetStore
{
    public MemoryFallbackLicenseSeatBudgetStore Inner { get; set; } = inner;
    public FallbackLicenseSeatBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackLicenseSeatBudget? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(FallbackLicenseSeatBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackLicenseSeatBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback license seat budgets. Empty fields match all.</summary>
public sealed record FallbackLicenseSeatBudgetQuery(string Channel, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackLicenseSeatBudget item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackLicenseSeatBudget> Apply(List<FallbackLicenseSeatBudget> items)
    {
        var matched = Collections.Filter(items, (FallbackLicenseSeatBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback license seat budget store.</summary>
public static class FallbackLicenseSeatBudgetStore
{
    public static IFallbackLicenseSeatBudgetStore NewMemoryFallbackLicenseSeatBudgetStore()
    {
        return new MemoryFallbackLicenseSeatBudgetStore(
            new Dictionary<string, FallbackLicenseSeatBudget>(),
            new List<string>());
    }

    public static IFallbackLicenseSeatBudgetStore NewCachedFallbackLicenseSeatBudgetStore(MemoryFallbackLicenseSeatBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackLicenseSeatBudgetStore(
            inner,
            FallbackLicenseSeatBudgetCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackLicenseSeatBudget> FindFallbackLicenseSeatBudgets(IFallbackLicenseSeatBudgetStore store, FallbackLicenseSeatBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
