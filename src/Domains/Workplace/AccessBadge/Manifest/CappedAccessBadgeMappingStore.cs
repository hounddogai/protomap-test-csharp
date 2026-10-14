namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped access badge mappings by code.</summary>
public interface ICappedAccessBadgeMappingStore
{
    CappedAccessBadgeMapping? Find(string code);
    void Save(CappedAccessBadgeMapping item);
    bool Remove(string code);
    List<CappedAccessBadgeMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAccessBadgeMappingStore(Dictionary<string, CappedAccessBadgeMapping> items, List<string> order) : ICappedAccessBadgeMappingStore
{
    public Dictionary<string, CappedAccessBadgeMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAccessBadgeMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAccessBadgeMapping item)
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

    public List<CappedAccessBadgeMapping> List()
    {
        var outValue = new List<CappedAccessBadgeMapping>();
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
public sealed class CachedCappedAccessBadgeMappingStore(MemoryCappedAccessBadgeMappingStore inner, CappedAccessBadgeMappingCache cache, int hits, int misses) : ICappedAccessBadgeMappingStore
{
    public MemoryCappedAccessBadgeMappingStore Inner { get; set; } = inner;
    public CappedAccessBadgeMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAccessBadgeMapping? Find(string code)
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

    public void Save(CappedAccessBadgeMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAccessBadgeMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped access badge mappings. Empty fields match all.</summary>
public sealed record CappedAccessBadgeMappingQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAccessBadgeMapping item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<CappedAccessBadgeMapping> Apply(List<CappedAccessBadgeMapping> items)
    {
        var matched = Collections.Filter(items, (CappedAccessBadgeMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped access badge mapping store.</summary>
public static class CappedAccessBadgeMappingStore
{
    public static ICappedAccessBadgeMappingStore NewMemoryCappedAccessBadgeMappingStore()
    {
        return new MemoryCappedAccessBadgeMappingStore(
            new Dictionary<string, CappedAccessBadgeMapping>(),
            new List<string>());
    }

    public static ICappedAccessBadgeMappingStore NewCachedCappedAccessBadgeMappingStore(MemoryCappedAccessBadgeMappingStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAccessBadgeMappingStore(
            inner,
            CappedAccessBadgeMappingCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedAccessBadgeMapping> FindCappedAccessBadgeMappings(ICappedAccessBadgeMappingStore store, CappedAccessBadgeMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
