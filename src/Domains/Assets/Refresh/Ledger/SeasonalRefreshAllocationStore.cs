namespace Acme.Workforce.Assets.Refresh.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal refresh allocations by code.</summary>
public interface ISeasonalRefreshAllocationStore
{
    SeasonalRefreshAllocation? Find(string code);
    void Save(SeasonalRefreshAllocation item);
    bool Remove(string code);
    List<SeasonalRefreshAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalRefreshAllocationStore(Dictionary<string, SeasonalRefreshAllocation> items, List<string> order) : ISeasonalRefreshAllocationStore
{
    public Dictionary<string, SeasonalRefreshAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalRefreshAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalRefreshAllocation item)
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

    public List<SeasonalRefreshAllocation> List()
    {
        var outValue = new List<SeasonalRefreshAllocation>();
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
public sealed class CachedSeasonalRefreshAllocationStore(MemorySeasonalRefreshAllocationStore inner, SeasonalRefreshAllocationCache cache, int hits, int misses) : ISeasonalRefreshAllocationStore
{
    public MemorySeasonalRefreshAllocationStore Inner { get; set; } = inner;
    public SeasonalRefreshAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalRefreshAllocation? Find(string code)
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

    public void Save(SeasonalRefreshAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalRefreshAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal refresh allocations. Empty fields match all.</summary>
public sealed record SeasonalRefreshAllocationQuery(string Locale, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalRefreshAllocation item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalRefreshAllocation> Apply(List<SeasonalRefreshAllocation> items)
    {
        var matched = Collections.Filter(items, (SeasonalRefreshAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal refresh allocation store.</summary>
public static class SeasonalRefreshAllocationStore
{
    public static ISeasonalRefreshAllocationStore NewMemorySeasonalRefreshAllocationStore()
    {
        return new MemorySeasonalRefreshAllocationStore(
            new Dictionary<string, SeasonalRefreshAllocation>(),
            new List<string>());
    }

    public static ISeasonalRefreshAllocationStore NewCachedSeasonalRefreshAllocationStore(MemorySeasonalRefreshAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalRefreshAllocationStore(
            inner,
            SeasonalRefreshAllocationCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalRefreshAllocation> FindSeasonalRefreshAllocations(ISeasonalRefreshAllocationStore store, SeasonalRefreshAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
