namespace Acme.Workforce.Contractors.Rate.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary rate tiers by code.</summary>
public interface IPrimaryRateTierStore
{
    PrimaryRateTier? Find(string code);
    void Save(PrimaryRateTier item);
    bool Remove(string code);
    List<PrimaryRateTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryRateTierStore(Dictionary<string, PrimaryRateTier> items, List<string> order) : IPrimaryRateTierStore
{
    public Dictionary<string, PrimaryRateTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryRateTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryRateTier item)
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

    public List<PrimaryRateTier> List()
    {
        var outValue = new List<PrimaryRateTier>();
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
public sealed class CachedPrimaryRateTierStore(MemoryPrimaryRateTierStore inner, PrimaryRateTierCache cache, int hits, int misses) : IPrimaryRateTierStore
{
    public MemoryPrimaryRateTierStore Inner { get; set; } = inner;
    public PrimaryRateTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryRateTier? Find(string code)
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

    public void Save(PrimaryRateTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryRateTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary rate tiers. Empty fields match all.</summary>
public sealed record PrimaryRateTierQuery(string Locale, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryRateTier item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryRateTier> Apply(List<PrimaryRateTier> items)
    {
        var matched = Collections.Filter(items, (PrimaryRateTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary rate tier store.</summary>
public static class PrimaryRateTierStore
{
    public static IPrimaryRateTierStore NewMemoryPrimaryRateTierStore()
    {
        return new MemoryPrimaryRateTierStore(new Dictionary<string, PrimaryRateTier>(), new List<string>());
    }

    public static IPrimaryRateTierStore NewCachedPrimaryRateTierStore(MemoryPrimaryRateTierStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryRateTierStore(inner, PrimaryRateTierCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PrimaryRateTier> FindPrimaryRateTiers(IPrimaryRateTierStore store, PrimaryRateTierQuery query)
    {
        return query.Apply(store.List());
    }
}
