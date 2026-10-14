namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant mappings by code.</summary>
public interface IEquityGrantMappingStore
{
    EquityGrantMapping? Find(string code);
    void Save(EquityGrantMapping item);
    bool Remove(string code);
    List<EquityGrantMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantMappingStore(Dictionary<string, EquityGrantMapping> items, List<string> order) : IEquityGrantMappingStore
{
    public Dictionary<string, EquityGrantMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantMapping item)
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

    public List<EquityGrantMapping> List()
    {
        var outValue = new List<EquityGrantMapping>();
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
public sealed class CachedEquityGrantMappingStore(MemoryEquityGrantMappingStore inner, EquityGrantMappingCache cache, int hits, int misses) : IEquityGrantMappingStore
{
    public MemoryEquityGrantMappingStore Inner { get; set; } = inner;
    public EquityGrantMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantMapping? Find(string code)
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

    public void Save(EquityGrantMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant mappings. Empty fields match all.</summary>
public sealed record EquityGrantMappingQuery(string Category, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantMapping item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantMapping> Apply(List<EquityGrantMapping> items)
    {
        var matched = Collections.Filter(items, (EquityGrantMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant mapping store.</summary>
public static class EquityGrantMappingStore
{
    public static IEquityGrantMappingStore NewMemoryEquityGrantMappingStore()
    {
        return new MemoryEquityGrantMappingStore(new Dictionary<string, EquityGrantMapping>(), new List<string>());
    }

    public static IEquityGrantMappingStore NewCachedEquityGrantMappingStore(MemoryEquityGrantMappingStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantMappingStore(inner, EquityGrantMappingCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EquityGrantMapping> FindEquityGrantMappings(IEquityGrantMappingStore store, EquityGrantMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
