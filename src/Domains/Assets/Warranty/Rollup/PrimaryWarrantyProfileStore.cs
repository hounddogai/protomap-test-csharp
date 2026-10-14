namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary warranty profiles by code.</summary>
public interface IPrimaryWarrantyProfileStore
{
    PrimaryWarrantyProfile? Find(string code);
    void Save(PrimaryWarrantyProfile item);
    bool Remove(string code);
    List<PrimaryWarrantyProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryWarrantyProfileStore(Dictionary<string, PrimaryWarrantyProfile> items, List<string> order) : IPrimaryWarrantyProfileStore
{
    public Dictionary<string, PrimaryWarrantyProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryWarrantyProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryWarrantyProfile item)
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

    public List<PrimaryWarrantyProfile> List()
    {
        var outValue = new List<PrimaryWarrantyProfile>();
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
public sealed class CachedPrimaryWarrantyProfileStore(MemoryPrimaryWarrantyProfileStore inner, PrimaryWarrantyProfileCache cache, int hits, int misses) : IPrimaryWarrantyProfileStore
{
    public MemoryPrimaryWarrantyProfileStore Inner { get; set; } = inner;
    public PrimaryWarrantyProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryWarrantyProfile? Find(string code)
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

    public void Save(PrimaryWarrantyProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryWarrantyProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary warranty profiles. Empty fields match all.</summary>
public sealed record PrimaryWarrantyProfileQuery(string Channel, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryWarrantyProfile item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryWarrantyProfile> Apply(List<PrimaryWarrantyProfile> items)
    {
        var matched = Collections.Filter(items, (PrimaryWarrantyProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary warranty profile store.</summary>
public static class PrimaryWarrantyProfileStore
{
    public static IPrimaryWarrantyProfileStore NewMemoryPrimaryWarrantyProfileStore()
    {
        return new MemoryPrimaryWarrantyProfileStore(
            new Dictionary<string, PrimaryWarrantyProfile>(),
            new List<string>());
    }

    public static IPrimaryWarrantyProfileStore NewCachedPrimaryWarrantyProfileStore(MemoryPrimaryWarrantyProfileStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryWarrantyProfileStore(
            inner,
            PrimaryWarrantyProfileCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryWarrantyProfile> FindPrimaryWarrantyProfiles(IPrimaryWarrantyProfileStore store, PrimaryWarrantyProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
