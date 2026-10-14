namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal garnishment allocations by code.</summary>
public interface ISeasonalGarnishmentAllocationStore
{
    SeasonalGarnishmentAllocation? Find(string code);
    void Save(SeasonalGarnishmentAllocation item);
    bool Remove(string code);
    List<SeasonalGarnishmentAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalGarnishmentAllocationStore(Dictionary<string, SeasonalGarnishmentAllocation> items, List<string> order) : ISeasonalGarnishmentAllocationStore
{
    public Dictionary<string, SeasonalGarnishmentAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalGarnishmentAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalGarnishmentAllocation item)
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

    public List<SeasonalGarnishmentAllocation> List()
    {
        var outValue = new List<SeasonalGarnishmentAllocation>();
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
public sealed class CachedSeasonalGarnishmentAllocationStore(MemorySeasonalGarnishmentAllocationStore inner, SeasonalGarnishmentAllocationCache cache, int hits, int misses) : ISeasonalGarnishmentAllocationStore
{
    public MemorySeasonalGarnishmentAllocationStore Inner { get; set; } = inner;
    public SeasonalGarnishmentAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalGarnishmentAllocation? Find(string code)
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

    public void Save(SeasonalGarnishmentAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalGarnishmentAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal garnishment allocations. Empty fields match all.</summary>
public sealed record SeasonalGarnishmentAllocationQuery(string Unit, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalGarnishmentAllocation item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalGarnishmentAllocation> Apply(List<SeasonalGarnishmentAllocation> items)
    {
        var matched = Collections.Filter(items, (SeasonalGarnishmentAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal garnishment allocation store.</summary>
public static class SeasonalGarnishmentAllocationStore
{
    public static ISeasonalGarnishmentAllocationStore NewMemorySeasonalGarnishmentAllocationStore()
    {
        return new MemorySeasonalGarnishmentAllocationStore(
            new Dictionary<string, SeasonalGarnishmentAllocation>(),
            new List<string>());
    }

    public static ISeasonalGarnishmentAllocationStore NewCachedSeasonalGarnishmentAllocationStore(MemorySeasonalGarnishmentAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalGarnishmentAllocationStore(
            inner,
            SeasonalGarnishmentAllocationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalGarnishmentAllocation> FindSeasonalGarnishmentAllocations(ISeasonalGarnishmentAllocationStore store, SeasonalGarnishmentAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
