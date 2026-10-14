namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary asset return profiles by code.</summary>
public interface IPrimaryAssetReturnProfileStore
{
    PrimaryAssetReturnProfile? Find(string code);
    void Save(PrimaryAssetReturnProfile item);
    bool Remove(string code);
    List<PrimaryAssetReturnProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAssetReturnProfileStore(Dictionary<string, PrimaryAssetReturnProfile> items, List<string> order) : IPrimaryAssetReturnProfileStore
{
    public Dictionary<string, PrimaryAssetReturnProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAssetReturnProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAssetReturnProfile item)
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

    public List<PrimaryAssetReturnProfile> List()
    {
        var outValue = new List<PrimaryAssetReturnProfile>();
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
public sealed class CachedPrimaryAssetReturnProfileStore(MemoryPrimaryAssetReturnProfileStore inner, PrimaryAssetReturnProfileCache cache, int hits, int misses) : IPrimaryAssetReturnProfileStore
{
    public MemoryPrimaryAssetReturnProfileStore Inner { get; set; } = inner;
    public PrimaryAssetReturnProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAssetReturnProfile? Find(string code)
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

    public void Save(PrimaryAssetReturnProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAssetReturnProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary asset return profiles. Empty fields match all.</summary>
public sealed record PrimaryAssetReturnProfileQuery(string Category, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAssetReturnProfile item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAssetReturnProfile> Apply(List<PrimaryAssetReturnProfile> items)
    {
        var matched = Collections.Filter(items, (PrimaryAssetReturnProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary asset return profile store.</summary>
public static class PrimaryAssetReturnProfileStore
{
    public static IPrimaryAssetReturnProfileStore NewMemoryPrimaryAssetReturnProfileStore()
    {
        return new MemoryPrimaryAssetReturnProfileStore(
            new Dictionary<string, PrimaryAssetReturnProfile>(),
            new List<string>());
    }

    public static IPrimaryAssetReturnProfileStore NewCachedPrimaryAssetReturnProfileStore(MemoryPrimaryAssetReturnProfileStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAssetReturnProfileStore(
            inner,
            PrimaryAssetReturnProfileCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryAssetReturnProfile> FindPrimaryAssetReturnProfiles(IPrimaryAssetReturnProfileStore store, PrimaryAssetReturnProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
