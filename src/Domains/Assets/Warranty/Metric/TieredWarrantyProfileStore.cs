namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered warranty profiles by code.</summary>
public interface ITieredWarrantyProfileStore
{
    TieredWarrantyProfile? Find(string code);
    void Save(TieredWarrantyProfile item);
    bool Remove(string code);
    List<TieredWarrantyProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredWarrantyProfileStore(Dictionary<string, TieredWarrantyProfile> items, List<string> order) : ITieredWarrantyProfileStore
{
    public Dictionary<string, TieredWarrantyProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredWarrantyProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredWarrantyProfile item)
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

    public List<TieredWarrantyProfile> List()
    {
        var outValue = new List<TieredWarrantyProfile>();
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
public sealed class CachedTieredWarrantyProfileStore(MemoryTieredWarrantyProfileStore inner, TieredWarrantyProfileCache cache, int hits, int misses) : ITieredWarrantyProfileStore
{
    public MemoryTieredWarrantyProfileStore Inner { get; set; } = inner;
    public TieredWarrantyProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredWarrantyProfile? Find(string code)
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

    public void Save(TieredWarrantyProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredWarrantyProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered warranty profiles. Empty fields match all.</summary>
public sealed record TieredWarrantyProfileQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(TieredWarrantyProfile item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredWarrantyProfile> Apply(List<TieredWarrantyProfile> items)
    {
        var matched = Collections.Filter(items, (TieredWarrantyProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered warranty profile store.</summary>
public static class TieredWarrantyProfileStore
{
    public static ITieredWarrantyProfileStore NewMemoryTieredWarrantyProfileStore()
    {
        return new MemoryTieredWarrantyProfileStore(
            new Dictionary<string, TieredWarrantyProfile>(),
            new List<string>());
    }

    public static ITieredWarrantyProfileStore NewCachedTieredWarrantyProfileStore(MemoryTieredWarrantyProfileStore inner, int ttl, IClock clock)
    {
        return new CachedTieredWarrantyProfileStore(inner, TieredWarrantyProfileCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredWarrantyProfile> FindTieredWarrantyProfiles(ITieredWarrantyProfileStore store, TieredWarrantyProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
