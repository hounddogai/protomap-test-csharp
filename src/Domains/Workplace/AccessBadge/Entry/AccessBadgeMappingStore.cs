namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge mappings by code.</summary>
public interface IAccessBadgeMappingStore
{
    AccessBadgeMapping? Find(string code);
    void Save(AccessBadgeMapping item);
    bool Remove(string code);
    List<AccessBadgeMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeMappingStore(Dictionary<string, AccessBadgeMapping> items, List<string> order) : IAccessBadgeMappingStore
{
    public Dictionary<string, AccessBadgeMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeMapping item)
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

    public List<AccessBadgeMapping> List()
    {
        var outValue = new List<AccessBadgeMapping>();
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
public sealed class CachedAccessBadgeMappingStore(MemoryAccessBadgeMappingStore inner, AccessBadgeMappingCache cache, int hits, int misses) : IAccessBadgeMappingStore
{
    public MemoryAccessBadgeMappingStore Inner { get; set; } = inner;
    public AccessBadgeMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeMapping? Find(string code)
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

    public void Save(AccessBadgeMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge mappings. Empty fields match all.</summary>
public sealed record AccessBadgeMappingQuery(string Category, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeMapping item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeMapping> Apply(List<AccessBadgeMapping> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge mapping store.</summary>
public static class AccessBadgeMappingStore
{
    public static IAccessBadgeMappingStore NewMemoryAccessBadgeMappingStore()
    {
        return new MemoryAccessBadgeMappingStore(new Dictionary<string, AccessBadgeMapping>(), new List<string>());
    }

    public static IAccessBadgeMappingStore NewCachedAccessBadgeMappingStore(MemoryAccessBadgeMappingStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeMappingStore(inner, AccessBadgeMappingCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeMapping> FindAccessBadgeMappings(IAccessBadgeMappingStore store, AccessBadgeMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
