namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary vendor profiles by code.</summary>
public interface IPrimaryVendorProfileStore
{
    PrimaryVendorProfile? Find(string code);
    void Save(PrimaryVendorProfile item);
    bool Remove(string code);
    List<PrimaryVendorProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryVendorProfileStore(Dictionary<string, PrimaryVendorProfile> items, List<string> order) : IPrimaryVendorProfileStore
{
    public Dictionary<string, PrimaryVendorProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryVendorProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryVendorProfile item)
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

    public List<PrimaryVendorProfile> List()
    {
        var outValue = new List<PrimaryVendorProfile>();
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
public sealed class CachedPrimaryVendorProfileStore(MemoryPrimaryVendorProfileStore inner, PrimaryVendorProfileCache cache, int hits, int misses) : IPrimaryVendorProfileStore
{
    public MemoryPrimaryVendorProfileStore Inner { get; set; } = inner;
    public PrimaryVendorProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryVendorProfile? Find(string code)
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

    public void Save(PrimaryVendorProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryVendorProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary vendor profiles. Empty fields match all.</summary>
public sealed record PrimaryVendorProfileQuery(string Title, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryVendorProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryVendorProfile> Apply(List<PrimaryVendorProfile> items)
    {
        var matched = Collections.Filter(items, (PrimaryVendorProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary vendor profile store.</summary>
public static class PrimaryVendorProfileStore
{
    public static IPrimaryVendorProfileStore NewMemoryPrimaryVendorProfileStore()
    {
        return new MemoryPrimaryVendorProfileStore(
            new Dictionary<string, PrimaryVendorProfile>(),
            new List<string>());
    }

    public static IPrimaryVendorProfileStore NewCachedPrimaryVendorProfileStore(MemoryPrimaryVendorProfileStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryVendorProfileStore(inner, PrimaryVendorProfileCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PrimaryVendorProfile> FindPrimaryVendorProfiles(IPrimaryVendorProfileStore store, PrimaryVendorProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
