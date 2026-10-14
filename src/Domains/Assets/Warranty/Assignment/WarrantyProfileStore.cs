namespace Acme.Workforce.Assets.Warranty.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty profiles by code.</summary>
public interface IWarrantyProfileStore
{
    WarrantyProfile? Find(string code);
    void Save(WarrantyProfile item);
    bool Remove(string code);
    List<WarrantyProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyProfileStore(Dictionary<string, WarrantyProfile> items, List<string> order) : IWarrantyProfileStore
{
    public Dictionary<string, WarrantyProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyProfile item)
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

    public List<WarrantyProfile> List()
    {
        var outValue = new List<WarrantyProfile>();
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
public sealed class CachedWarrantyProfileStore(MemoryWarrantyProfileStore inner, WarrantyProfileCache cache, int hits, int misses) : IWarrantyProfileStore
{
    public MemoryWarrantyProfileStore Inner { get; set; } = inner;
    public WarrantyProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyProfile? Find(string code)
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

    public void Save(WarrantyProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty profiles. Empty fields match all.</summary>
public sealed record WarrantyProfileQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyProfile> Apply(List<WarrantyProfile> items)
    {
        var matched = Collections.Filter(items, (WarrantyProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty profile store.</summary>
public static class WarrantyProfileStore
{
    public static IWarrantyProfileStore NewMemoryWarrantyProfileStore()
    {
        return new MemoryWarrantyProfileStore(new Dictionary<string, WarrantyProfile>(), new List<string>());
    }

    public static IWarrantyProfileStore NewCachedWarrantyProfileStore(MemoryWarrantyProfileStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyProfileStore(inner, WarrantyProfileCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantyProfile> FindWarrantyProfiles(IWarrantyProfileStore store, WarrantyProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
