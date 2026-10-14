namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty overrides by code.</summary>
public interface IWarrantyOverrideStore
{
    WarrantyOverride? Find(string code);
    void Save(WarrantyOverride item);
    bool Remove(string code);
    List<WarrantyOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyOverrideStore(Dictionary<string, WarrantyOverride> items, List<string> order) : IWarrantyOverrideStore
{
    public Dictionary<string, WarrantyOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyOverride item)
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

    public List<WarrantyOverride> List()
    {
        var outValue = new List<WarrantyOverride>();
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
public sealed class CachedWarrantyOverrideStore(MemoryWarrantyOverrideStore inner, WarrantyOverrideCache cache, int hits, int misses) : IWarrantyOverrideStore
{
    public MemoryWarrantyOverrideStore Inner { get; set; } = inner;
    public WarrantyOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyOverride? Find(string code)
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

    public void Save(WarrantyOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty overrides. Empty fields match all.</summary>
public sealed record WarrantyOverrideQuery(string Locale, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyOverride item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyOverride> Apply(List<WarrantyOverride> items)
    {
        var matched = Collections.Filter(items, (WarrantyOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty override store.</summary>
public static class WarrantyOverrideStore
{
    public static IWarrantyOverrideStore NewMemoryWarrantyOverrideStore()
    {
        return new MemoryWarrantyOverrideStore(new Dictionary<string, WarrantyOverride>(), new List<string>());
    }

    public static IWarrantyOverrideStore NewCachedWarrantyOverrideStore(MemoryWarrantyOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyOverrideStore(inner, WarrantyOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<WarrantyOverride> FindWarrantyOverrides(IWarrantyOverrideStore store, WarrantyOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
