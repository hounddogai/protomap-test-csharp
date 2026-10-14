namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient garnishment mappings by code.</summary>
public interface ILenientGarnishmentMappingStore
{
    LenientGarnishmentMapping? Find(string code);
    void Save(LenientGarnishmentMapping item);
    bool Remove(string code);
    List<LenientGarnishmentMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientGarnishmentMappingStore(Dictionary<string, LenientGarnishmentMapping> items, List<string> order) : ILenientGarnishmentMappingStore
{
    public Dictionary<string, LenientGarnishmentMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientGarnishmentMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientGarnishmentMapping item)
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

    public List<LenientGarnishmentMapping> List()
    {
        var outValue = new List<LenientGarnishmentMapping>();
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
public sealed class CachedLenientGarnishmentMappingStore(MemoryLenientGarnishmentMappingStore inner, LenientGarnishmentMappingCache cache, int hits, int misses) : ILenientGarnishmentMappingStore
{
    public MemoryLenientGarnishmentMappingStore Inner { get; set; } = inner;
    public LenientGarnishmentMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientGarnishmentMapping? Find(string code)
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

    public void Save(LenientGarnishmentMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientGarnishmentMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient garnishment mappings. Empty fields match all.</summary>
public sealed record LenientGarnishmentMappingQuery(string Department, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(LenientGarnishmentMapping item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<LenientGarnishmentMapping> Apply(List<LenientGarnishmentMapping> items)
    {
        var matched = Collections.Filter(items, (LenientGarnishmentMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient garnishment mapping store.</summary>
public static class LenientGarnishmentMappingStore
{
    public static ILenientGarnishmentMappingStore NewMemoryLenientGarnishmentMappingStore()
    {
        return new MemoryLenientGarnishmentMappingStore(
            new Dictionary<string, LenientGarnishmentMapping>(),
            new List<string>());
    }

    public static ILenientGarnishmentMappingStore NewCachedLenientGarnishmentMappingStore(MemoryLenientGarnishmentMappingStore inner, int ttl, IClock clock)
    {
        return new CachedLenientGarnishmentMappingStore(
            inner,
            LenientGarnishmentMappingCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientGarnishmentMapping> FindLenientGarnishmentMappings(ILenientGarnishmentMappingStore store, LenientGarnishmentMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
